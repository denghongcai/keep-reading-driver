using System;
using System.ServiceProcess;

namespace KeepReadingDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = ParseCommandLineArgs(args);

            if (Environment.UserInteractive)
            {
                // 以控制台模式运行（调试用）
                Console.WriteLine("Keep Reading Driver - Console Mode");
                Console.WriteLine($"Monitoring drive: {options.DriveLetter}, Interval: {options.IntervalSeconds} seconds");
                Console.WriteLine("Press Ctrl+C to exit...");

                var service = new DriveReaderService(options);
                service.StartConsole();

                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    service.StopConsole();
                };

                Console.ReadKey();
            }
            else
            {
                // 作为 Windows 服务运行
                ServiceBase[] servicesToRun = { new DriveReaderService(options) };
                ServiceBase.Run(servicesToRun);
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
                    case "-h":
                    case "--help":
                        Console.WriteLine("KeepReadingDriver - Windows Service to prevent hard drive sleep");
                        Console.WriteLine("Usage: KeepReadingDriver.exe [options]");
                        Console.WriteLine("Options:");
                        Console.WriteLine("  -d, --drive <letter>     Drive letter to read (default: C)");
                        Console.WriteLine("  -i, --interval <seconds> Interval in seconds (default: 300)");
                        Console.WriteLine("  -h, --help              Show this help message");
                        Environment.Exit(0);
                        break;
                }
            }

            return options;
        }
    }
}
