using System;
using System.ServiceProcess;
using System.Threading;

namespace KeepReadingDriver
{
    class Program
    {
        private static bool _isRunningAsService = false;
        private static DriveReaderOptions _options;
        
        static void Main(string[] args)
        {
            try
            {
                _options = ParseCommandLineArgs(args);

                if (!_options.RunAsService)
                {
                    // 以控制台模式运行（调试用）
                    Console.WriteLine("Keep Reading Driver - Console Mode");
                    Console.WriteLine($"Monitoring drive: {_options.DriveLetter}, Interval: {_options.IntervalSeconds} seconds");
                    Console.WriteLine("Press Ctrl+C to exit...");

                    var service = new DriveReaderService(_options, false); // 传递 false 表示控制台模式
                    var exitRequested = false;
                    
                    service.StartConsole();

                    Console.CancelKeyPress += (sender, e) =>
                    {
                        e.Cancel = true; // 阻止立即终止，让我们优雅地退出
                        Console.WriteLine();
                        Console.WriteLine("Exit requested, stopping service...");
                        service.StopConsole();
                        exitRequested = true;
                    };

                    // 保持程序运行直到请求退出
                    while (!exitRequested)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    
                    Console.WriteLine("Service stopped. Press any key to exit...");
                    Console.ReadKey();
                }
                else
                {
                    // 作为 Windows 服务运行
                    _isRunningAsService = true;
                    ServiceBase[] servicesToRun = { new DriveReaderService(_options, true) }; // 传递 true 表示服务模式
                    ServiceBase.Run(servicesToRun);
                }
            }
            catch (Exception ex)
            {
                if (!_isRunningAsService && (_options == null || !_options.RunAsService))
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                }
                else
                {
                    // 写入事件日志
                    try
                    {
                        using (var eventLog = new System.Diagnostics.EventLog("Application"))
                        {
                            eventLog.Source = "KeepReadingDriver";
                            eventLog.WriteEntry($"Service startup error: {ex.Message}", 
                                              System.Diagnostics.EventLogEntryType.Error);
                        }
                    }
                    catch { }
                }
                Environment.Exit(1);
            }
        }

        private static DriveReaderOptions ParseCommandLineArgs(string[] args)
        {
            var options = new DriveReaderOptions();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-d":
                    case "--drive":
                        if (i + 1 < args.Length)
                        {
                            options.DriveLetter = args[++i];
                        }
                        break;
                    case "-i":
                    case "--interval":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int interval))
                        {
                            options.IntervalSeconds = interval;
                        }
                        break;
                    case "-s":
                    case "--service":
                        options.RunAsService = true;
                        break;
                    case "-h":
                    case "--help":
                        Console.WriteLine("KeepReadingDriver - Windows Service to prevent hard drive sleep");
                        Console.WriteLine("Usage: KeepReadingDriver.exe [options]");
                        Console.WriteLine("Options:");
                        Console.WriteLine("  -d, --drive <letter>     Drive letter to read (default: C)");
                        Console.WriteLine("  -i, --interval <seconds> Interval in seconds (default: 300)");
                        Console.WriteLine("  -s, --service           Run as Windows Service (used by Service Control Manager)");
                        Console.WriteLine("  -h, --help              Show this help message");
                        Environment.Exit(0);
                        break;
                }
            }

            return options;
        }
    }
}
