public static class FechaHelper
{
    private static readonly TimeZoneInfo SantoDomingoTZ = GetTimeZone();

    private static TimeZoneInfo GetTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Santo_Domingo"); // Render/Linux
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time"); // Windows
        }
    }

    public static string GetFechaLocal()
    {
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SantoDomingoTZ);
        return localTime.ToString("yyyy-MM-dd");
    }
}
