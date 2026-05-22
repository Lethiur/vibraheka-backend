output "ssm_email_verification_template_id_parameter_name"{
  value = aws_ssm_parameter.Verification_Email_Template.name
}

output "ssm_email_password_reset_template_id_parameter_name"{
  value = aws_ssm_parameter.Password_Reset_Email_Template.name
}

output "ssm_subscription_thank_you_template_id_parameter_name" {
  value = aws_ssm_parameter.Subscription_Thank_You_Email_Template.name
}

output "ssm_trial_ending_soon_template_id_parameter_name" {
  value = aws_ssm_parameter.Trial_Ending_Soon_Email_Template.name
}

output "ssm_user_welcome_template_id_parameter_name" {
  value = aws_ssm_parameter.User_Welcome_Email_Template.name
}

output "ssm_forgot_password_completed_template_parameter_name" {
  value = aws_ssm_parameter.Forgot_Password_Completed_Email_Template.name
}
output "ssm_subscription_cancelled_template_id_parameter_name" {
  value = aws_ssm_parameter.Subscription_Cancelled_Email_Template.name
}

output "ssm_subscription_reactivated_template_id_parameter_name" {
  value = aws_ssm_parameter.Subscription_Reactivated_Email_Template.name
}

output "ssm_read_vh_parameters_policy_arn"{
  value = aws_iam_policy.SSM_Policy.arn
}