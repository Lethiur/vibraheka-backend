
output "lambda_function_arn" {
  value = aws_lambda_function.stripe_lambda.arn
}

output "event_rule_name" {
  value = aws_cloudwatch_event_rule.stripe_rule.name
}
