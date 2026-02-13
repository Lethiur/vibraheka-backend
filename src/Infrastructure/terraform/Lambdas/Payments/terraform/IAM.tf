
resource "aws_iam_policy" "stripe_put_events" {
  name        = "StripeEventBridgePolicy"
  description = "Allow Stripe to put events on EventBridge"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action   = "events:PutEvents"
        Resource = var.stripe_event_bus_arn
      }
    ]
  })
}
