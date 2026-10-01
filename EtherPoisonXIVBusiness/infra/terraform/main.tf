data "aws_availability_zones" "available" {
  state = "available"
}

data "aws_caller_identity" "current" {}

locals {
  name = "${var.unit_id}-${var.environment}"
  tags = {
    BusinessUnit = var.unit_id
    Owner        = var.owner_tag
    Environment  = var.environment
    ManagedBy    = "terraform"
    Workload     = "ctrader-cbot"
  }
}

resource "aws_vpc" "unit" {
  cidr_block           = "10.72.0.0/16"
  enable_dns_support   = true
  enable_dns_hostnames = true
  tags                 = { Name = "${local.name}-vpc" }
}

resource "aws_internet_gateway" "unit" {
  vpc_id = aws_vpc.unit.id
  tags   = { Name = "${local.name}-igw" }
}

resource "aws_subnet" "public" {
  count                   = 2
  vpc_id                  = aws_vpc.unit.id
  cidr_block              = cidrsubnet(aws_vpc.unit.cidr_block, 8, count.index)
  availability_zone       = data.aws_availability_zones.available.names[count.index]
  map_public_ip_on_launch = false
  tags                    = { Name = "${local.name}-public-${count.index + 1}" }
}

resource "aws_route_table" "public" {
  vpc_id = aws_vpc.unit.id
  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.unit.id
  }
}

resource "aws_route_table_association" "public" {
  count          = 2
  subnet_id      = aws_subnet.public[count.index].id
  route_table_id = aws_route_table.public.id
}

resource "aws_security_group" "task" {
  name_prefix = "${local.name}-task-"
  vpc_id      = aws_vpc.unit.id
  description = "Outbound-only cTrader CLI runtime"
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
  tags = { Name = "${local.name}-task" }
}

resource "aws_ecr_repository" "unit" {
  name                 = local.name
  image_tag_mutability = "IMMUTABLE"
  image_scanning_configuration { scan_on_push = true }
  encryption_configuration { encryption_type = "AES256" }
}

resource "aws_s3_bucket" "evidence" {
  bucket_prefix = "${local.name}-evidence-"
  force_destroy = false
}

resource "aws_s3_bucket_public_access_block" "evidence" {
  bucket                  = aws_s3_bucket.evidence.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_versioning" "evidence" {
  bucket = aws_s3_bucket.evidence.id
  versioning_configuration { status = "Enabled" }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "evidence" {
  bucket = aws_s3_bucket.evidence.id
  rule { apply_server_side_encryption_by_default { sse_algorithm = "AES256" } }
}

resource "aws_cloudwatch_log_group" "unit" {
  name              = "/units/${local.name}/ctrader"
  retention_in_days = 14
}

resource "aws_secretsmanager_secret" "ctrader_password" {
  name                    = "${local.name}/ctrader-id-password"
  description             = "Set the cTrader ID password outside Terraform; never put its value in tfvars/state."
  recovery_window_in_days = 7
}

data "aws_iam_policy_document" "ecs_assume" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type        = "Service"
      identifiers = ["ecs-tasks.amazonaws.com"]
    }
  }
}

resource "aws_iam_role" "execution" {
  name               = "${local.name}-execution"
  assume_role_policy = data.aws_iam_policy_document.ecs_assume.json
}

resource "aws_iam_role_policy_attachment" "execution" {
  role       = aws_iam_role.execution.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

resource "aws_iam_role_policy" "secret" {
  name = "ctrader-password-read"
  role = aws_iam_role.execution.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect   = "Allow"
      Action   = ["secretsmanager:GetSecretValue"]
      Resource = aws_secretsmanager_secret.ctrader_password.arn
    }]
  })
}

resource "aws_iam_role" "task" {
  name               = "${local.name}-task"
  assume_role_policy = data.aws_iam_policy_document.ecs_assume.json
}

resource "aws_ecs_cluster" "unit" {
  name = local.name
}

resource "aws_ecs_task_definition" "unit" {
  family                   = local.name
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = var.task_cpu
  memory                   = var.task_memory
  execution_role_arn       = aws_iam_role.execution.arn
  task_role_arn            = aws_iam_role.task.arn
  runtime_platform {
    operating_system_family = "LINUX"
    cpu_architecture        = "X86_64"
  }
  container_definitions = jsonencode([{
    name      = "ctrader"
    image     = "${aws_ecr_repository.unit.repository_url}:${var.image_tag}"
    essential = true
    environment = [
      { name = "CTID", value = var.ctid },
      { name = "ACCOUNT", value = var.account_id },
      { name = "SYMBOL", value = var.symbol },
      { name = "PERIOD", value = var.period }
    ]
    secrets = [{
      name      = "CTRADER_PASSWORD"
      valueFrom = aws_secretsmanager_secret.ctrader_password.arn
    }]
    logConfiguration = {
      logDriver = "awslogs"
      options = {
        awslogs-group         = aws_cloudwatch_log_group.unit.name
        awslogs-region        = var.aws_region
        awslogs-stream-prefix = "runtime"
      }
    }
  }])
}

resource "aws_ecs_service" "unit" {
  name            = local.name
  cluster         = aws_ecs_cluster.unit.id
  task_definition = aws_ecs_task_definition.unit.arn
  desired_count   = var.desired_count
  launch_type     = "FARGATE"
  network_configuration {
    subnets          = aws_subnet.public[*].id
    security_groups  = [aws_security_group.task.id]
    assign_public_ip = true
  }
  deployment_minimum_healthy_percent = 0
  deployment_maximum_percent         = 100

  lifecycle {
    precondition {
      condition = var.desired_count == 0 || (
        var.release_approved && (var.environment != "live" || var.live_trading_approved) &&
        var.ctid != "" && var.account_id != "" && var.image_tag != "candidate"
      )
      error_message = "Runtime needs release_approved, live_trading_approved for live, cTID, account ID, and a reviewed immutable image tag."
    }
  }
  depends_on = [aws_iam_role_policy_attachment.execution, aws_iam_role_policy.secret]
}
