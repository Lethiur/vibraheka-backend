
# Virtual Deliverability Manager (VDM) — account-level configuration.
# Applied only from the "main" workspace (shared resource, same guard as domain identity).
# aws_sesv2_account_vdm_attributes is a singleton per AWS account;
# count=0 in non-main workspaces avoids touching the shared setting.

resource "aws_sesv2_account_vdm_attributes" "VibraHeka_ses_vdm" {
  count = local.manage_shared_ses ? 1 : 0
  vdm_enabled = "ENABLED"
  
  dashboard_attributes {
    engagement_metrics = "ENABLED"
  }

  guardian_attributes {
    optimized_shared_delivery = "ENABLED"
  }
}

