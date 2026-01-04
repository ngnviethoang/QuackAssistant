namespace QuackAssistant.Shared;

public static class Helper
{
    public static TimeZoneInfo GetVietnamTimeZone()
    {
        if (OperatingSystem.IsWindows()) return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        if (OperatingSystem.IsLinux()) return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");

        return TimeZoneInfo.FindSystemTimeZoneById("UTC");
    }
}