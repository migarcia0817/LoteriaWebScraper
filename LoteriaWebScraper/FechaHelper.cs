namespace LoteriaWebScraper
{
    public static class FechaHelper
    {
        private static readonly TimeZoneInfo SantoDomingoTZ = GetTimeZone();

        private static TimeZoneInfo GetTimeZone()
        {
            try
            {
                // Linux / Render usa IANA
                return TimeZoneInfo.FindSystemTimeZoneById("America/Santo_Domingo");
            }
            catch (TimeZoneNotFoundException)
            {
                // Windows usa Registry
                return TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
            }
        }

        public static string GetFechaLocal()
        {
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SantoDomingoTZ);
            return localTime.ToString("yyyy-MM-dd");
        }

        public static DateTime GetDateTimeLocal()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SantoDomingoTZ);
        }
    }
}
