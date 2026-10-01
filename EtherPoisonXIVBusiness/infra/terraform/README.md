# Terraform deployment — XIV unit on AWS

This stack registers `ether-poison-xiv` in AWS inventory through resource names, default tags, and the `unit_registration` output. It provisions an ECS Fargate service at **zero running tasks** by default, an immutable ECR repository, an encrypted versioned private evidence bucket, a CloudWatch log group, a Secrets Manager secret shell, and an outbound-only VPC task network. It does **not** register a legal company or a cTrader Store product.

## Prerequisites

- AWS credentials scoped to your own AWS account, Terraform >= 1.6, AWS CLI and Docker on the operator machine.
- A cTrader ID, dedicated demo account ID, actual broker symbol and a successful cTrader `.algo` build and backtests.
- A remote Terraform backend configured by the operator for team use. The evidence bucket made by this stack cannot bootstrap its own backend. Local Terraform state may contain cTID and account identifiers; keep it private. The cTrader password value is never entered through Terraform.

## 1. Create infrastructure in the paused state

From `infra/terraform`:

```sh
cp terraform.tfvars.example terraform.tfvars
terraform init
terraform fmt -check
terraform validate
terraform plan -out=paused.tfplan
terraform apply paused.tfplan
terraform output unit_registration
terraform output -raw ecr_repository_url
terraform output -raw ctrader_password_secret_arn
```

These actions create billable AWS resources, though the ECS service runs zero tasks. Review the plan and costs in your AWS account before applying.

## 2. Build, record, and package a reviewed runtime

Build the cBot in cTrader Algo/SDK, run the evidence gate in the project root, and copy the reviewed `.algo` to `deploy/artifacts/EtherPoisonXIVBusiness.algo`. The cTrader Console base is pinned to Spotware's stable `5.9.11` tag. From the project root, authenticate Docker to ECR for the selected region, then build and push a unique tag:

```sh
aws ecr get-login-password --region eu-west-1 | docker login --username AWS --password-stdin ACCOUNT_ID.dkr.ecr.eu-west-1.amazonaws.com
docker build -f deploy/Dockerfile -t ACCOUNT_ID.dkr.ecr.eu-west-1.amazonaws.com/ether-poison-xiv-demo:reviewed-v0-1-0 deploy
docker push ACCOUNT_ID.dkr.ecr.eu-west-1.amazonaws.com/ether-poison-xiv-demo:reviewed-v0-1-0
```

Replace `ACCOUNT_ID`, region and tag from your Terraform output. The repository rejects overwriting a tag. Keep the `.algo` checksum, ECR image digest, and parameter configuration with the release evidence.

## 3. Supply credentials without putting the password in state

Set the cTrader ID password as the value of the Terraform-created secret using the AWS CLI or console. Example with a locally protected one-line file:

```sh
aws secretsmanager put-secret-value --secret-id SECRET_ARN --secret-string file:///secure/path/ctrader-password.txt
```

The task receives the secret as an environment variable and writes a mode-600 password file on its ephemeral filesystem for the CLI. The file is discarded when the task is destroyed. Anyone who can inspect task environment, ECS execution settings, or logs should be treated as privileged. The cTID and account ID are plain task environment values, so avoid putting personal identifiers in the resource tags.

## 4. Activate only after review

Fill `ctid`, `account_id`, and a unique `image_tag` in `terraform.tfvars`. Set `release_approved = true` and `desired_count = 1` only after the specific `.algo`, image digest, broker account, symbol and evidence have been reviewed. The `live` environment also requires `live_trading_approved = true`. Run `terraform plan` and `terraform apply`. This is the step that starts automatic orders.

To pause new runtime work, set `desired_count = 0` and apply. Stopping a task does not guarantee broker pending orders or positions are gone: inspect them in cTrader and handle them explicitly. The service replaces failed tasks automatically when count is one. Review restarts, logs, order protection and account state.

## Limitations

- This is one task for one account/symbol/timeframe. Multiple units need separate state and labels.
- Public subnets provide outbound internet without a NAT gateway. There are **no inbound security-group rules**; the task may still receive a public IP for outbound connections.
- The runtime image is built only after platform compilation. The Terraform code does not compile C#, execute backtests, write a secret value, or guarantee broker connectivity.
- ECS task replacement can restart a bot with an existing basket. The cBot deliberately locks rescue after restart until flat. Verify order state before activation or rollback.
- `release_approved` is an operator flag, not a cryptographic verification of the release gate. Keep the report manifest and image digest in your release record.

References: [Spotware cTrader Console image](https://github.com/spotware/ctrader-console-docker), [Spotware CLI run reference](https://github.com/spotware/CLI-references), [AWS ECS task definition](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/ecs_task_definition).
