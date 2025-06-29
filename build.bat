@echo off
echo 正在编译 Keep Reading Driver...
dotnet build -c Release

if %errorlevel% neq 0 (
    echo 编译失败！
    pause
    exit /b 1
)

echo 正在发布自包含可执行文件...
dotnet publish -c Release -r win-x64 --self-contained

if %errorlevel% neq 0 (
    echo 发布失败！
    pause
    exit /b 1
)

echo 编译和发布完成！
echo 可执行文件位置: bin\Release\netcoreapp2.1\win-x64\publish\KeepReadingDriver.exe
echo.
echo 下一步操作：
echo 1. 以管理员身份运行 PowerShell
echo 2. 执行 .\Install-Service.ps1 来安装服务
pause
