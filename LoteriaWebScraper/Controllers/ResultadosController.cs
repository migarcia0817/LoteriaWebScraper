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

        // ✅ Endpoint para mostrar resultados de NY Noche (sin filtrar fecha)
        [HttpGet("ny-noche")]
        public async Task<IActionResult> ObtenerNyNoche()
        {
            var scraper = new ScraperService(null);
            var resultados = await scraper.ObtenerNumerosGanadoresAsync();

            var nyNoche = resultados
                .Where(r => r.Loteria.ToUpperInvariant().Contains("NEW YORK"))
                .Where(r =>
                {
                    // ✅ Normalización de hora
                    var horaNormalizada = NormalizarNombre(r.Loteria, r.Hora);
                    return horaNormalizada == "NY.Noche 11:30 PM";
                })
                .ToList();

            if (!nyNoche.Any())
                return NotFound("No se encontraron resultados de N.Y Noche");

            var numeros = nyNoche.Select(r => r.Numero).Take(3).ToList();

            return Ok(new
            {
                LoteriaClave = "NYNoche_1130_PM",
                LoteriaNombre = "N.Y Noche 11:30 PM",
                PrimerPremio = numeros.ElementAtOrDefault(0) ?? "",
                SegundoPremio = numeros.ElementAtOrDefault(1) ?? "",
                TercerPremio = numeros.ElementAtOrDefault(2) ?? ""
            });
        }

        // 👉 Método de normalización robusto
        private string NormalizarNombre(string nombre, string horaNormalizada)
        {
            if (nombre.ToUpperInvariant().StartsWith("NEW YORK"))
            {
                var hora = horaNormalizada.Replace(" ", "").ToUpperInvariant();

                if (hora.Contains("2:30PM"))
                    return "NY.Tarde 3:30 PM";

                if (hora.Contains("10:30PM") || hora.Contains("11:30PM"))
                    return "NY.Noche 11:30 PM";
            }

            return nombre; // fallback si no coincide
        }
    }
}
