using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace LoteriaWebScraper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultadosController : ControllerBase
    {
        private readonly FirebaseClient _firebase;

        public ResultadosController()
        {
            _firebase = new FirebaseClient(
                "https://bancachupon-default-rtdb.firebaseio.com/",
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () =>
                        Task.FromResult(Environment.GetEnvironmentVariable("FIREBASE_SECRET"))
                });
        }

        // ✅ Endpoint para mostrar resultados de NY Noche del día anterior
        [HttpGet("ny-noche-ayer")]
        public async Task<IActionResult> ObtenerNyNocheAyer()
        {
            var fechaAyer = DateTime.Today.AddDays(-1).Date;

            var scraper = new ScraperService(null);
            var resultados = await scraper.ObtenerNumerosGanadoresAsync();

            var nyNoche = resultados
                .Where(r => r.Loteria.ToUpperInvariant().StartsWith("NEW YORK"))
                .Where(r =>
                {
                    if (DateTime.TryParse(r.Fecha, new CultureInfo("es-ES"),
                                          DateTimeStyles.None, out DateTime fecha))
                        return fecha.Date == fechaAyer;
                    return false;
                })
                .Where(r =>
                {
                    var hora1 = r.Hora.Replace(" ", "").ToUpperInvariant();
                    return hora1.Contains("10:30PM") || hora1.Contains("11:30PM");
                })
                .ToList();

            if (!nyNoche.Any())
                return NotFound($"No se encontraron resultados de N.Y Noche para {fechaAyer:yyyy-MM-dd}");

            var numeros = nyNoche.Select(r => r.Numero).Take(3).ToList();

            return Ok(new
            {
                FechaSorteo = fechaAyer.ToString("yyyy-MM-dd"),
                LoteriaClave = "NYNoche_1130_PM",
                LoteriaNombre = "N.Y Noche 11:30 PM",
                PrimerPremio = numeros.ElementAtOrDefault(0) ?? "",
                SegundoPremio = numeros.ElementAtOrDefault(1) ?? "",
                TercerPremio = numeros.ElementAtOrDefault(2) ?? ""
            });
        }
    }
}
