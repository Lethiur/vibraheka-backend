
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
  domain = "vibraheka.com"
  mail_from_domain = "mail.vibraheka.com"
}

output "ses_verification_token" {
  value       = aws_ses_domain_identity.VibraHeka_ses_domain.verification_token
  description = "Crea un registro TXT con nombre _amazonses.tu-dominio.com y este valor"
}

output "ses_dkim_tokens" {
  value       = aws_ses_domain_dkim.VibraHeka_ses_dkim.dkim_tokens
  description = "Crea 3 registros CNAME usando estos tokens"
}