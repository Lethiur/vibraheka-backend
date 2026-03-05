param(
    [string]$TerraformDir = "src/Infrastructure/terraform",
    [string]$Workspace = "",
    [string]$AppSettingsPath = "",
    [string]$PasswordResetTokenSecret = "",
    [string[]]$AppSettingsPaths = @(
        "src/Web/appsettings.json",
        "src/Web/appsettings.Development.json",
        "src/Web/appsettings.Test.json"
    )
)

$ErrorActionPreference = "Stop"

function Set-PropertyIfPresent {
    param(
        [object]$Outputs,
        [object]$Target,
        [string]$OutputName,
        [string]$PropertyName
    )

    function Set-TargetValue {
        param(
            [object]$InnerTarget,
            [string]$InnerPropertyName,
            [object]$InnerValue
        )

        if ($InnerTarget -is [hashtable]) {
            $InnerTarget[$InnerPropertyName] = $InnerValue
            return
        }

        $existingProperty = $InnerTarget.PSObject.Properties[$InnerPropertyName]
        if ($null -eq $existingProperty) {
            $InnerTarget | Add-Member -MemberType NoteProperty -Name $InnerPropertyName -Value $InnerValue
        }
        else {
            $InnerTarget.$InnerPropertyName = $InnerValue
        }
    }

    if ($Outputs -is [hashtable]) {
        if ($Outputs.ContainsKey($OutputName) -and $null -ne $Outputs[$OutputName] -and $null -ne $Outputs[$OutputName].value) {
            Set-TargetValue -InnerTarget $Target -InnerPropertyName $PropertyName -InnerValue $Outputs[$OutputName].value
        }
        return
    }

    $outputProperty = $Outputs.PSObject.Properties[$OutputName]
    if ($null -ne $outputProperty -and $null -ne $outputProperty.Value -and $null -ne $outputProperty.Value.value) {
        Set-TargetValue -InnerTarget $Target -InnerPropertyName $PropertyName -InnerValue $outputProperty.Value.value
    }
}

Push-Location $TerraformDir
try {
    if (-not [string]::IsNullOrWhiteSpace($Workspace)) {
        terraform workspace select $Workspace | Out-Null
    }

    $rawOutputs = terraform output -json
}
finally {
    Pop-Location
}

if ($PSVersionTable.PSVersion.Major -ge 6) {
    $outputs = $rawOutputs | ConvertFrom-Json -AsHashtable
}
else {
$outputs = $rawOutputs | ConvertFrom-Json
}

if ([string]::IsNullOrWhiteSpace($PasswordResetTokenSecret) -and -not [string]::IsNullOrWhiteSpace($env:PASSWORD_RESET_TOKEN_SECRET)) {
    $PasswordResetTokenSecret = $env:PASSWORD_RESET_TOKEN_SECRET
}

if (-not [string]::IsNullOrWhiteSpace($AppSettingsPath)) {
    $AppSettingsPaths += $AppSettingsPath
}

$AppSettingsPaths = $AppSettingsPaths | Select-Object -Unique

foreach ($path in $AppSettingsPaths) {
    if (-not (Test-Path $path)) {
        throw "AppSettings file not found at path: $path"
    }

    $jsonText = Get-Content -Path $path -Raw
    $settings = $jsonText | ConvertFrom-Json

    if ($null -eq $settings.AWS) {
        $settings | Add-Member -MemberType NoteProperty -Name AWS -Value ([pscustomobject]@{})
    }

    if ($null -eq $settings.Backend) {
        $settings | Add-Member -MemberType NoteProperty -Name Backend -Value ([pscustomobject]@{})
    }

    Set-PropertyIfPresent -Outputs $outputs -Target $settings.AWS -OutputName "users_table_name" -PropertyName "UsersTable"
    Set-PropertyIfPresent -Outputs $outputs -Target $settings.AWS -OutputName "verification_codes_table_name" -PropertyName "CodesTable"
    Set-PropertyIfPresent -Outputs $outputs -Target $settings.AWS -OutputName "email_templates_table_name" -PropertyName "EmailTemplatesTable"
    Set-PropertyIfPresent -Outputs $outputs -Target $settings.AWS -OutputName "email_templates_bucket_name" -PropertyName "EmailTemplatesBucketName"
    Set-PropertyIfPresent -Outputs $outputs -Target $settings.AWS -OutputName "action_log_table_name" -PropertyName "ActionLogTable"
    Set-PropertyIfPresent -Outputs $outputs -Target $settings.AWS -OutputName "subscriptions_table_name" -PropertyName "SubscriptionTable"
    Set-PropertyIfPresent -Outputs $outputs -Target $settings.AWS -OutputName "subscriptions_table_user_index_name" -PropertyName "SubscriptionUserIdIndex"
    Set-PropertyIfPresent -Outputs $outputs -Target $settings.AWS -OutputName "cognito_client_id" -PropertyName "ClientId"
    Set-PropertyIfPresent -Outputs $outputs -Target $settings.AWS -OutputName "cognito_pool_id" -PropertyName "UserPoolId"
    Set-PropertyIfPresent -Outputs $outputs -Target $settings.AWS -OutputName "backend_api_gateway_base_route" -PropertyName "ApiGatewayBaseUrl"
    Set-PropertyIfPresent -Outputs $outputs -Target $settings.AWS -OutputName "settings_namespace" -PropertyName "SettingsNameSpace"
    if (-not [string]::IsNullOrWhiteSpace($PasswordResetTokenSecret)) {
        if ($settings.AWS.PSObject.Properties["PasswordResetTokenSecret"]) {
            $settings.AWS.PasswordResetTokenSecret = $PasswordResetTokenSecret
        }
        else {
            $settings.AWS | Add-Member -MemberType NoteProperty -Name PasswordResetTokenSecret -Value $PasswordResetTokenSecret
        }
    }

    Set-PropertyIfPresent -Outputs $outputs -Target $settings.Backend -OutputName "backend_api_gateway_base_route" -PropertyName "ApiGatewayBaseUrl"
    Set-PropertyIfPresent -Outputs $outputs -Target $settings.Backend -OutputName "backend_api_gateway_endpoint" -PropertyName "ApiGatewayEndpoint"
    Set-PropertyIfPresent -Outputs $outputs -Target $settings.Backend -OutputName "backend_ecr_repository_url" -PropertyName "EcrRepositoryUrl"

    $updatedJson = $settings | ConvertTo-Json -Depth 20
    Set-Content -Path $path -Value $updatedJson
    Write-Host "Updated appsettings file: $path"
}
