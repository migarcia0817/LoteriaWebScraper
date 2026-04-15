namespace LoteriaWebScraper
{
    public static class FechaHelper
    {
        private static readonly TimeZoneInfo SantoDomingoTZ =
            TimeZoneInfo.FindSystemTimeZoneById("America/Santo_Domingo");

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
