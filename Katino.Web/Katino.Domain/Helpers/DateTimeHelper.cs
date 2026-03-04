namespace Katino.Domain.Helpers;

public class DateTimeHelper
{
    public static DateTime GetCurrentKyivDateTime()
    {
        var utcNow = DateTime.UtcNow;

        TimeZoneInfo tz;
        try
        {
            // Linux / IANA
            tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        }
        catch (TimeZoneNotFoundException)
        {
            // Windows
            tz = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        }
        catch (InvalidTimeZoneException)
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        }

        return TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
    }
}
