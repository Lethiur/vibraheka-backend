
# 1. Definir la identidad del dominio
resource "aws_ses_domain_identity" "VibraHeka_ses_domain" {
  domain = "www.vibraheka.com"
}

# 2. Configurar DKIM
resource "aws_ses_domain_dkim" "VibraHeka_ses_dkim" {
  domain = aws_ses_domain_identity.VibraHeka_ses_domain.domain
}

# 3. Output de los valores necesarios (Para verlos en tu terminal)
output "ses_verification_token" {
  value       = aws_ses_domain_identity.VibraHeka_ses_domain.verification_token
  description = "Crea un registro TXT con nombre _amazonses.tu-dominio.com y este valor"
}

output "ses_dkim_tokens" {
  value       = aws_ses_domain_dkim.VibraHeka_ses_dkim.dkim_tokens
  description = "Crea 3 registros CNAME usando estos tokens"
}