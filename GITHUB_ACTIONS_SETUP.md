# GitHub Actions Deployment Guide

This guide provides step-by-step instructions for setting up GitHub Actions pipelines to deploy the RFID Inventory Tracker application to Azure using the existing TestApps database.

## 🚀 Quick Start

### 1. Prerequisites Setup

Before running the GitHub Actions pipelines, ensure you have:

- **Azure Subscription** with appropriate permissions
- **TestApps Database Access** on `heccdbs.database.windows.net`
- **GitHub Repository** with admin access to configure secrets and environments

### 2. Azure Service Principal Creation

Create service principals for staging and production environments:

```powershell
# Login to Azure
az login

# Create service principal for staging
$stagingSP = az ad sp create-for-rbac --name "rfid-inventory-tracker-staging" --role contributor --scopes /subscriptions/{subscription-id} --sdk-auth | ConvertFrom-Json

# Create service principal for production
$productionSP = az ad sp create-for-rbac --name "rfid-inventory-tracker-production" --role contributor --scopes /subscriptions/{subscription-id} --sdk-auth | ConvertFrom-Json

# Display the service principal information
Write-Host "Staging Service Principal JSON:"
$stagingSP | ConvertTo-Json
Write-Host "`nProduction Service Principal JSON:"
$productionSP | ConvertTo-Json
```

## 🔧 GitHub Configuration

### 3. Repository Secrets Setup

Navigate to your GitHub repository → Settings → Secrets and variables → Actions, and add the following secrets:

#### 🔐 Azure Credentials
```
AZURE_CREDENTIALS_STAGING
AZURE_CREDENTIALS_PRODUCTION
```
Use the JSON output from the service principal creation step above.

#### 🗄️ Database Configuration
```
TESTAPPS_CONNECTION_STRING
```
Format: `Server=heccdbs.database.windows.net;Database=TestApps;User ID=<username>;Password=<password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`

#### 🏗️ Terraform Backend
```
TF_STATE_RESOURCE_GROUP
TF_STATE_STORAGE_ACCOUNT
SQL_ADMIN_USERNAME
SQL_ADMIN_PASSWORD
```

#### 📊 Monitoring
```
APPLICATION_INSIGHTS_CONNECTION_STRING_STAGING
APPLICATION_INSIGHTS_CONNECTION_STRING_PRODUCTION
```

### 4. GitHub Environments Setup

Create the following environments in your repository (Settings → Environments):

#### 🟡 Staging Environment
- **Name**: `staging`
- **Protection Rules**: 
  - Require pull request reviews
  - Allow administrators to bypass

#### 🔴 Production Environment
- **Name**: `production`
- **Protection Rules**:
  - Required reviewers (add DevOps team members)
  - Require pull request reviews
  - Allow administrators to bypass

#### 🏗️ Infrastructure Environments
Create these additional environments for infrastructure deployment:
- `staging-infrastructure`
- `production-infrastructure`
- `staging-infrastructure-destroy`
- `production-infrastructure-destroy`

## 🔄 Pipeline Overview

### Infrastructure Pipeline (`.github/workflows/infrastructure.yml`)

**Triggers:**
- Push to `main` or `develop` branches (infrastructure changes only)
- Manual dispatch with environment selection
- Pull requests to `main` (infrastructure changes only)

**Jobs:**
1. **Terraform Validate** - Validates Terraform syntax and security
2. **Terraform Plan** - Creates deployment plans for staging/production
3. **Terraform Apply** - Deploys infrastructure to Azure
4. **Terraform Destroy** - (Manual only) Destroys infrastructure

**Key Features:**
- Uses existing TestApps database (no new database creation)
- Stores secrets in Azure Key Vault
- Configures Application Insights monitoring
- Sets up health checks and alerts

### Application Pipeline (`.github/workflows/ci-cd.yml`)

**Triggers:**
- Push to `main` (production deployment) or `develop` (staging deployment)
- Manual dispatch with environment selection
- Pull requests to `main` (build and test only)

**Jobs:**
1. **Build and Test** - Compiles application and runs unit tests
2. **Security Scan** - CodeQL analysis and dependency scanning
3. **Validate TestApps Connection** - Tests database connectivity
4. **Deploy to Staging/Production** - Deploys to Azure App Service
5. **Verify Deployment** - Post-deployment validation

## 🎯 Deployment Workflow

### First-Time Setup

1. **Setup Terraform Backend**
   ```
   Go to Actions → Setup Environment → Run workflow
   Select "setup-terraform-backend"
   ```

2. **Validate Secrets**
   ```
   Go to Actions → Setup Environment → Run workflow
   Select "validate-secrets"
   ```

3. **Test Database Connection**
   ```
   Go to Actions → Setup Environment → Run workflow
   Select "test-testapps-connection"
   ```

### Infrastructure Deployment

1. **Deploy Staging Infrastructure**
   ```
   Go to Actions → Infrastructure Deployment → Run workflow
   Environment: staging
   Action: apply
   ```

2. **Deploy Production Infrastructure**
   ```
   Go to Actions → Infrastructure Deployment → Run workflow
   Environment: production
   Action: apply
   ```

### Application Deployment

1. **Staging Deployment**
   - Push to `develop` branch, or
   - Manual dispatch with environment: staging

2. **Production Deployment**
   - Push to `main` branch, or
   - Manual dispatch with environment: production

## 🛠️ Manual Deployment Commands

If you prefer to deploy manually, use these PowerShell commands:

### Infrastructure Deployment
```powershell
# Navigate to infrastructure directory
Set-Location "infrastructure"

# Login to Azure
az login

# Initialize Terraform
terraform init `
  -backend-config="resource_group_name=your-tfstate-rg" `
  -backend-config="storage_account_name=your-tfstate-account" `
  -backend-config="container_name=tfstate" `
  -backend-config="key=rfid-inventory-tracker-staging.terraform.tfstate"

# Create terraform.tfvars
@"
environment = "staging"
location = "East US"
app_name = "rfid-inventory-tracker"
testapps_db_server = "heccdbs.database.windows.net"
testapps_db_name = "TestApps"
testapps_db_username = "your-username"
testapps_db_password = "your-password"
"@ | Out-File -FilePath "terraform.tfvars" -Encoding UTF8

# Plan and apply
terraform plan -var-file="terraform.tfvars"
terraform apply -var-file="terraform.tfvars"
```

### Application Deployment
```powershell
# Build and publish application
dotnet publish InventoryTracker.Web -c Release -o ./publish

# Create deployment package
Compress-Archive -Path "./publish/*" -DestinationPath "deployment.zip" -Force

# Deploy to Azure App Service
az webapp deployment source config-zip `
  --name "app-rfid-inventory-tracker-staging" `
  --resource-group "rg-rfid-inventory-tracker-staging" `
  --src "deployment.zip"
```

## 🔍 Monitoring and Troubleshooting

### Health Checks
- **Application Health**: `https://your-app.azurewebsites.net/health`
- **Ready Check**: `https://your-app.azurewebsites.net/health/ready`
- **Live Check**: `https://your-app.azurewebsites.net/health/live`

### Application Insights
- Navigate to Azure Portal → Application Insights → your-application-insights-resource
- Monitor performance, errors, and usage metrics

### Log Streaming
```powershell
# View live application logs
az webapp log tail --name "app-rfid-inventory-tracker-staging" --resource-group "rg-rfid-inventory-tracker-staging"
```

### Common Issues

1. **Database Connection Errors**
   - Verify TestApps connection string is correct
   - Check if database server allows Azure services
   - Validate username/password credentials

2. **Key Vault Access Issues**
   - Ensure App Service managed identity has Key Vault access
   - Verify Key Vault access policies are configured correctly

3. **Deployment Failures**
   - Check GitHub Actions logs for detailed error messages
   - Verify all required secrets are configured
   - Ensure Azure service principals have sufficient permissions

## 📚 Additional Resources

- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure Key Vault Best Practices](https://docs.microsoft.com/en-us/azure/key-vault/general/best-practices)

## 🆘 Support

For deployment issues:
1. Check the GitHub Actions logs first
2. Review the Application Insights logs in Azure Portal
3. Create a GitHub issue with relevant error messages
4. Contact the DevOps team for infrastructure-related problems
