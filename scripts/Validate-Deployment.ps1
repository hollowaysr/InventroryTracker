# RFID Inventory Tracker - Deployment Validation Script
# This script validates that all components are properly configured for deployment

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("staging", "production")]
    [string]$Environment = "staging"
)

Write-Host "🔍 RFID Inventory Tracker - Deployment Validation" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green
Write-Host ""

$validationResults = @()

# Function to add validation result
function Add-ValidationResult {
    param($Test, $Status, $Message, $Details = "")
    $validationResults += [PSCustomObject]@{
        Test = $Test
        Status = $Status
        Message = $Message
        Details = $Details
    }
}

# 1. Check .NET Environment
Write-Host "1️⃣ .NET Environment Validation" -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    if ($dotnetVersion -like "9.*") {
        Add-ValidationResult "✅ .NET Version" "PASS" ".NET $dotnetVersion installed"
    } else {
        Add-ValidationResult "❌ .NET Version" "FAIL" ".NET 9.0 required, found $dotnetVersion"
    }
} catch {
    Add-ValidationResult "❌ .NET Installation" "FAIL" ".NET not found or not in PATH"
}

# 2. Check Project Structure
Write-Host "2️⃣ Project Structure Validation" -ForegroundColor Yellow
$requiredFiles = @(
    "InventoryTracker.sln",
    "InventoryTracker.Web/InventoryTracker.Web.csproj",
    "InventoryTracker.Core/InventoryTracker.Core.csproj",
    "InventoryTracker.Data/InventoryTracker.Data.csproj",
    "TestDatabaseConnection.csproj",
    "infrastructure/main.tf",
    ".github/workflows/ci-cd.yml",
    ".github/workflows/infrastructure.yml"
)

foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Add-ValidationResult "✅ File Structure" "PASS" "$file exists"
    } else {
        Add-ValidationResult "❌ File Structure" "FAIL" "$file missing"
    }
}

# 3. Check Solution Build
Write-Host "3️⃣ Solution Build Validation" -ForegroundColor Yellow
try {
    Write-Host "Building solution..." -ForegroundColor White
    $buildOutput = dotnet build --configuration Release --verbosity quiet 2>&1
    if ($LASTEXITCODE -eq 0) {
        Add-ValidationResult "✅ Solution Build" "PASS" "Solution builds successfully"
    } else {
        Add-ValidationResult "❌ Solution Build" "FAIL" "Build failed: $buildOutput"
    }
} catch {
    Add-ValidationResult "❌ Solution Build" "ERROR" "Build error: $_"
}

# 4. Check Unit Tests
Write-Host "4️⃣ Unit Tests Validation" -ForegroundColor Yellow
try {
    Write-Host "Running unit tests..." -ForegroundColor White
    $testOutput = dotnet test --configuration Release --verbosity quiet --no-build 2>&1
    if ($LASTEXITCODE -eq 0) {
        Add-ValidationResult "✅ Unit Tests" "PASS" "All tests pass"
    } else {
        Add-ValidationResult "❌ Unit Tests" "FAIL" "Test failures: $testOutput"
    }
} catch {
    Add-ValidationResult "❌ Unit Tests" "ERROR" "Test error: $_"
}

# 5. Check TestApps Database Connection
Write-Host "5️⃣ TestApps Database Connection" -ForegroundColor Yellow
if ($env:ConnectionStrings__DefaultConnection) {
    try {
        Write-Host "Testing database connection..." -ForegroundColor White
        $dbTestOutput = dotnet run --project TestDatabaseConnection 2>&1
        if ($LASTEXITCODE -eq 0) {
            Add-ValidationResult "✅ Database Connection" "PASS" "TestApps database accessible"
        } else {
            Add-ValidationResult "❌ Database Connection" "FAIL" "Connection failed: $dbTestOutput"
        }
    } catch {
        Add-ValidationResult "❌ Database Connection" "ERROR" "Connection test error: $_"
    }
} else {
    Add-ValidationResult "⚠️ Database Connection" "SKIP" "ConnectionStrings__DefaultConnection not set"
}

# 6. Check Azure CLI
Write-Host "6️⃣ Azure CLI Validation" -ForegroundColor Yellow
try {
    $azVersion = az version --query '"azure-cli"' -o tsv 2>$null
    if ($azVersion) {
        Add-ValidationResult "✅ Azure CLI" "PASS" "Azure CLI $azVersion installed"
        
        # Check if logged in
        $azAccount = az account show --query "user.name" -o tsv 2>$null
        if ($azAccount) {
            Add-ValidationResult "✅ Azure Authentication" "PASS" "Logged in as $azAccount"
        } else {
            Add-ValidationResult "⚠️ Azure Authentication" "WARN" "Not logged into Azure CLI"
        }
    } else {
        Add-ValidationResult "❌ Azure CLI" "FAIL" "Azure CLI not installed"
    }
} catch {
    Add-ValidationResult "❌ Azure CLI" "ERROR" "Azure CLI check failed: $_"
}

# 7. Check Terraform
Write-Host "7️⃣ Terraform Validation" -ForegroundColor Yellow
try {
    $tfVersion = terraform version 2>$null | Select-String "Terraform v" | ForEach-Object { $_.ToString().Split(' ')[1] }
    if ($tfVersion) {
        Add-ValidationResult "✅ Terraform" "PASS" "Terraform $tfVersion installed"
        
        # Validate Terraform files
        Push-Location "infrastructure"
        try {
            $tfValidation = terraform validate 2>&1
            if ($LASTEXITCODE -eq 0) {
                Add-ValidationResult "✅ Terraform Config" "PASS" "Terraform configuration valid"
            } else {
                Add-ValidationResult "❌ Terraform Config" "FAIL" "Terraform validation failed: $tfValidation"
            }
        } catch {
            Add-ValidationResult "❌ Terraform Config" "ERROR" "Terraform validation error: $_"
        }
        Pop-Location
    } else {
        Add-ValidationResult "❌ Terraform" "FAIL" "Terraform not installed"
    }
} catch {
    Add-ValidationResult "❌ Terraform" "ERROR" "Terraform check failed: $_"
}

# 8. Check GitHub Actions Workflows
Write-Host "8️⃣ GitHub Actions Validation" -ForegroundColor Yellow
$workflowFiles = @(
    ".github/workflows/ci-cd.yml",
    ".github/workflows/infrastructure.yml",
    ".github/workflows/setup.yml"
)

foreach ($workflow in $workflowFiles) {
    if (Test-Path $workflow) {
        try {
            $content = Get-Content $workflow -Raw
            if ($content -match "name:" -and $content -match "on:" -and $content -match "jobs:") {
                Add-ValidationResult "✅ GitHub Workflow" "PASS" "$workflow syntax appears valid"
            } else {
                Add-ValidationResult "❌ GitHub Workflow" "FAIL" "$workflow syntax invalid"
            }
        } catch {
            Add-ValidationResult "❌ GitHub Workflow" "ERROR" "$workflow read error: $_"
        }
    } else {
        Add-ValidationResult "❌ GitHub Workflow" "FAIL" "$workflow missing"
    }
}

# 9. Generate Summary Report
Write-Host ""
Write-Host "📊 Validation Summary" -ForegroundColor Green
Write-Host "====================" -ForegroundColor Green

$passCount = ($validationResults | Where-Object { $_.Status -eq "PASS" }).Count
$failCount = ($validationResults | Where-Object { $_.Status -eq "FAIL" }).Count
$warnCount = ($validationResults | Where-Object { $_.Status -eq "WARN" }).Count
$skipCount = ($validationResults | Where-Object { $_.Status -eq "SKIP" }).Count
$errorCount = ($validationResults | Where-Object { $_.Status -eq "ERROR" }).Count

Write-Host ""
Write-Host "📈 Results:" -ForegroundColor White
Write-Host "  ✅ Passed: $passCount" -ForegroundColor Green
Write-Host "  ❌ Failed: $failCount" -ForegroundColor Red
Write-Host "  ⚠️  Warnings: $warnCount" -ForegroundColor Yellow
Write-Host "  ⏭️  Skipped: $skipCount" -ForegroundColor Gray
Write-Host "  💥 Errors: $errorCount" -ForegroundColor Magenta
Write-Host ""

# Display detailed results
Write-Host "📋 Detailed Results:" -ForegroundColor White
Write-Host "===================" -ForegroundColor White
foreach ($result in $validationResults) {
    $statusColor = switch ($result.Status) {
        "PASS" { "Green" }
        "FAIL" { "Red" }
        "WARN" { "Yellow" }
        "SKIP" { "Gray" }
        "ERROR" { "Magenta" }
        default { "White" }
    }
    
    Write-Host "$($result.Test): $($result.Message)" -ForegroundColor $statusColor
    if ($result.Details) {
        Write-Host "  Details: $($result.Details)" -ForegroundColor Gray
    }
}

Write-Host ""

# Recommendations
if ($failCount -gt 0 -or $errorCount -gt 0) {
    Write-Host "❌ Deployment Readiness: NOT READY" -ForegroundColor Red
    Write-Host ""
    Write-Host "🛠️ Recommended Actions:" -ForegroundColor Yellow
    Write-Host "1. Fix all failed validations above" -ForegroundColor White
    Write-Host "2. Ensure .NET 9.0 SDK is installed" -ForegroundColor White
    Write-Host "3. Install Azure CLI and login: az login" -ForegroundColor White
    Write-Host "4. Install Terraform from https://terraform.io" -ForegroundColor White
    Write-Host "5. Set TestApps connection string environment variable" -ForegroundColor White
    Write-Host "6. Re-run this validation script" -ForegroundColor White
} else {
    Write-Host "✅ Deployment Readiness: READY!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🚀 Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Run setup script: .\scripts\Setup-AzureDeployment.ps1" -ForegroundColor White
    Write-Host "2. Configure GitHub secrets and environments" -ForegroundColor White
    Write-Host "3. Run GitHub Actions workflows for deployment" -ForegroundColor White
    Write-Host "4. Monitor deployment in Azure Portal" -ForegroundColor White
}

Write-Host ""
Write-Host "📋 Validation completed at $(Get-Date)" -ForegroundColor Green
