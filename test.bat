@echo off
echo 测试 Keep Reading Driver（控制台模式）
echo 按 Ctrl+C 退出测试
echo.
echo 使用参数: --drive C --interval 10 （10秒间隔进行测试）
echo.
.\bin\Release\netcoreapp2.1\win-x64\publish\KeepReadingDriver.exe --drive C --interval 10
