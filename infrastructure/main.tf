# Configure the Azure Provider
terraform {
  required_version = ">= 1.7.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
  
  # Store state in Azure Storage (best practice)
  # Backend configuration will be provided during initialization
  backend "azurerm" {}
}

# Configure the Microsoft Azure Provider
provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
    key_vault {
      purge_soft_delete_on_destroy    = true
      recover_soft_deleted_key_vaults = true
    }
  }
}

# Variables
variable "environment" {
  description = "Environment name (staging, prod)"
  type        = string
  
  validation {
    condition = contains(["staging", "prod"], var.environment)
    error_message = "Environment must be either 'staging' or 'prod'."
  }
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "East US"
}

variable "app_name" {
  description = "Application name"
  type        = string
  default     = "rfid-inventory-tracker"
}

variable "testapps_db_server" {
  description = "TestApps database server FQDN"
  type        = string
  default     = "heccdbs.database.windows.net"
}

variable "testapps_db_name" {
  description = "TestApps database name"
  type        = string
  default     = "TestApps"
}

variable "testapps_db_username" {
  description = "TestApps database username"
  type        = string
  sensitive   = true
}

variable "testapps_db_password" {
  description = "TestApps database password"
  type        = string
  sensitive   = true
}

# Local values for naming consistency
locals {
  resource_prefix = "${var.app_name}-${var.environment}"
  common_tags = {
    Environment   = var.environment
    Application   = var.app_name
    ManagedBy     = "Terraform"
    Project       = "RFID-Inventory-Tracker"
    CreatedDate   = formatdate("YYYY-MM-DD", timestamp())
  }
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "rg-${local.resource_prefix}"
  location = var.location

  tags = local.common_tags
}

# Log Analytics Workspace for Application Insights
resource "azurerm_log_analytics_workspace" "main" {
  name                = "log-${local.resource_prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = var.environment == "prod" ? 90 : 30

  tags = local.common_tags
}

# Application Insights
resource "azurerm_application_insights" "main" {
  name                = "ai-${local.resource_prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"

  tags = local.common_tags
}

# Key Vault for application secrets
resource "azurerm_key_vault" "main" {
  name                = "kv-${local.resource_prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"

  # Enable soft delete and purge protection for production
  soft_delete_retention_days = var.environment == "prod" ? 90 : 7
  purge_protection_enabled   = var.environment == "prod" ? true : false

  # Network access rules
  network_acls {
    default_action = "Allow"
    bypass         = "AzureServices"
  }

  # Current deployment identity access
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    secret_permissions = [
      "Get", "List", "Set", "Delete", "Recover", "Backup", "Restore", "Purge"
    ]
    
    certificate_permissions = [
      "Get", "List", "Create", "Import", "Delete", "Recover", "Backup", "Restore", "ManageContacts", "ManageIssuers", "GetIssuers", "ListIssuers", "SetIssuers", "DeleteIssuers"
    ]
  }

  tags = local.common_tags
}

# Store TestApps database connection string in Key Vault
resource "azurerm_key_vault_secret" "testapps_connection_string" {
  name         = "TestAppsConnectionString"
  value        = "Server=${var.testapps_db_server};Database=${var.testapps_db_name};User ID=${var.testapps_db_username};Password=${var.testapps_db_password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  key_vault_id = azurerm_key_vault.main.id

  tags = local.common_tags
}

# Store Application Insights connection string in Key Vault
resource "azurerm_key_vault_secret" "application_insights_connection_string" {
  name         = "ApplicationInsightsConnectionString"
  value        = azurerm_application_insights.main.connection_string
  key_vault_id = azurerm_key_vault.main.id

  tags = local.common_tags
}

# App Service Plan
resource "azurerm_service_plan" "main" {
  name                = "sp-${local.resource_prefix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = var.environment == "prod" ? "P1v3" : "B2"

  tags = local.common_tags
}

# App Service
resource "azurerm_linux_web_app" "main" {
  name                = "app-${local.resource_prefix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    application_stack {
      dotnet_version = "9.0"
    }
    
    always_on                         = var.environment == "prod" ? true : false
    health_check_path                 = "/health"
    health_check_eviction_time_in_min = var.environment == "prod" ? 10 : 5
    
    # Security headers
    use_32_bit_worker   = false
    websockets_enabled  = false
    
    # CORS configuration for RFID Inventory Tracker
    cors {
      allowed_origins     = var.environment == "prod" ? ["https://app-${local.resource_prefix}.azurewebsites.net"] : ["*"]
      support_credentials = false
    }
  }

  # Application settings
  app_settings = {
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.main.name};SecretName=${azurerm_key_vault_secret.application_insights_connection_string.name})"
    "ASPNETCORE_ENVIRONMENT"                = var.environment == "prod" ? "Production" : "Staging"
    "WEBSITE_RUN_FROM_PACKAGE"              = "1"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"       = "true"
    
    # RFID Tracker specific settings
    "InventoryTracker__Environment"         = var.environment
    "InventoryTracker__DatabaseProvider"    = "TestApps"
    "Logging__LogLevel__Default"            = var.environment == "prod" ? "Information" : "Debug"
    "Logging__LogLevel__Microsoft.AspNetCore" = "Warning"
  }

  # Database connection string from Key Vault
  connection_string {
    name  = "DefaultConnection"
    type  = "SQLAzure"
    value = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.main.name};SecretName=${azurerm_key_vault_secret.testapps_connection_string.name})"
  }

  # Managed Identity for Key Vault access
  identity {
    type = "SystemAssigned"
  }

  # HTTPS only
  https_only = true

  tags = local.common_tags
}

# Key Vault access for App Service
resource "azurerm_key_vault_access_policy" "app_service" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = azurerm_linux_web_app.main.identity[0].tenant_id
  object_id    = azurerm_linux_web_app.main.identity[0].principal_id

  secret_permissions = [
    "Get", "List"
  ]
}

# Data source for current client config
data "azurerm_client_config" "current" {}

# Action Group for monitoring alerts
resource "azurerm_monitor_action_group" "main" {
  name                = "ag-${local.resource_prefix}"
  resource_group_name = azurerm_resource_group.main.name
  short_name          = var.environment == "prod" ? "RFIDProd" : "RFIDStag"

  email_receiver {
    name          = "DevOps Team"
    email_address = "devops@company.com"  # Update with actual email
  }

  tags = local.common_tags
}

# Application Insights alerts
resource "azurerm_monitor_metric_alert" "app_availability" {
  name                = "alert-${local.resource_prefix}-availability"
  resource_group_name = azurerm_resource_group.main.name
  scopes              = [azurerm_application_insights.main.id]
  description         = "RFID Inventory Tracker application availability alert"
  severity            = var.environment == "prod" ? 0 : 1
  frequency           = "PT5M"
  window_size         = "PT15M"

  criteria {
    metric_namespace = "Microsoft.Insights/components"
    metric_name      = "availabilityResults/availabilityPercentage"
    aggregation      = "Average"
    operator         = "LessThan"
    threshold        = var.environment == "prod" ? 95 : 90
  }

  action {
    action_group_id = azurerm_monitor_action_group.main.id
  }

  tags = local.common_tags
}

resource "azurerm_monitor_metric_alert" "app_response_time" {
  name                = "alert-${local.resource_prefix}-response-time"
  resource_group_name = azurerm_resource_group.main.name
  scopes              = [azurerm_application_insights.main.id]
  description         = "RFID Inventory Tracker response time alert"
  severity            = var.environment == "prod" ? 1 : 2
  frequency           = "PT5M"
  window_size         = "PT15M"

  criteria {
    metric_namespace = "Microsoft.Insights/components"
    metric_name      = "requests/duration"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = var.environment == "prod" ? 2000 : 5000  # milliseconds
  }

  action {
    action_group_id = azurerm_monitor_action_group.main.id
  }

  tags = local.common_tags
}

# Outputs
output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "app_service_name" {
  description = "Name of the App Service"
  value       = azurerm_linux_web_app.main.name
}

output "app_service_url" {
  description = "URL of the App Service"
  value       = "https://${azurerm_linux_web_app.main.default_hostname}"
}

output "application_insights_connection_string" {
  description = "Application Insights connection string"
  value       = azurerm_application_insights.main.connection_string
  sensitive   = true
}

output "key_vault_name" {
  description = "Name of the Key Vault"
  value       = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  description = "URI of the Key Vault"
  value       = azurerm_key_vault.main.vault_uri
}

output "testapps_database_info" {
  description = "TestApps database connection information"
  value = {
    server   = var.testapps_db_server
    database = var.testapps_db_name
  }
}
