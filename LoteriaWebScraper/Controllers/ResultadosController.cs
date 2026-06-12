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

        // ✅ Endpoint manual para publicar NY Noche de una fecha específica
        [HttpPost("publicar-ny-noche-manual")]
        public async Task<IActionResult> PublicarNyNocheManual([FromQuery] string fecha,
                                                               [FromQuery] string primerPremio,
                                                               [FromQuery] string segundoPremio,
                                                               [FromQuery] string tercerPremio)
        {
            if (string.IsNullOrWhiteSpace(fecha))
                return BadRequest("Debe especificar la fecha en formato yyyy-MM-dd");

            var resultado = new
            {
                FechaSorteo = fecha,
                LoteriaClave = "NYNoche_1130_PM",
                LoteriaNombre = "N.Y Noche 11:30 PM",
                PrimerPremio = primerPremio ?? "",
                SegundoPremio = segundoPremio ?? "",
                TercerPremio = tercerPremio ?? ""
            };

            await _firebase
                .Child("Resultados")
                .Child("NYNoche_1130_PM")
                .Child(fecha)
                .PutAsync(resultado);

            return Ok($"✅ Resultado de N.Y Noche publicado para {fecha}");
        }
    }
}
