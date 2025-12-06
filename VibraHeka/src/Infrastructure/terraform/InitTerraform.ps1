# setup-backend.ps1
param(
    [string]$BucketName = "vibraheka-tf",
    [string]$DynamoTable = "VibraHeka-tf",
    [string]$Region = "eu-west-1",
    [string]$Profile = "Twingers"
)

Write-Host "Setting up Terraform backend resources..." -ForegroundColor Green

# Configurar perfil
$env:AWS_PROFILE = $Profile

# Crear bucket S3 si no existe
$bucketExists = aws s3api head-bucket --bucket $BucketName 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Creating S3 bucket: $BucketName" -ForegroundColor Yellow

    if ($Region -eq "us-east-1") {
        aws s3api create-bucket --bucket $BucketName --region $Region
    } else {
        aws s3api create-bucket --bucket $BucketName --region $Region --create-bucket-configuration LocationConstraint=$Region
    }

    # Bloquear acceso público
    aws s3api put-public-access-block --bucket $BucketName --public-access-block-configuration BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true

    Write-Host "S3 bucket created and configured!" -ForegroundColor Green
} else {
    Write-Host "S3 bucket already exists: $BucketName" -ForegroundColor Blue
}

# Crear tabla DynamoDB si no existe (opcional, si no usas use_lockfile)
try {
    aws dynamodb describe-table --table-name $DynamoTable --region $Region > $null 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "DynamoDB table already exists: $DynamoTable" -ForegroundColor Blue
    }
} catch {
    Write-Host "Creating DynamoDB table: $DynamoTable" -ForegroundColor Yellow
    aws dynamodb create-table `
        --table-name $DynamoTable `
        --attribute-definitions AttributeName=LockID,AttributeType=S `
        --key-schema AttributeName=LockID,KeyType=HASH `
        --billing-mode PAY_PER_REQUEST `
        --region $Region

    Write-Host "DynamoDB table created!" -ForegroundColor Green
}

Write-Host "Backend setup complete! You can now run 'terraform init'" -ForegroundColor Green