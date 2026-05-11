using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            var fechaAyer = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");

            var scraper = new ScraperService(null); // inyecta logger si lo tienes
            var resultados = await scraper.ObtenerNumerosGanadoresAsync();

            // Filtrar New York Noche del día anterior
            var nyNoche = resultados
                .Where(r => r.Loteria.ToUpperInvariant().StartsWith("NEW YORK"))
                .Where(r => DateTime.TryParse(r.Fecha, out DateTime fecha) && fecha.ToString("yyyy-MM-dd") == fechaAyer)
                .Where(r =>
                {
                    if (DateTime.TryParse(r.Hora, out DateTime horaNY))
                        return (horaNY.Hour == 22 && horaNY.Minute == 30) || (horaNY.Hour == 23 && horaNY.Minute == 30);
                    return false;
                })
                .ToList();

            if (!nyNoche.Any())
                return NotFound($"No se encontraron resultados de N.Y Noche para {fechaAyer}");

            var numeros = nyNoche.Select(r => r.Numero).Take(3).ToList();
            var primerPremio = numeros.ElementAtOrDefault(0) ?? "";
            var segundoPremio = numeros.ElementAtOrDefault(1) ?? "";
            var tercerPremio = numeros.ElementAtOrDefault(2) ?? "";

            // ✅ Devolver solo los números ganadores
            return Ok(new
            {
                FechaSorteo = fechaAyer,
                LoteriaClave = "NYNoche_1130_PM",
                LoteriaNombre = "N.Y Noche 11:30 PM",
                PrimerPremio = primerPremio,
                SegundoPremio = segundoPremio,
                TercerPremio = tercerPremio
            });
        }
    }
}
