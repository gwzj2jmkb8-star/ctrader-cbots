variable "aws_region" {
  description = "AWS region for the unit."
  type        = string
  default     = "eu-west-1"
}

variable "unit_id" {
  description = "Stable unit identifier used for names and registration tags. Lowercase letters, digits and dashes only."
  type        = string
  default     = "ether-poison-xiv"
  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{2,30}$", var.unit_id))
    error_message = "Use 3-31 lowercase letters, digits and dashes, starting with a letter."
  }
}

variable "owner_tag" {
  description = "Non-secret owner/team label for resource inventory."
  type        = string
  default     = "mati"
}

variable "environment" {
  description = "Unit environment name."
  type        = string
  default     = "demo"
  validation {
    condition     = contains(["demo", "staging", "live"], var.environment)
    error_message = "Choose demo, staging, or live."
  }
}

variable "image_tag" {
  description = "Immutable pushed ECR image tag for a compiled and reviewed .algo."
  type        = string
  default     = "candidate"
}

variable "desired_count" {
  description = "Runtime count. Defaults to zero; at most one instance for this unit."
  type        = number
  default     = 0
  validation {
    condition     = contains([0, 1], var.desired_count)
    error_message = "Only zero or one task is supported."
  }
}

variable "release_approved" {
  description = "Explicit operator acknowledgment after compilation, backtests, secret setup, and image push."
  type        = bool
  default     = false
}

variable "live_trading_approved" {
  description = "Second explicit gate if environment is live. Never set during initial demo deployment."
  type        = bool
  default     = false
}

variable "ctid" {
  description = "cTrader ID (identifier; stored in task definition)."
  type        = string
  default     = ""
}

variable "account_id" {
  description = "Dedicated cTrader account identifier, preferably demo for this initial unit."
  type        = string
  default     = ""
}

variable "symbol" {
  description = "Broker symbol, including any broker-specific suffix."
  type        = string
  default     = "XAUUSD"
}

variable "period" {
  description = "cTrader CLI period token."
  type        = string
  default     = "h4"
}

variable "task_cpu" {
  type    = number
  default = 1024
}

variable "task_memory" {
  type    = number
  default = 2048
}
