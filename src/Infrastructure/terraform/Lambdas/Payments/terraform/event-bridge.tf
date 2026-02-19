
resource "aws_cloudwatch_event_rule" "stripe_rule" {
  name          = "stripe-subscriptions-rule"
  event_bus_name = "aws.partner/stripe.com/ed_test_61U8OhWNACpdhq6GG16U8LJp90CQBOsqL618O7RCiVVA"
  
  event_pattern = jsonencode({
    "source": [
      { "prefix": "aws.partner/stripe.com" }
    ],
    "detail-type": [ 
      "checkout.session.completed",
      "invoice.paid",
      "invoice.payment_failed",
      "customer.subscription.deleted",
      "customer.subscription.updated"
    ]
    })
}

resource "aws_cloudwatch_event_target" "stripe_lambda_target" {
  rule      = aws_cloudwatch_event_rule.stripe_rule.name
  event_bus_name = "aws.partner/stripe.com/ed_test_61U8OhWNACpdhq6GG16U8LJp90CQBOsqL618O7RCiVVA"
  target_id = "stripeLambda"
  arn       = aws_lambda_function.stripe_lambda.arn
}

resource "aws_lambda_permission" "allow_eventbridge" {
  statement_id  = "AllowExecutionFromEventBridge"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.stripe_lambda.function_name
  principal     = "events.amazonaws.com"
  source_arn    = aws_cloudwatch_event_rule.stripe_rule.arn
}
