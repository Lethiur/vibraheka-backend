
resource "aws_ses_event_destination" "VibraHeka-SES" {
  name                   = "cloudwatch-events"
  configuration_set_name = aws_ses_configuration_set.VibraHeka_ses_config.name
  enabled                = true
  matching_types         = [
    "send",
    "delivery",
    "open",
    "click",
    "bounce",
    "complaint"
  ]

  cloudwatch_destination {
    default_value = "tracking-config"
    dimension_name = "ses:configuration-set"
    value_source = "emailHeader"
  }
}
