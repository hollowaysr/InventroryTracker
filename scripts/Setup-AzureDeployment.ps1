# RFID Inventory Tracker - Azure Deployment Setup Script
# This script helps configure Azure resources and GitHub secrets for deployment

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("staging", "production")]
    [string]$Environment,
    
    [Parameter(Mandatory=$true)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory=$true)]
    [string]$TestAppsUsername,
    
    [Parameter(Mandatory=$true)]
    [SecureString]$TestAppsPassword,
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupPrefix = "rg-rfid-inventory-tracker"
)

# Convert SecureString to plain text for connection string
$TestAppsPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($TestAppsPassword))

Write-Host "🚀 RFID Inventory Tracker - Azure Setup Script" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""

# 1. Azure Login and Subscription Selection
Write-Host "1️⃣ Azure Authentication" -ForegroundColor Yellow
Write-Host "Logging into Azure..." -ForegroundColor White
az login

Write-Host "Setting subscription: $SubscriptionId" -ForegroundColor White
az account set --subscription $SubscriptionId

# Verify login
$currentAccount = az account show --query "user.name" -o tsv
Write-Host "✅ Logged in as: $currentAccount" -ForegroundColor Green
Write-Host ""

# 2. Create Service Principal
Write-Host "2️⃣ Service Principal Creation" -ForegroundColor Yellow
$spName = "rfid-inventory-tracker-$Environment"
Write-Host "Creating service principal: $spName" -ForegroundColor White

$sp = az ad sp create-for-rbac --name $spName --role contributor --scopes "/subscriptions/$SubscriptionId" --sdk-auth | ConvertFrom-Json

Write-Host "✅ Service principal created successfully!" -ForegroundColor Green
Write-Host ""

# 3. Test TestApps Database Connection
Write-Host "3️⃣ Database Connection Test" -ForegroundColor Yellow
$connectionString = "Server=heccdbs.database.windows.net;Database=TestApps;User ID=$TestAppsUsername;Password=$TestAppsPasswordPlain;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

Write-Host "Testing connection to TestApps database..." -ForegroundColor White
try {
    # Test database connection using .NET test project
    $env:ConnectionStrings__DefaultConnection = $connectionString
    $testResult = dotnet run --project TestDatabaseConnection 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Database connection successful!" -ForegroundColor Green
    } else {
        Write-Host "❌ Database connection failed!" -ForegroundColor Red
        Write-Host "Error: $testResult" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Database connection test failed: $_" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 4. Create Terraform Backend (if needed)
Write-Host "4️⃣ Terraform Backend Setup" -ForegroundColor Yellow
$tfResourceGroup = "tfstate-rg"
$tfStorageAccount = "tfstate$(Get-Random -Minimum 100000 -Maximum 999999)"
$tfContainer = "tfstate"

Write-Host "Checking if Terraform backend exists..." -ForegroundColor White
$rgExists = az group exists --name $tfResourceGroup
if ($rgExists -eq "false") {
    Write-Host "Creating Terraform backend resources..." -ForegroundColor White
    
    # Create resource group
    az group create --name $tfResourceGroup --location "East US"
    
    # Create storage account
    az storage account create `
        --name $tfStorageAccount `
        --resource-group $tfResourceGroup `
        --location "East US" `
        --sku Standard_LRS `
        --encryption-services blob
    
    # Create container
    az storage container create `
        --name $tfContainer `
        --account-name $tfStorageAccount `
        --auth-mode login
    
    Write-Host "✅ Terraform backend created successfully!" -ForegroundColor Green
} else {
    Write-Host "✅ Terraform backend already exists!" -ForegroundColor Green
    # Get existing storage account
    $tfStorageAccount = az storage account list --resource-group $tfResourceGroup --query "[0].name" -o tsv
}
Write-Host ""

# 5. Generate GitHub Secrets
Write-Host "5️⃣ GitHub Secrets Configuration" -ForegroundColor Yellow
Write-Host "==================================" -ForegroundColor White
Write-Host ""

Write-Host "📋 Copy the following secrets to your GitHub repository:" -ForegroundColor Cyan
Write-Host "(Settings → Secrets and variables → Actions)" -ForegroundColor Gray
Write-Host ""

Write-Host "🔐 AZURE_CREDENTIALS_$($Environment.ToUpper())" -ForegroundColor White
Write-Host "Value:" -ForegroundColor Gray
$sp | ConvertTo-Json -Compress
Write-Host ""

Write-Host "🗄️ TESTAPPS_CONNECTION_STRING" -ForegroundColor White
Write-Host "Value:" -ForegroundColor Gray
Write-Host $connectionString
Write-Host ""

Write-Host "🏗️ TF_STATE_RESOURCE_GROUP" -ForegroundColor White
Write-Host "Value:" -ForegroundColor Gray
Write-Host $tfResourceGroup
Write-Host ""

Write-Host "🏗️ TF_STATE_STORAGE_ACCOUNT" -ForegroundColor White
Write-Host "Value:" -ForegroundColor Gray
Write-Host $tfStorageAccount
Write-Host ""

Write-Host "🔑 SQL_ADMIN_USERNAME" -ForegroundColor White
Write-Host "Value:" -ForegroundColor Gray
Write-Host $TestAppsUsername
Write-Host ""

Write-Host "🔑 SQL_ADMIN_PASSWORD" -ForegroundColor White
Write-Host "Value:" -ForegroundColor Gray
Write-Host $TestAppsPasswordPlain
Write-Host ""

# 6. Generate terraform.tfvars
Write-Host "6️⃣ Terraform Configuration" -ForegroundColor Yellow
$tfVarsPath = "infrastructure/terraform.tfvars"
$tfVarsContent = @"
# Terraform variables for $Environment environment
environment = "$Environment"
location = "East US"
app_name = "rfid-inventory-tracker"

# TestApps Database Configuration
testapps_db_server = "heccdbs.database.windows.net"
testapps_db_name = "TestApps"
testapps_db_username = "$TestAppsUsername"
testapps_db_password = "$TestAppsPasswordPlain"
"@

Write-Host "Creating terraform.tfvars file..." -ForegroundColor White
$tfVarsContent | Out-File -FilePath $tfVarsPath -Encoding UTF8 -Force
Write-Host "✅ Created: $tfVarsPath" -ForegroundColor Green
Write-Host ""

# 7. Next Steps
Write-Host "7️⃣ Next Steps" -ForegroundColor Yellow
Write-Host "===============" -ForegroundColor White
Write-Host ""
Write-Host "1. 📋 Add the secrets shown above to your GitHub repository" -ForegroundColor White
Write-Host "2. 🌍 Create GitHub environments: staging, production, staging-infrastructure, production-infrastructure" -ForegroundColor White
Write-Host "3. 🚀 Run the GitHub Actions workflows:" -ForegroundColor White
Write-Host "   - Infrastructure Deployment (deploy infrastructure first)" -ForegroundColor White
Write-Host "   - Application CI/CD (deploy application code)" -ForegroundColor White
Write-Host ""
Write-Host "4. 🔍 Monitor deployment in:" -ForegroundColor White
Write-Host "   - GitHub Actions tab in your repository" -ForegroundColor White
Write-Host "   - Azure Portal for resource monitoring" -ForegroundColor White
Write-Host ""

Write-Host "🎉 Setup completed successfully!" -ForegroundColor Green
Write-Host "Your RFID Inventory Tracker is ready for deployment to Azure!" -ForegroundColor Green

# Clean up sensitive variables
$TestAppsPasswordPlain = $null
$connectionString = $null
[System.GC]::Collect()
