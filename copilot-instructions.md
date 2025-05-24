# Copilot Coding Best Practices for Azure Functions & Azure App Services

This document outlines best practices for developing web-hosted projects using Azure Functions and hosting them in Azure App Services. Follow these guidelines to ensure your project is secure, maintainable, and scalable.

## 1. Azure Functions Best Practices
- **Use the Latest Programming Models:**
  - For JavaScript, use v4; for Python, use v2.
  - Ensure your Function App is configured to use Functions Host v4.
- **Project Structure & Tools:**
  - Use Azure Functions Core Tools for local development and deployment.
  - Generate `local.settings.json` for local configuration management.
  - Prefer extension bundles over direct SDKs for bindings.
- **Bindings & Triggers:**
  - Use Event Grid as the source for Blob triggers for better scalability.
  - For Durable Functions, use the Durable Task Framework for performance.
- **.NET Functions:**
  - Use the latest SDKs and prefer the isolated process model over in-process.
- **Configuration:**
  - Let the user choose the function authentication level as appropriate.
  - Use the latest language runtime version.
- **Avoid:**
  - Do not generate or commit `function.json` for v4 (JavaScript) or v2 (Python) projects.

## 2. General Azure Coding Best Practices
- **Authentication & Security:**
  - Always use Managed Identity for Azure-hosted code.
  - Never hardcode credentials; store secrets in Azure Key Vault.
  - Implement credential rotation and least privilege access.
  - Enable encryption and secure connections for all services.
- **Error Handling & Reliability:**
  - Implement retry logic with exponential backoff for transient failures.
  - Add proper logging and monitoring (e.g., Application Insights).
  - Use circuit breakers where needed.
  - Ensure proper resource cleanup.
- **Performance & Scaling:**
  - Use connection pooling for databases.
  - Configure concurrency and timeouts appropriately.
  - Implement caching where beneficial.
  - Optimize batch operations and monitor resource usage.
- **Database & Storage Operations:**
  - Use parameterized queries and proper indexing.
  - Handle connection management and enable encryption.
  - For file storage, use batch operations and manage concurrency.
  - **Database Project Requirement:** All databases used in the solution must be defined in a SQL Server Database Project (`.sqlproj`) contained within the Visual Studio solution. This ensures:
    - Version control of database schema and structure
    - Automated database deployments through CI/CD pipelines
    - Database schema comparison and validation
    - Consistent database environments across development, testing, and production
  - **Code-First Entity Framework:** All database objects should be defined as code-first entities following Entity Framework best practices:
    - Define entity classes with proper data annotations or Fluent API configuration
    - Use Entity Framework migrations to manage database schema changes
    - Implement DbContext with proper configuration and dependency injection
    - Follow naming conventions: entities in PascalCase, properties in PascalCase, navigation properties clearly named
    - Use appropriate data types and constraints (e.g., [Required], [MaxLength], [Key])
    - Define relationships using navigation properties and foreign key constraints
    - Implement proper indexing through data annotations or Fluent API
    - Use value objects and owned entities where appropriate for complex types
- **Quality & Maintainability:**
  - Write clean, readable, and well-organized code.
  - Use consistent naming and language-specific conventions.
  - Separate concerns and document key decisions.

## 3. Azure App Services Hosting
- **Deployment:**
  - Use CI/CD pipelines for automated deployments.
  - Store configuration in environment variables or Azure App Configuration.
- **Security:**
  - Enable HTTPS and enforce secure access.
  - Use RBAC for management and data plane access.
- **Monitoring:**
  - Integrate with Application Insights for logging and telemetry.
  - Set up alerts for critical metrics.

## 4. Implementation Checklist
- [ ] Use Managed Identity for all Azure resource access
- [ ] Store secrets in Azure Key Vault
- [ ] Implement error handling and logging
- [ ] Use the latest Azure Functions runtime and SDKs
- [ ] Automate deployments with CI/CD
- [ ] Document configuration and key decisions

## 5. References
- [Azure Functions Best Practices](https://learn.microsoft.com/azure/azure-functions/functions-best-practices)
- [Azure App Service Best Practices](https://learn.microsoft.com/azure/app-service/best-practices)
- [Managed Identities in Azure](https://learn.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview)
- [Azure Key Vault](https://learn.microsoft.com/azure/key-vault/general/overview)

## 6. Testing Best Practices
- **Unit Test Coverage:**
  - Ensure all code is covered by unit tests.
  - Write tests for all functions, modules, and critical logic paths.
  - Use mocks and stubs for external dependencies (e.g., Azure services, databases).
  - Do not use the Moq framework for mocking in tests; prefer alternative mocking libraries or built-in test doubles.
  - Use the Builder pattern for creating test objects and arranging test data to improve readability and maintainability of unit tests.
  - Run tests automatically in your CI/CD pipeline.
  - Maintain high code coverage and review untested code regularly.

## 7. CI/CD Pipeline Best Practices
- **YAML Pipeline Configuration:**
  - Define CI/CD pipelines using YAML files for both GitHub Actions (`.github/workflows/*.yml`) and Azure DevOps (`azure-pipelines.yml`).
  - Include steps for build, test (unit/integration), security scanning, and deployment.
  - Use secrets and service connections for authentication; never store credentials in the pipeline file.
  - Automate running of all tests and code quality checks before deployment.
  - Use environment variables and pipeline variables for configuration.
  - Store pipeline files in source control and review changes as part of code review.
  - Example references:
    - [GitHub Actions Documentation](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)
    - [Azure Pipelines YAML Documentation](https://learn.microsoft.com/azure/devops/pipelines/yaml-schema)

## 8. Infrastructure as Code (IaC) with Terraform
- **Terraform Script Best Practices:**
  - Define Azure resources using Terraform scripts (`.tf` files) for infrastructure management.
  - Use modules to organize and reuse infrastructure code.
  - Store state securely using Azure Storage backend.
  - Use variables and outputs for configuration and resource references.
  - Never hardcode secrets; use Key Vault or pipeline secrets.
  - Version control all Terraform scripts and modules.
  - Example reference: [Terraform Azure Provider Documentation](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)

- **CI/CD for Terraform Deployments:**
  - Create YAML pipelines (GitHub Actions or Azure DevOps) to automate Terraform plan and apply steps.
  - Include steps for `terraform init`, `terraform validate`, `terraform plan`, and `terraform apply`.
  - Use pipeline secrets for Azure credentials and backend configuration.
  - Require manual approval for production deployments.
  - Store Terraform state securely and lock state during apply.
  - Example references:
    - [Deploying Terraform with GitHub Actions](https://learn.hashicorp.com/tutorials/terraform/github-actions)
    - [Terraform with Azure Pipelines](https://learn.microsoft.com/azure/developer/terraform/create-cicd-pipeline)

## 9. C# Coding Practices
- **General Guidelines:**
  - Follow the official Microsoft C# coding conventions: [C# Coding Conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
  - Use PascalCase for class, method, and property names; use camelCase for local variables and parameters.
  - Prefer explicit types over var except when the type is obvious from the right side of the assignment.
  - Use meaningful, descriptive names for all identifiers.
  - Keep methods short and focused on a single responsibility.
  - Avoid magic numbers and strings; use constants or enums.
  - Use string interpolation instead of string concatenation.
  - Prefer async/await for asynchronous programming.
  - Use exception handling judiciously; catch only exceptions you can handle.
  - Document public APIs with XML comments.
- **Object-Oriented Design:**
  - Apply SOLID principles (Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion).
  - Use interfaces and dependency injection for testability and flexibility.
  - Favor composition over inheritance.
- **Code Quality:**
  - Use static code analysis tools (e.g., Roslyn analyzers, SonarQube).
  - Write unit tests for all logic; avoid testing implementation details.
  - Review code for readability, maintainability, and performance.
- **.NET & Azure Specific:**
  - Use the latest supported .NET version.
  - Prefer nullable reference types and enable nullable context.
  - Use `IConfiguration` and options pattern for configuration.
  - Use `ILogger<T>` for logging.
  - Dispose of IDisposable objects properly (prefer using statements).
  - Avoid blocking calls in async code (e.g., `.Result`, `.Wait()`).

---

**Always validate your implementation against these best practices before deploying to production.**
