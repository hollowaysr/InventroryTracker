# RFID Inventory Tracker API Documentation

## Overview

The RFID Inventory Tracker API provides comprehensive functionality for managing customer lists and RFID tags with bulk operations and export capabilities.

**Base URL**: `https://your-api-domain.com/api`  
**Authentication**: Bearer Token (Azure AD JWT)  
**Content-Type**: `application/json`

## Authentication

All API endpoints require authentication using Azure AD JWT tokens.

### Getting a Token
```http
POST https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id={your-client-id}
&client_secret={your-client-secret}
&scope={your-api-scope}/.default
```

### Using the Token
```http
Authorization: Bearer {your-jwt-token}
```

## Error Responses

All endpoints return consistent error responses:

```json
{
  "title": "Error Title",
  "status": 400,
  "detail": "Detailed error message",
  "instance": "/api/endpoint",
  "errors": {
    "field": ["Validation error message"]
  }
}
```

## Customer Lists API

### Get All Customer Lists
```http
GET /api/customerlists
```

**Response:**
```json
[
  {
    "id": 1,
    "name": "Customer ABC Inventory",
    "description": "Main inventory for Customer ABC",
    "systemRef": "ABC-001",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z",
    "tagCount": 150
  }
]
```

### Get Customer List by ID
```http
GET /api/customerlists/{id}
```

**Parameters:**
- `id` (path, required): Customer list ID

**Response:**
```json
{
  "id": 1,
  "name": "Customer ABC Inventory",
  "description": "Main inventory for Customer ABC",
  "systemRef": "ABC-001",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z",
  "tagCount": 150
}
```

### Create Customer List
```http
POST /api/customerlists
```

**Request Body:**
```json
{
  "name": "New Customer Inventory",
  "description": "Inventory for new customer",
  "systemRef": "NEW-001"
}
```

**Response:** `201 Created` with created customer list object

### Update Customer List
```http
PUT /api/customerlists/{id}
```

**Request Body:**
```json
{
  "name": "Updated Customer Inventory",
  "description": "Updated description",
  "systemRef": "UPD-001"
}
```

**Response:** `200 OK` with updated customer list object

### Delete Customer List
```http
DELETE /api/customerlists/{id}
```

**Response:** `204 No Content`

## RFID Tags API

### Get All RFID Tags
```http
GET /api/rfidtags
```

**Response:**
```json
[
  {
    "id": 1,
    "rfid": "1234567890ABCDEF",
    "listId": 1,
    "name": "Sample Tag",
    "description": "Description of the tag",
    "color": "Blue",
    "size": "Medium",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z",
    "customerListName": "Customer ABC Inventory"
  }
]
```

### Get RFID Tag by ID
```http
GET /api/rfidtags/{id}
```

### Get RFID Tag by RFID Value
```http
GET /api/rfidtags/rfid/{rfid}
```

**Parameters:**
- `rfid` (path, required): RFID tag value (16 characters)

### Get RFID Tags by Customer List
```http
GET /api/rfidtags/list/{listId}
```

**Parameters:**
- `listId` (path, required): Customer list ID

### Create RFID Tag
```http
POST /api/rfidtags
```

**Request Body:**
```json
{
  "rfid": "1234567890ABCDEF",
  "listId": 1,
  "name": "New Tag",
  "description": "Tag description",
  "color": "Red",
  "size": "Large"
}
```

**Validation Rules:**
- `rfid`: Required, exactly 16 characters, unique
- `listId`: Required, must exist
- `name`: Required, max 100 characters
- `description`: Optional, max 500 characters
- `color`: Optional, max 50 characters
- `size`: Optional, max 50 characters

### Update RFID Tag
```http
PUT /api/rfidtags/{id}
```

### Delete RFID Tag
```http
DELETE /api/rfidtags/{id}
```

## Bulk Operations API

### Create Bulk RFID Tags
```http
POST /api/rfidtags/bulk
```

**Request Body:**
```json
{
  "listId": 1,
  "tags": [
    {
      "rfid": "1111111111111111",
      "name": "Tag 1",
      "description": "First tag"
    },
    {
      "rfid": "2222222222222222",
      "name": "Tag 2",
      "description": "Second tag"
    }
  ]
}
```

**Response:**
```json
{
  "totalCount": 2,
  "successCount": 2,
  "errorCount": 0,
  "errors": [],
  "createdTags": [/* array of created tags */]
}
```

### Create RFID Tags from CSV
```http
POST /api/rfidtags/bulk-csv
```

**Request Body:**
```json
{
  "listId": 1,
  "rfidCsv": "1111111111111111,2222222222222222,3333333333333333"
}
```

**Features:**
- Automatic validation of RFID format
- Duplicate detection and filtering
- Default metadata assignment
- Batch processing with rollback on errors

## Export API

### Export RFID Tags
```http
POST /api/rfidtags/export
```

**Request Body:**
```json
{
  "listId": 1,
  "format": "Excel",
  "includeMetadata": true
}
```

**Supported Formats:**
- `CSV`: Comma-separated values
- `Excel`: Microsoft Excel (.xlsx) with formatting
- `JSON`: JavaScript Object Notation
- `XML`: Extensible Markup Language

**Response:** Binary file download with appropriate content-type

### Export and Email RFID Tags
```http
POST /api/rfidtags/export-email
```

**Request Body:**
```json
{
  "listId": 1,
  "format": "Excel",
  "includeMetadata": true,
  "emailAddress": "user@example.com"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Export emailed successfully to user@example.com"
}
```

## Health Checks

### Application Health
```http
GET /health
```

**Response:**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "exception": null,
      "duration": "00:00:00.0234567"
    },
    {
      "name": "self",
      "status": "Healthy",
      "exception": null,
      "duration": "00:00:00.0001234"
    }
  ]
}
```

### Readiness Check
```http
GET /health/ready
```

### Liveness Check
```http
GET /health/live
```

## Rate Limiting

- **Limit**: 100 requests per minute per user/IP
- **Headers**: 
  - `X-RateLimit-Limit`: Maximum requests allowed
  - `X-RateLimit-Remaining`: Remaining requests in window
  - `X-RateLimit-Reset`: Time when limit resets

**Rate Limit Exceeded Response:**
```http
HTTP/1.1 429 Too Many Requests
Content-Type: application/json

{
  "error": "Rate limit exceeded",
  "retryAfter": 60
}
```

## Examples

### Complete Workflow Example

1. **Create a customer list:**
```bash
curl -X POST "https://api.example.com/api/customerlists" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Demo Customer",
    "description": "Demo customer inventory",
    "systemRef": "DEMO-001"
  }'
```

2. **Bulk import RFID tags:**
```bash
curl -X POST "https://api.example.com/api/rfidtags/bulk-csv" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "listId": 1,
    "rfidCsv": "1111111111111111,2222222222222222,3333333333333333"
  }'
```

3. **Export to Excel and email:**
```bash
curl -X POST "https://api.example.com/api/rfidtags/export-email" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "listId": 1,
    "format": "Excel",
    "includeMetadata": true,
    "emailAddress": "manager@company.com"
  }'
```

## SDK and Client Libraries

### JavaScript/TypeScript
```typescript
const client = new RfidTrackerClient({
  baseUrl: 'https://api.example.com',
  token: 'your-jwt-token'
});

const customerLists = await client.customerLists.getAll();
const tags = await client.rfidTags.getByListId(1);
```

### C# Client
```csharp
var client = new RfidTrackerClient("https://api.example.com", "your-jwt-token");
var customerLists = await client.CustomerLists.GetAllAsync();
var tags = await client.RfidTags.GetByListIdAsync(1);
```

## Troubleshooting

### Common Issues

1. **401 Unauthorized**: Check your JWT token and ensure it's valid
2. **403 Forbidden**: Verify your user has the required role permissions
3. **404 Not Found**: Ensure the resource ID exists
4. **429 Too Many Requests**: Implement retry logic with exponential backoff
5. **500 Internal Server Error**: Check application logs and health endpoints

### Support

- Check `/health` endpoint for application status
- Review Application Insights telemetry
- Enable debug logging for detailed error information
- Contact support with correlation ID from response headers
