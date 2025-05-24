# RFID Inventory Tracker

A comprehensive RFID inventory management system built with ASP.NET Core 9, Entity Framework Core, and Azure services. This application provides APIs for managing customer lists and RFID tags with bulk operations, export functionality, and enterprise-grade features.

## Features

### Core Functionality
- **Customer List Management** (FR001-FR006)
  - Create, read, update, and delete customer lists
  - System reference mapping for external integrations
  - Comprehensive CRUD operations with validation

- **RFID Tag Management** (FR001-FR006)
  - Individual RFID tag CRUD operations
  - Tag metadata support (name, description, color, size)
  - List-based organization and filtering

- **Bulk Operations** (FR007-FR008)
  - CSV bulk import with validation and duplicate detection
  - Batch creation with error handling and rollback
  - Support for comma-separated RFID input

- **Export Functionality** (FR009-FR012)
  - Multiple export formats: CSV, Excel, JSON, XML
  - Email delivery with attachments
  - Configurable metadata inclusion
  - Professional Excel formatting with EPPlus

### Enterprise Features
- **Azure AD Authentication & Authorization**
  - JWT Bearer token authentication
  - Role-based access control (Admin, Manager, User)
  - Swagger UI integration with authentication

- **Azure Integration**
  - Application Insights telemetry and monitoring
  - Azure Communication Services for email
  - Health checks for database and application status
  - Response compression for performance

- **Security & Performance**
  - Rate limiting (100 requests per minute per user/IP)
  - Redis caching support with fallback to memory cache
  - CORS configuration for development
  - HTTPS redirection and HSTS headers

## Technology Stack

- **Backend**: ASP.NET Core 9.0, C# 12
- **Database**: SQL Server with Entity Framework Core 9
- **Authentication**: Microsoft Identity Web (Azure AD)
- **Email**: Azure Communication Services
- **Caching**: Redis / In-Memory Cache
- **Export**: EPPlus for Excel, built-in JSON/XML/CSV
- **Testing**: xUnit, Microsoft.EntityFrameworkCore.InMemory
- **CI/CD**: GitHub Actions, Azure DevOps
- **Infrastructure**: Terraform (Azure App Service, SQL Database, Key Vault)

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server (LocalDB for development)
- Azure subscription (for production features)
- Visual Studio 2022 or VS Code

### Local Development Setup

1. **Clone the repository**
   ```powershell
   git clone <repository-url>
   cd InventroryTracker
   ```

2. **Restore packages**
   ```powershell
   dotnet restore
   ```

3. **Update connection string** in `appsettings.json`
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InventoryTrackerDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Run database migrations**
   ```powershell
   dotnet ef database update --project InventoryTracker.Data --startup-project InventoryTracker.Web
   ```

5. **Run the application**
   ```powershell
   dotnet run --project InventoryTracker.Web
   ```

6. **Access Swagger UI**
   - Development: `https://localhost:7000` (or configured port)
   - API documentation available at root URL

### Configuration

#### Azure AD Authentication
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "yourdomain.onmicrosoft.com",
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_CLIENT_ID",
    "Audience": "YOUR_API_AUDIENCE"
  }
}
```

#### Email Service (Azure Communication Services)
```json
{
  "ConnectionStrings": {
    "AzureCommunicationServices": "YOUR_AZURE_COMMUNICATION_SERVICES_CONNECTION_STRING"
  },
  "Email": {
    "FromAddress": "noreply@yourdomain.com"
  }
}
```

#### Application Insights
```json
{
  "ApplicationInsights": {
    "ConnectionString": "YOUR_APPLICATION_INSIGHTS_CONNECTION_STRING"
  }
}
```

## API Documentation

### Customer Lists
- `GET /api/customerlists` - Get all customer lists
- `GET /api/customerlists/{id}` - Get customer list by ID
- `POST /api/customerlists` - Create new customer list
- `PUT /api/customerlists/{id}` - Update customer list
- `DELETE /api/customerlists/{id}` - Delete customer list

### RFID Tags
- `GET /api/rfidtags` - Get all RFID tags
- `GET /api/rfidtags/{id}` - Get RFID tag by ID
- `GET /api/rfidtags/rfid/{rfid}` - Get tag by RFID value
- `GET /api/rfidtags/list/{listId}` - Get tags by customer list
- `POST /api/rfidtags` - Create new RFID tag
- `PUT /api/rfidtags/{id}` - Update RFID tag
- `DELETE /api/rfidtags/{id}` - Delete RFID tag

### Bulk Operations
- `POST /api/rfidtags/bulk` - Create multiple RFID tags
- `POST /api/rfidtags/bulk-csv` - Create tags from CSV input

### Export Operations
- `POST /api/rfidtags/export` - Export tags to file
- `POST /api/rfidtags/export-email` - Export and email tags

## Testing

### Run Unit Tests
```powershell
dotnet test InventoryTracker.Tests
```

### Test Coverage
- Service layer unit tests (100% coverage)
- Repository integration tests
- Controller integration tests
- End-to-end API tests

## Deployment

### Azure Deployment with Terraform

1. **Configure Terraform variables**
   ```powershell
   cp infrastructure/terraform.tfvars.example infrastructure/terraform.tfvars
   # Edit terraform.tfvars with your values
   ```

2. **Deploy infrastructure**
   ```powershell
   cd infrastructure
   terraform init
   terraform plan
   terraform apply
   ```

### CI/CD Pipelines

#### GitHub Actions
- Automated build, test, and deployment
- Security scanning with CodeQL
- Environment-specific deployments
- Artifact management

#### Azure DevOps
- Complete build and release pipeline
- Integration with Azure Key Vault
- Automated testing and deployment

## Monitoring and Observability

### Health Checks
- `/health` - Overall application health
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

### Application Insights
- Request/response telemetry
- Dependency tracking
- Custom metrics and logging
- Performance monitoring

### Logging
- Structured logging with Serilog
- Azure Application Insights integration
- Correlation IDs for request tracking

## Security

### Authentication & Authorization
- Azure AD integration with JWT tokens
- Role-based access control
- API endpoint protection

### Rate Limiting
- 100 requests per minute per user/IP
- Configurable limits per endpoint
- 429 status code for exceeded limits

### Data Protection
- HTTPS enforcement
- HSTS headers
- Secure cookie configuration
- Input validation and sanitization

## Performance

### Caching Strategy
- Redis distributed cache (production)
- In-memory cache (development)
- Response caching for static data

### Database Optimization
- Entity Framework query optimization
- Database indexes for performance
- Connection pooling and retry policies

### Response Compression
- Gzip compression for API responses
- Reduced bandwidth usage
- Improved load times

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add/update tests
5. Submit a pull request

### Code Standards
- Follow C# coding conventions
- Include XML documentation
- Write comprehensive tests
- Update documentation

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For support and questions:
- Create an issue in the repository
- Check the API documentation in Swagger UI
- Review the application logs and health checks