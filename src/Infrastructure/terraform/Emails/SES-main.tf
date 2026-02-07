
# 1. Definir la identidad del dominio
resource "aws_ses_domain_identity" "VibraHeka_ses_domain" {
  domain = "vibraheka.com"
}

# 2. Configurar DKIM
resource "aws_ses_domain_dkim" "VibraHeka_ses_dkim" {
  domain = aws_ses_domain_identity.VibraHeka_ses_domain.domain
}

resource "aws_ses_configuration_set" "VibraHeka_ses_config" {
  name = "VibraHeka-ses-config"
}

resource "aws_ses_domain_mail_from" "VibraHeka_ses_tracking" {
  domain = aws_ses_domain_identity.VibraHeka_ses_domain.domain
  mail_from_domain = "mail.vibraheka.com"
}

output "ses_config_arn" {
  value = aws_ses_configuration_set.VibraHeka_ses_config.arn
}

output "ses_config_name" {
  value = aws_ses_configuration_set.VibraHeka_ses_config.name
}

output "ses_email_from_domain" {
  value = aws_ses_domain_mail_from.VibraHeka_ses_tracking.mail_from_domain
}

output "ses_email_domain_arn" {
  value = aws_ses_domain_identity.VibraHeka_ses_domain.arn
}