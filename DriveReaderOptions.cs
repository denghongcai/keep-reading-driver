namespace KeepReadingDriver
{
    public class DriveReaderOptions
    {
        public string DriveLetter { get; set; } = "C";
        public int IntervalSeconds { get; set; } = 300; // 默认5分钟
        public bool RunAsService { get; set; } = false; // 默认为控制台模式
    }
}
