# RFID Inventory Tracker - Quick Start Verification

## Application Verification Steps

This document provides quick verification steps to ensure the RFID Inventory Tracker application is working correctly.

### 1. Build Verification
```powershell
# Navigate to project directory
Set-Location "c:\Users\hollo\source\code_repos\InventroryTracker\InventroryTracker"

# Clean and build solution
dotnet clean
dotnet restore
dotnet build --configuration Release

# Expected: Build succeeded with no errors
```

### 2. Database Setup
```powershell
# Apply database migrations
dotnet ef database update --project InventoryTracker.Data --startup-project InventoryTracker.Web

# Verify database creation
dotnet ef database list --project InventoryTracker.Data --startup-project InventoryTracker.Web
```

### 3. Run Tests
```powershell
# Run all unit and integration tests
dotnet test --configuration Release --logger "console;verbosity=detailed"

# Expected: All tests pass
```

### 4. Start Application
```powershell
# Run the web application
dotnet run --project InventoryTracker.Web --environment Development

# Expected: Application starts on https://localhost:7000 (or configured port)
```

### 5. API Health Check
```powershell
# Test health endpoint (in new terminal while app is running)
Invoke-RestMethod -Uri "https://localhost:7000/health" -Method Get

# Expected: Returns JSON with "status": "Healthy"
```

### 6. Swagger UI Verification
- Open browser to: `https://localhost:7000`
- Verify Swagger UI loads with all API endpoints
- Check authentication section is available

### 7. Basic API Test (without authentication)
```powershell
# Test customer lists endpoint (if authentication is disabled for development)
Invoke-RestMethod -Uri "https://localhost:7000/api/customerlists" -Method Get -Headers @{"Accept"="application/json"}

# Note: May return 401 if authentication is enabled
```

## Quick Demo Workflow

### 1. Create Customer List
```json
POST /api/customerlists
{
  "name": "Demo Customer List",
  "description": "Test customer list for verification",
  "systemRef": "DEMO-001"
}
```

### 2. Bulk Import RFID Tags
```json
POST /api/rfidtags/bulk-csv
{
  "listId": 1,
  "rfidCsv": "1111111111111111,2222222222222222,3333333333333333"
}
```

### 3. Export Data
```json
POST /api/rfidtags/export
{
  "listId": 1,
  "format": "CSV",
  "includeMetadata": true
}
```

## Troubleshooting

### Common Issues

1. **Build Errors**
   - Ensure .NET 9.0 SDK is installed
   - Run `dotnet restore` to restore NuGet packages
   - Check for any missing dependencies

2. **Database Issues**
   - Verify SQL Server LocalDB is installed
   - Check connection string in appsettings.json
   - Ensure migrations are applied

3. **Authentication Issues**
   - For development, authentication can be disabled
   - Check Azure AD configuration if using production settings
   - Verify JWT token format and expiration

4. **Port Conflicts**
   - Application runs on HTTPS port 7000 by default
   - Check Properties/launchSettings.json for port configuration
   - Ensure port is not in use by another application

### Verification Checklist

- [ ] Solution builds without errors
- [ ] Database migrations apply successfully
- [ ] All tests pass
- [ ] Application starts without errors
- [ ] Health check endpoint responds
- [ ] Swagger UI loads correctly
- [ ] API endpoints are accessible
- [ ] Authentication is configured (if enabled)

## Success Indicators

✅ **Build Success**: Solution compiles without errors  
✅ **Database Ready**: Migrations applied and database accessible  
✅ **Tests Passing**: All unit and integration tests pass  
✅ **API Functional**: Health checks and basic endpoints respond  
✅ **Documentation Available**: Swagger UI loads with complete API documentation  

## Production Readiness Checklist

Before deploying to production:

- [ ] Configure Azure AD authentication
- [ ] Set up Azure Communication Services for email
- [ ] Configure Application Insights monitoring
- [ ] Set up Redis cache (optional)
- [ ] Configure HTTPS certificates
- [ ] Set up CI/CD pipeline
- [ ] Configure backup strategy
- [ ] Set up monitoring alerts

## Performance Baseline

Expected performance metrics:
- **Startup Time**: < 10 seconds
- **Health Check Response**: < 100ms
- **API Response Time**: < 200ms for simple operations
- **Bulk Import**: > 1000 tags per minute
- **Export Generation**: < 5 seconds for 10,000 records

## Next Steps

1. **Local Development**: Use the application for development and testing
2. **Azure Deployment**: Follow DEPLOYMENT.md for production setup
3. **Integration**: Use API_DOCUMENTATION.md for client integration
4. **Monitoring**: Set up Application Insights for production monitoring
5. **Scaling**: Configure auto-scaling based on usage patterns
