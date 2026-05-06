using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
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

        // ✅ Ahora Swagger mostrará textboxes separados
        [HttpPost("publicar-ny-noche")]
        public async Task<IActionResult> PublicarNyNoche(
            [FromQuery] string fechaSorteo,
            [FromQuery] string primerPremio,
            [FromQuery] string segundoPremio,
            [FromQuery] string tercerPremio)
        {
            if (string.IsNullOrEmpty(fechaSorteo))
                return BadRequest("Fecha inválida");

            await _firebase
                .Child("Resultados")
                .Child("NYNoche_1130_PM")
                .Child(fechaSorteo)
                .PutAsync(new
                {
                    FechaSorteo = fechaSorteo,
                    LoteriaClave = "NYNoche_1130_PM",
                    LoteriaNombre = "N.Y Noche 11:30 PM",
                    PrimerPremio = primerPremio,
                    SegundoPremio = segundoPremio,
                    TercerPremio = tercerPremio
                });

            return Ok(new { mensaje = $"Resultado de N.Y Noche publicado para {fechaSorteo}" });
        }
    }
}
