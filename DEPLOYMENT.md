# RFID Inventory Tracker - Deployment Guide

## Local Development Deployment

### Prerequisites
- .NET 9.0 SDK
- SQL Server LocalDB
- Visual Studio 2022 or VS Code
- Azure CLI (for Azure deployment)

### Step 1: Clone and Build
```powershell
# Clone the repository
git clone <repository-url>
Set-Location "InventroryTracker"

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build --configuration Release
```

### Step 2: Database Setup
```powershell
# Apply database migrations
dotnet ef database update --project InventoryTracker.Data --startup-project InventoryTracker.Web

# Verify database creation
dotnet ef database list --project InventoryTracker.Data --startup-project InventoryTracker.Web
```

### Step 3: Configuration
1. Update `appsettings.json` with your settings:
   - Database connection string
   - Azure AD configuration (if using)
   - Email service settings
   - Application Insights connection string

### Step 4: Run the Application
```powershell
# Run in development mode
dotnet run --project InventoryTracker.Web --environment Development

# Access the API
# https://localhost:7000 (or your configured port)
```

## Azure Production Deployment

### Option 1: Terraform Infrastructure as Code

1. **Configure Terraform variables**
```powershell
Set-Location "infrastructure"
Copy-Item "terraform.tfvars.example" "terraform.tfvars"
# Edit terraform.tfvars with your Azure settings
```

2. **Deploy Azure infrastructure**
```powershell
# Login to Azure
az login

# Initialize Terraform
terraform init

# Plan deployment
terraform plan

# Apply infrastructure
terraform apply
```

### Option 2: Manual Azure Deployment

1. **Create Azure Resources**
```powershell
# Login to Azure
az login

# Create resource group
az group create --name "rg-rfid-tracker" --location "East US"

# Create SQL Server and Database
az sql server create --name "sql-rfid-tracker" --resource-group "rg-rfid-tracker" --location "East US" --admin-user "sqladmin" --admin-password "YourPassword123!"

az sql db create --name "rfid-inventory-db" --server "sql-rfid-tracker" --resource-group "rg-rfid-tracker" --service-objective "S0"

# Create App Service Plan
az appservice plan create --name "asp-rfid-tracker" --resource-group "rg-rfid-tracker" --sku "S1" --is-linux

# Create Web App
az webapp create --name "app-rfid-tracker" --resource-group "rg-rfid-tracker" --plan "asp-rfid-tracker" --runtime "DOTNETCORE|9.0"
```

2. **Configure Application Settings**
```powershell
# Set connection string
az webapp config connection-string set --name "app-rfid-tracker" --resource-group "rg-rfid-tracker" --connection-string-type "SQLAzure" --settings DefaultConnection="Server=tcp:sql-rfid-tracker.database.windows.net,1433;Database=rfid-inventory-db;User ID=sqladmin;Password=YourPassword123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Set application settings
az webapp config appsettings set --name "app-rfid-tracker" --resource-group "rg-rfid-tracker" --settings @azure-app-settings.json
```

3. **Deploy Application**
```powershell
# Publish the application
dotnet publish InventoryTracker.Web -c Release -o ./publish

# Create deployment package
Compress-Archive -Path "./publish/*" -DestinationPath "deployment.zip"

# Deploy to Azure
az webapp deployment source config-zip --name "app-rfid-tracker" --resource-group "rg-rfid-tracker" --src "deployment.zip"
```

## CI/CD Deployment

### GitHub Actions
The repository includes a complete GitHub Actions workflow in `.github/workflows/ci-cd.yml` that:
- Builds and tests the application
- Runs security scans
- Deploys to Azure App Service
- Manages environment-specific configurations

### Azure DevOps
The repository includes an Azure DevOps pipeline in `azure-pipelines.yml` that:
- Performs comprehensive testing
- Builds Docker images
- Deploys infrastructure with Terraform
- Manages release to multiple environments

## Configuration Management

### Environment Variables
```powershell
# Production environment variables
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ConnectionStrings__DefaultConnection = "your-sql-connection-string"
$env:AzureAd__TenantId = "your-tenant-id"
$env:AzureAd__ClientId = "your-client-id"
$env:ConnectionStrings__AzureCommunicationServices = "your-email-connection-string"
$env:ApplicationInsights__ConnectionString = "your-appinsights-connection-string"
```

### Azure Key Vault Integration
For production environments, use Azure Key Vault to manage secrets:

```json
{
  "KeyVault": {
    "VaultUri": "https://your-keyvault.vault.azure.net/"
  }
}
```

## Monitoring and Observability

### Application Insights Setup
1. Create Application Insights resource in Azure
2. Copy the connection string to application settings
3. Monitor telemetry at https://portal.azure.com

### Health Check Endpoints
- `/health` - Overall application health
- `/health/ready` - Readiness probe for Kubernetes
- `/health/live` - Liveness probe for Kubernetes

### Log Aggregation
Configure structured logging for production:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

## Security Configuration

### Azure AD Authentication
1. Register application in Azure AD
2. Configure API permissions
3. Set up client credentials for service-to-service authentication
4. Configure role-based access control

### HTTPS and Security Headers
The application automatically configures:
- HTTPS redirection
- HSTS headers
- CORS policies
- Rate limiting

## Performance Optimization

### Database Performance
- Enable connection pooling
- Configure query timeout settings
- Implement database indexes (see `InventoryTracker.Database` project)

### Caching Configuration
```json
{
  "ConnectionStrings": {
    "Redis": "your-redis-connection-string"
  }
}
```

### Response Compression
Automatically enabled for:
- JSON responses
- Static files
- API documentation

## Backup and Disaster Recovery

### Database Backup
```powershell
# Automated backup (Azure SQL Database)
az sql db export --name "rfid-inventory-db" --server "sql-rfid-tracker" --resource-group "rg-rfid-tracker" --admin-user "sqladmin" --admin-password "YourPassword123!" --storage-key-type "StorageAccessKey" --storage-key "your-storage-key" --storage-uri "https://yourstorageaccount.blob.core.windows.net/backups/database-backup.bacpac"
```

### Application Data Export
Use the built-in export functionality to create regular data backups in multiple formats (CSV, Excel, JSON, XML).

## Troubleshooting

### Common Issues

1. **Database Connection Issues**
   - Verify connection string format
   - Check SQL Server authentication settings
   - Ensure database exists and migrations are applied

2. **Authentication Problems**
   - Verify Azure AD configuration
   - Check JWT token expiration
   - Validate client credentials

3. **Performance Issues**
   - Enable Application Insights profiling
   - Check database query performance
   - Monitor rate limiting metrics

### Diagnostic Commands
```powershell
# Check application health
Invoke-RestMethod -Uri "https://your-app.azurewebsites.net/health" -Method Get

# View application logs
az webapp log tail --name "app-rfid-tracker" --resource-group "rg-rfid-tracker"

# Check resource utilization
az monitor metrics list --resource "your-resource-id" --metric "CpuPercentage"
```

## Support and Maintenance

### Regular Maintenance Tasks
1. Monitor Application Insights for errors and performance
2. Review security scan results from CI/CD pipelines
3. Update NuGet packages regularly
4. Monitor database growth and performance
5. Review and rotate Azure AD secrets
6. Backup and test disaster recovery procedures

### Support Contacts
- Technical Issues: Create GitHub issue
- Security Concerns: Contact security team
- Infrastructure: Contact DevOps team

## Scaling Considerations

### Horizontal Scaling
- Use Azure App Service scale-out capabilities
- Implement Redis for distributed caching
- Configure load balancer health checks

### Database Scaling
- Consider Azure SQL Database elastic pools
- Implement read replicas for reporting workloads
- Monitor DTU/vCore utilization

### Performance Monitoring
- Set up Application Insights alerts
- Monitor API response times
- Track database performance metrics
- Configure auto-scaling rules
