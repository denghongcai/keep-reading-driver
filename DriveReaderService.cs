using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Diagnostics;

namespace KeepReadingDriver
{
    public class DriveReaderService : ServiceBase
    {
        private readonly DriveReaderOptions _options;
        private Timer _timer;
        private bool _isRunning;

        public DriveReaderService(DriveReaderOptions options)
        {
            _options = options ?? new DriveReaderOptions();
            ServiceName = "KeepReadingDriver";
            
            // 确保事件日志源存在
            try
            {
                if (!EventLog.SourceExists("KeepReadingDriver"))
                {
                    EventLog.CreateEventSource("KeepReadingDriver", "Application");
                }
            }
            catch
            {
                // 忽略创建事件日志源的错误
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                WriteLog($"Keep Reading Driver Service starting. Monitoring drive: {_options.DriveLetter}, Interval: {_options.IntervalSeconds} seconds");
                StartService();
                WriteLog("Keep Reading Driver Service started successfully.");
            }
            catch (Exception ex)
            {
                WriteLog($"Error starting service: {ex.Message}", true);
                throw; // 重新抛出异常，让服务管理器知道启动失败
            }
        }

        protected override void OnStop()
        {
            try
            {
                WriteLog("Keep Reading Driver Service stopping.");
                StopService();
                WriteLog("Keep Reading Driver Service stopped successfully.");
            }
            catch (Exception ex)
            {
                WriteLog($"Error stopping service: {ex.Message}", true);
            }
        }

        public void StartConsole()
        {
            WriteLog($"Keep Reading Driver Console started. Monitoring drive: {_options.DriveLetter}, Interval: {_options.IntervalSeconds} seconds");
            StartService();
        }

        public void StopConsole()
        {
            WriteLog("Keep Reading Driver Console stopped.");
            StopService();
        }

        private void StartService()
        {
            try
            {
                _isRunning = true;

                // 确保盘符格式正确
                var drivePath = _options.DriveLetter.EndsWith(":") ? _options.DriveLetter : _options.DriveLetter + ":";
                if (!drivePath.EndsWith("\\"))
                {
                    drivePath += "\\";
                }

                // 检查驱动器是否存在
                if (!Directory.Exists(drivePath))
                {
                    throw new DirectoryNotFoundException($"Drive {drivePath} does not exist or is not accessible.");
                }

                WriteLog($"Drive {drivePath} is accessible, starting monitoring...");

                // 创建定时器
                _timer = new Timer(TimerCallback, drivePath, TimeSpan.Zero, TimeSpan.FromSeconds(_options.IntervalSeconds));
                
                WriteLog($"Timer created with interval: {_options.IntervalSeconds} seconds");
            }
            catch (Exception ex)
            {
                _isRunning = false;
                WriteLog($"Failed to start service: {ex.Message}", true);
                throw;
            }
        }

        private void StopService()
        {
            try
            {
                _isRunning = false;
                _timer?.Dispose();
                _timer = null;
                WriteLog("Service stopped successfully.");
            }
            catch (Exception ex)
            {
                WriteLog($"Error during service stop: {ex.Message}", true);
            }
        }

        private void TimerCallback(object state)
        {
            if (!_isRunning) return;

            var drivePath = (string)state;
            try
            {
                ReadDrive(drivePath);
                WriteLog($"Successfully read drive {drivePath}", false);
            }
            catch (Exception ex)
            {
                WriteLog($"Error reading drive {drivePath}: {ex.Message}");
            }
        }

        private void ReadDrive(string drivePath)
        {
            try
            {
                // 读取根目录，获取文件和文件夹信息
                var entries = Directory.GetFileSystemEntries(drivePath);
                WriteLog($"Found {entries.Length} entries in {drivePath}", false);

                // 如果根目录为空，尝试读取磁盘信息
                if (entries.Length == 0)
                {
                    var driveInfo = new DriveInfo(drivePath);
                    if (driveInfo.IsReady)
                    {
                        WriteLog($"Drive {drivePath} - Total Size: {driveInfo.TotalSize / (1024 * 1024 * 1024)} GB, " +
                               $"Available Space: {driveInfo.AvailableFreeSpace / (1024 * 1024 * 1024)} GB", false);
                    }
                }

                // 为了确保磁盘活动，我们也可以尝试访问一个小文件
                TryAccessTestFile(drivePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                WriteLog($"Access denied to {drivePath}: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                WriteLog($"Directory not found {drivePath}: {ex.Message}");
            }
            catch (IOException ex)
            {
                WriteLog($"IO error accessing {drivePath}: {ex.Message}");
            }
        }

        private void TryAccessTestFile(string drivePath)
        {
            try
            {
                var testFilePath = Path.Combine(drivePath, ".keep_alive_test");
                
                // 尝试创建一个很小的临时文件
                File.WriteAllText(testFilePath, DateTime.Now.ToString());
                
                // 立即读取并删除
                if (File.Exists(testFilePath))
                {
                    File.ReadAllText(testFilePath);
                    File.Delete(testFilePath);
                    WriteLog($"Test file operation completed on {drivePath}", false);
                }
            }
            catch (Exception ex)
            {
                // 如果无法创建测试文件（如只读驱动器），忽略错误
                WriteLog($"Could not create test file on {drivePath}: {ex.Message}", false);
            }
        }

        private void WriteLog(string message, bool isError = false)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logMessage = $"{timestamp} - {message}";

                if (Environment.UserInteractive)
                {
                    // 控制台模式
                    if (isError)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"ERROR: {logMessage}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine(logMessage);
                    }
                }
                else
                {
                    // 服务模式，写入事件日志
                    try
                    {
                        using (EventLog eventLog = new EventLog("Application"))
                        {
                            eventLog.Source = "KeepReadingDriver";
                            var entryType = isError ? EventLogEntryType.Error : EventLogEntryType.Information;
                            eventLog.WriteEntry(logMessage, entryType);
                        }
                    }
                    catch (Exception ex)
                    {
                        // 如果无法写入事件日志，尝试写入到文件
                        try
                        {
                            var logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
                                                     "KeepReadingDriver", "service.log");
                            Directory.CreateDirectory(Path.GetDirectoryName(logFile));
                            File.AppendAllText(logFile, $"{logMessage} (EventLog Error: {ex.Message})\r\n");
                        }
                        catch
                        {
                            // 最后的备选方案：什么都不做，避免服务崩溃
                        }
                    }
                }
            }
            catch
            {
                // 确保日志记录不会导致服务崩溃
            }
        }
    }
}
