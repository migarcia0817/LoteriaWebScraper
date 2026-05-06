using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoteriaWebScraper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProbandonyController : ControllerBase
    {
        // 👉 Endpoint de prueba
        [HttpGet("probar-normalizacion")]
        public IActionResult ProbarNormalizacion()
        {
            string nombre = "New York Noche";
            string horaNormalizada = "10:30PM"; // igual que lo que llega del scraper

            string clave = NormalizarNombre(nombre, horaNormalizada);

            return Ok(new { clave });
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
