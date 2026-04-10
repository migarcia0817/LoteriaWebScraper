using Microsoft.AspNetCore.Mvc;

namespace LoteriaWebScraper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScraperController : ControllerBase
    {
        private readonly ScraperService _scraper;

        public ScraperController(ScraperService scraper)
        {
            _scraper = scraper;
        }

        // GET api/scraper/run
        [HttpGet("run")]
        public async Task<IActionResult> Run()
        {
            var resultados = await _scraper.ObtenerNumerosGanadoresAsync();
            await _scraper.GuardarResultadosEnFirebase(resultados);

            return Ok(new
            {
                message = "✅ Resultados actualizados en Firebase",
                count = resultados.Count
            });
        }
    }
}
