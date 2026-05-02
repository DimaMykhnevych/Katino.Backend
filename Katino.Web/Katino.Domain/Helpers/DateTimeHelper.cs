namespace Katino.Domain.Helpers;

public class DateTimeHelper
{
    public static DateTime GetCurrentKyivDateTime()
    {
        var utcNow = DateTime.UtcNow;
        return TimeZoneInfo.ConvertTimeFromUtc(utcNow, GetKyivTimeZone());
    }

    public static DateTime ToKyivDateTime(DateTimeOffset dt)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(dt.UtcDateTime, GetKyivTimeZone());
    }

    private static TimeZoneInfo GetKyivTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        }
    }
}
