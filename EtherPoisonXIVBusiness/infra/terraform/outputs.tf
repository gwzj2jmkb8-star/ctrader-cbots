output "unit_registration" {
  value = {
    unit_id        = var.unit_id
    owner          = var.owner_tag
    environment    = var.environment
    aws_account_id = data.aws_caller_identity.current.account_id
    aws_region     = var.aws_region
    cluster        = aws_ecs_cluster.unit.name
    service        = aws_ecs_service.unit.name
  }
}

output "ecr_repository_url" { value = aws_ecr_repository.unit.repository_url }
output "evidence_bucket" { value = aws_s3_bucket.evidence.bucket }
output "ctrader_password_secret_arn" { value = aws_secretsmanager_secret.ctrader_password.arn }
output "log_group" { value = aws_cloudwatch_log_group.unit.name }
