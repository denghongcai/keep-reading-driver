# 卸载 Keep Reading Driver 服务的 PowerShell 脚本

param(
    [string]$ServiceName = "KeepReadingDriver"
)

# 检查是否以管理员身份运行
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
    Write-Host "此脚本需要管理员权限运行。请以管理员身份重新运行 PowerShell。" -ForegroundColor Red
    pause
    exit 1
}

try {
    # 检查服务是否存在
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if (-not $service) {
        Write-Host "服务 '$ServiceName' 不存在。" -ForegroundColor Yellow
        pause
        exit 0
    }

    Write-Host "正在停止服务 '$ServiceName'..." -ForegroundColor Yellow
    
    # 停止服务
    if ($service.Status -eq 'Running') {
        Stop-Service -Name $ServiceName -Force
        Write-Host "服务已停止。" -ForegroundColor Green
    }
    
    # 删除服务
    Write-Host "正在删除服务..." -ForegroundColor Yellow
    sc.exe delete $ServiceName
    
    Write-Host "服务 '$ServiceName' 已成功卸载。" -ForegroundColor Green
}
catch {
    Write-Host "卸载服务时出错: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

pause
