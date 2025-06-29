# Keep Reading Driver - Windows 硬盘防休眠服务

这是一个 Windows 服务程序，用于定期读取指定的磁盘驱动器，防止硬盘进入休眠状态。

## 功能特性

- 支持作为 Windows 服务运行
- 可配置监控的驱动器盘符
- 可配置检查时间间隔
- 支持命令行参数
- 详细的日志记录
- 自动处理权限和访问错误

## 使用方法

### 1. 编译项目

```powershell
# 编译
dotnet build -c Release

# 发布自包含可执行文件
dotnet publish -c Release -r win-x64 --self-contained
```

### 2. 安装服务

以管理员身份运行 PowerShell，然后执行：

```powershell
# 使用默认参数安装（监控 C 盘，间隔 300 秒）
.\Install-Service.ps1

# 自定义参数安装
.\Install-Service.ps1 -Drive "D" -Interval 180
```

### 3. 管理服务

```powershell
# 查看服务状态
Get-Service -Name KeepReadingDriver

# 启动服务
Start-Service -Name KeepReadingDriver

# 停止服务
Stop-Service -Name KeepReadingDriver

# 卸载服务
.\Uninstall-Service.ps1
```

### 4. 手动运行（调试用）

```powershell
# 默认参数运行
.\bin\Release\netcoreapp2.1\win-x64\publish\KeepReadingDriver.exe

# 自定义参数运行
.\bin\Release\netcoreapp2.1\win-x64\publish\KeepReadingDriver.exe --drive D --interval 180

# 查看帮助
.\bin\Release\netcoreapp2.1\win-x64\publish\KeepReadingDriver.exe --help
```

## 命令行参数

- `-d, --drive <letter>`: 要监控的驱动器盘符（默认：C）
- `-i, --interval <seconds>`: 检查间隔秒数（默认：300）
- `-h, --help`: 显示帮助信息

## 工作原理

服务每隔指定的时间间隔会执行以下操作：

1. 读取指定驱动器的根目录列表
2. 获取驱动器基本信息（总空间、可用空间等）
3. 尝试在根目录创建一个临时测试文件并立即删除（如果权限允许）

这些操作足以让操作系统认为驱动器处于活动状态，从而防止其进入休眠。

## 日志查看

服务运行时会记录详细日志，可以通过以下方式查看：

1. **事件查看器**：Windows 日志 → 应用程序，查找来源为 "KeepReadingDriver" 的事件
2. **控制台输出**：手动运行时可以直接在控制台看到日志

## 注意事项

- 需要管理员权限才能安装和管理服务
- 确保指定的驱动器存在且可访问
- 对于只读驱动器，服务会跳过文件创建操作，仅执行读取操作
- 服务会自动处理权限不足等错误情况，不会因为单次失败而停止

## 系统要求

- Windows 10/11 或 Windows Server 2016+
- .NET Core 2.1 Runtime（如果使用发布的自包含版本则不需要）

## 故障排除

1. **服务无法启动**：检查目标驱动器是否存在
2. **权限错误**：确保以管理员身份运行安装脚本
3. **找不到可执行文件**：先运行 `dotnet publish -c Release -r win-x64 --self-contained` 发布项目
