# 安装 Keep Reading Driver 服务的 PowerShell 脚本

param(
    [string]$Drive = "C",
    [int]$Interval = 300,
    [string]$ServiceName = "KeepReadingDriver",
    [string]$DisplayName = "Keep Reading Driver Service",
    [string]$Description = "定期读取指定磁盘驱动器以防止其进入休眠状态"
)

# 检查是否以管理员身份运行
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
    Write-Host "此脚本需要管理员权限运行。请以管理员身份重新运行 PowerShell。" -ForegroundColor Red
    pause
    exit 1
}

# 获取当前脚本所在目录
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ExePath = Join-Path $ScriptDir "bin\Release\netcoreapp2.1\win-x64\publish\KeepReadingDriver.exe"

# 检查可执行文件是否存在
if (-not (Test-Path $ExePath)) {
    Write-Host "找不到可执行文件: $ExePath" -ForegroundColor Red
    Write-Host "请先发布项目：dotnet publish -c Release -r win-x64 --self-contained" -ForegroundColor Yellow
    pause
    exit 1
}

try {
    # 停止服务（如果正在运行）
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($service) {
        Write-Host "正在停止现有服务..." -ForegroundColor Yellow
        Stop-Service -Name $ServiceName -Force
        
        Write-Host "正在删除现有服务..." -ForegroundColor Yellow
        sc.exe delete $ServiceName
        Start-Sleep -Seconds 2
    }

    # 创建服务
    Write-Host "正在安装服务..." -ForegroundColor Green
    $BinaryPath = "`"$ExePath`" --drive $Drive --interval $Interval"
    
    sc.exe create $ServiceName binPath= $BinaryPath DisplayName= $DisplayName start= auto
    sc.exe description $ServiceName $Description
    
    # 启动服务
    Write-Host "正在启动服务..." -ForegroundColor Green
    Start-Service -Name $ServiceName
    
    # 检查服务状态
    $service = Get-Service -Name $ServiceName
    Write-Host "服务状态: $($service.Status)" -ForegroundColor Green
    
    Write-Host "`n服务安装成功！" -ForegroundColor Green
    Write-Host "服务名称: $ServiceName" -ForegroundColor Cyan
    Write-Host "监控驱动器: $Drive" -ForegroundColor Cyan
    Write-Host "检查间隔: $Interval 秒" -ForegroundColor Cyan
    Write-Host "`n可以使用以下命令管理服务："
    Write-Host "启动: Start-Service -Name $ServiceName" -ForegroundColor Gray
    Write-Host "停止: Stop-Service -Name $ServiceName" -ForegroundColor Gray
    Write-Host "查看状态: Get-Service -Name $ServiceName" -ForegroundColor Gray
    Write-Host "卸载: .\Uninstall-Service.ps1" -ForegroundColor Gray
}
catch {
    Write-Host "安装服务时出错: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

pause
