namespace Katino.Domain.Helpers;

public class DateTimeHelper
{
    public static DateTime GetCurrentKyivDateTime()
    {
        var kyivTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, kyivTimeZone);
    }
}
