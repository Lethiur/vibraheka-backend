resource "aws_iam_role_policy_attachment" "VH_email_lambda_logs" {
  role       = aws_iam_role.VH_email_lambda_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_iam_policy_attachment" "PAM_lambda_kms_policy_attach" {
  name       = "send-email-lambda_kms_policy_attachment-${terraform.workspace}"
  roles      = [aws_iam_role.VH_email_lambda_role.name]
  policy_arn = aws_iam_policy.kms_policy.arn
}

resource "aws_lambda_permission" "allow_cognito_create_challenge" {
  statement_id  = "AllowExecutionFromCognito"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.send_email_cognito.function_name
  principal     = "cognito-idp.amazonaws.com"
  source_arn    = var.user_pool_arn
}

resource "aws_lambda_permission" "allow_notification_eventbridge" {
  statement_id  = "AllowExecutionFromNotificationEventBridge"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.send_email_notifications.function_name
  principal     = "events.amazonaws.com"
  source_arn    = aws_cloudwatch_event_rule.notification_email_requested.arn
}
