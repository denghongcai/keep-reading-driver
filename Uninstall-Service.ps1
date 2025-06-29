# Uninstall Keep Reading Driver Service PowerShell Script

param(
    [string]$ServiceName = "KeepReadingDriver"
)

# Check if running as administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator))
{
    Write-Host "This script requires administrator privileges. Please run PowerShell as administrator." -ForegroundColor Red
    pause
    exit 1
}

try {
    # Check if service exists
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if (-not $service) {
        Write-Host "Service '$ServiceName' does not exist." -ForegroundColor Yellow
        pause
        exit 0
    }

    Write-Host "Stopping service '$ServiceName'..." -ForegroundColor Yellow
    
    # Stop service
    if ($service.Status -eq 'Running') {
        Stop-Service -Name $ServiceName -Force
        Write-Host "Service stopped." -ForegroundColor Green
    }
    
    # Delete service
    Write-Host "Removing service..." -ForegroundColor Yellow
    sc.exe delete $ServiceName
    
    # Remove event log source
    Write-Host "Removing event log source..." -ForegroundColor Yellow
    try {
        if ([System.Diagnostics.EventLog]::SourceExists("KeepReadingDriver")) {
            [System.Diagnostics.EventLog]::DeleteEventSource("KeepReadingDriver")
            Write-Host "Event log source removed successfully." -ForegroundColor Green
        } else {
            Write-Host "Event log source does not exist." -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "Warning: Could not remove event log source: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    
    Write-Host "Service '$ServiceName' has been successfully uninstalled." -ForegroundColor Green
}
catch {
    Write-Host "Error uninstalling service: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

pause
