using Microsoft.AspNetCore.Mvc;

namespace LoteriaWebScraper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LimpiezaController : ControllerBase
    {
        private readonly ScraperService _scraper;

        public LimpiezaController(ScraperService scraper)
        {
            _scraper = scraper;
        }

        // Endpoint: POST /api/limpieza/fechas-futuras
        [HttpPost("fechas-futuras")]
        public async Task<IActionResult> LimpiarFechasFuturas()
        {
            await _scraper.LimpiarFechasFuturas();
            return Ok("Fechas futuras eliminadas.");
        }
    }
}

