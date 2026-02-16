
resource "aws_iam_policy" "stripe_put_events" {
  name        = "StripeEventBridgePolicy-${terraform.workspace}"
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


# Policy para DynamoDB

resource "aws_iam_policy" "stripe_lambda_dynamodb_policy" {
  name        = "StripeLambdaDynamoDBPolicy-${terraform.workspace}"
  description = "Permite a la Lambda leer y escribir en la tabla DynamoDB específica"
  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect = "Allow",
        Action = [
          "dynamodb:GetItem",
          "dynamodb:BatchGetItem",
          "dynamodb:Query",
          "dynamodb:Scan",
          "dynamodb:PutItem",
          "dynamodb:UpdateItem",
          "dynamodb:DeleteItem"
        ],
        Resource = var.dynamodb_table_arn
      }
    ]
  })
}
