resource "aws_ssm_parameter" "Verification_Email_Template" {
  name = "${var.ssm_namespace}VerificationEmailTemplate"
  type = "String"
  value = "test"
  lifecycle {
    ignore_changes = [value]
  }
  tags = local.tags
}

resource "aws_ssm_parameter" "Password_Reset_Email_Template" {
  name = "${var.ssm_namespace}RecoverPasswordEmailTemplate"
  type = "String"
  value = "test"
  lifecycle {
    ignore_changes = [value]
  }
  tags = local.tags
}

resource "aws_ssm_parameter" "Password_Change_Email_Template" {
  name = "${var.ssm_namespace}PasswordChangedEmailTemplate"
  type = "String"
  value = "test"
  lifecycle {
    ignore_changes = [value]
  }
  tags = local.tags
}

resource "aws_ssm_parameter" "User_Welcome_Email_Template" {
  name = "${var.ssm_namespace}UserWelcomeEmailTemplate"
  type = "String"
  value = "test"
  lifecycle {
    ignore_changes = [value]
  }
  overwrite = true
  tags = local.tags
}

resource "aws_ssm_parameter" "Subscription_Thank_You_Email_Template" {
  name = "${var.ssm_namespace}SubscriptionThankYouEmailTemplate"
  type = "String"
  value = "test"
  lifecycle {
    ignore_changes = [value]
  }
  tags = local.tags
  overwrite = true
}


resource "aws_ssm_parameter" "Subscription_Cancelled_Email_Template" {
  name = "${var.ssm_namespace}SubscriptionCancelledEmailTemplate"
  type = "String"
  value = "test"
  lifecycle {
    ignore_changes = [value]
  }
  tags = local.tags
}

resource "aws_ssm_parameter" "Subscription_Reactivated_Email_Template" {
  name = "${var.ssm_namespace}SubscriptionReactivatedEmailTemplate"
  type = "String"
  value = "test"
  lifecycle {
    ignore_changes = [value]
  }
  tags = local.tags
  overwrite = true
}

resource "aws_ssm_parameter" "Trial_Ending_Soon_Email_Template" {
  name = "${var.ssm_namespace}TrialEndingSoonEmailTemplate"
  type = "String"
  value = "test"
  lifecycle {
    ignore_changes = [value]
  }
  tags = local.tags
  overwrite = true
}
resource "aws_ssm_parameter" "Forgot_Password_Completed_Email_Template" {
  name = "${var.ssm_namespace}ForgotPasswordCompletedEmailTemplate"
  type = "String"
  value = "test"
  lifecycle {
    ignore_changes = [value]
  }
  tags = local.tags
}