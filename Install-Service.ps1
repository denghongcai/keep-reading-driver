# Install Keep Reading Driver Service PowerShell Script

param(
    [string]$Drive = "C",
    [int]$Interval = 300,
    [string]$ServiceName = "KeepReadingDriver",
    [string]$DisplayName = "Keep Reading Driver Service",
    [string]$Description = "Periodically reads specified disk drive to prevent it from going to sleep"
)

# Check if running as administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator))
{
    Write-Host "This script requires administrator privileges. Please run PowerShell as administrator." -ForegroundColor Red
    pause
    exit 1
}

# Get current script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ExePath = Join-Path $ScriptDir "bin\Release\netcoreapp2.1\win-x64\publish\KeepReadingDriver.exe"

# Check if executable file exists
if (-not (Test-Path $ExePath)) {
    Write-Host "Executable file not found: $ExePath" -ForegroundColor Red
    Write-Host "Please publish the project first: dotnet publish -c Release -r win-x64 --self-contained" -ForegroundColor Yellow
    pause
    exit 1
}

try {
    # Stop service if running
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($service) {
        Write-Host "Stopping existing service..." -ForegroundColor Yellow
        Stop-Service -Name $ServiceName -Force
        
        Write-Host "Removing existing service..." -ForegroundColor Yellow
        sc.exe delete $ServiceName
        Start-Sleep -Seconds 2
    }

    # Create service
    Write-Host "Installing service..." -ForegroundColor Green
    $BinaryPath = "`"$ExePath`" --drive $Drive --interval $Interval --service"
    
    sc.exe create $ServiceName binPath= $BinaryPath DisplayName= $DisplayName start= auto
    sc.exe description $ServiceName $Description
    
    # Start service
    Write-Host "Starting service..." -ForegroundColor Green
    Start-Service -Name $ServiceName
    
    # Check service status
    $service = Get-Service -Name $ServiceName
    Write-Host "Service status: $($service.Status)" -ForegroundColor Green
    
    Write-Host "`nService installed successfully!" -ForegroundColor Green
    Write-Host "Service name: $ServiceName" -ForegroundColor Cyan
    Write-Host "Monitoring drive: $Drive" -ForegroundColor Cyan
    Write-Host "Check interval: $Interval seconds" -ForegroundColor Cyan
    Write-Host "`nYou can use the following commands to manage the service:"
    Write-Host "Start: Start-Service -Name $ServiceName" -ForegroundColor Gray
    Write-Host "Stop: Stop-Service -Name $ServiceName" -ForegroundColor Gray
    Write-Host "Status: Get-Service -Name $ServiceName" -ForegroundColor Gray
    Write-Host "Uninstall: .\Uninstall-Service.ps1" -ForegroundColor Gray
}
catch {
    Write-Host "Error installing service: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

pause
