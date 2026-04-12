using Firebase.Database;
using Firebase.Database.Query;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace LoteriaWebScraper
{
    public class ScraperService
    {
        private readonly ILogger<ScraperService> _logger;
        private readonly FirebaseClient _firebaseClient;

        // Diccionario de mapeo entre nombre y clave (claves limpias para Firebase)
        private static readonly Dictionary<string, string> LoteriaClaves = new Dictionary<string, string>
        {
            { "Anguilla 08:00 AM", "Anguilla_800_AM" },
            { "Anguilla 09:00 AM", "Anguilla_900_AM" },
            { "Anguilla 10:00 AM", "Anguilla_1000_AM" },
            { "Anguilla 11:00 AM", "Anguilla_1100_AM" },
            { "Anguilla 12:00 PM", "Anguilla_1200_PM" },
            { "Anguilla 1:00 PM", "Anguilla_100_PM" },
            { "Anguilla 2:00 PM", "Anguilla_200_PM" },
            { "Anguilla 3:00 PM", "Anguilla_300_PM" },
            { "Anguilla 4:00 PM", "Anguilla_400_PM" },
            { "Anguilla 5:00 PM", "Anguilla_500_PM" },
            { "Anguilla 6:00 PM", "Anguilla_600_PM" },
            { "Anguilla 7:00 PM", "Anguilla_700_PM" },
            { "Anguilla 8:00 PM", "Anguilla_800_PM" },
            { "Anguilla 9:00 PM", "Anguilla_900_PM" },
            { "Anguilla 10:00 PM", "Anguilla_1000_PM"},
            { "NY.Tarde 3:30 PM", "NYTarde_330PM"},
            { "NY.Noche 11:30 PM", "NYNoche_1130PM"},
            { "King Lotery 12:30 PM", "KingLot_1230PM"},
            { "King Lotery 7:30 PM", "KingLot_730PM"},
            { "Suerte 12:30 PM", "Suerte_1230PM"},
            { "Suerte 6:00 PM", "Suerte_600PM"},
            { "Primera 12:00 PM", "Primera_1200PM"},
            { "Primera 7:00 PM", "Primera_700PM"},
            { "Q.Real Tarde 12:55 PM", "QRealTarde_1255PM" },
            { "FL.Tarde 1:30 PM", "FLTarde_130PM"},
            { "FL.Noche 9:45 PM", "FLNoche_945PM" },
            { "Loteka 7:55 PM", "Loteka_755PM"},
            { "Gana Mas 2:30 PM", "GanaMas_230PM"},
            { "Nacional", "NacNoche_900PM"},
            { "Leidsa", "Leidsa_850PM"},
            { "Haiti Bolet 9:30 AM", "HaitiBolet_930AM" },
            { "Haiti Bolet 10:30 AM", "HaitiBolet_1030AM" },
            { "Haiti Bolet 11:30 AM", "HaitiBolet_1130AM" },
            { "Haiti Bolet 5:30 PM", "HaitiBolet_530PM" },
            { "Haiti Bolet 6:30 PM", "HaitiBolet_630PM" },
            { "Haiti Bolet 7:30 PM", "HaitiBolet_730PM" }
        };

        public ScraperService(ILogger<ScraperService> logger)
        {
            _logger = logger;
            _firebaseClient = new FirebaseClient("https://bancachupon-default-rtdb.firebaseio.com/");
        }

        public async Task<List<(string Loteria, string Fecha, string Hora, string Numero)>> ObtenerNumerosGanadoresAsync()
        {
            var url = "https://enloteria.com";
            using var client = new HttpClient();
            var html = await client.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var resultados = new List<(string Loteria, string Fecha, string Hora, string Numero)>();

            var loteriaBloques = doc.DocumentNode.SelectNodes("//div[contains(@class,'card-body')]");
            if (loteriaBloques != null)
            {
                foreach (var bloque in loteriaBloques)
                {
                    var nombre = bloque.SelectSingleNode(".//h5[contains(@class,'lottery-name')]")?.InnerText.Trim() ?? "Desconocida";
                    var fecha = bloque.SelectSingleNode(".//span[contains(@class,'result-date')]")?.InnerText.Trim() ?? "";
                    var hora = bloque.SelectSingleNode(".//span[contains(@class,'lottery-closing-time')]")?.InnerText.Trim() ?? "";

                    // Normalizar fecha directamente aquí
                    string fechaNormalizada;
                    if (DateTime.TryParse(fecha, out var fechaSorteo))
                        fechaNormalizada = fechaSorteo.ToString("yyyy-MM-dd");
                    else
                        fechaNormalizada = FechaHelper.GetFechaLocal();

                    var numeros = bloque.SelectNodes(".//div[contains(@class,'result-number')]");
                    if (numeros != null)
                    {
                        foreach (var num in numeros)
                        {
                            resultados.Add((nombre, fechaNormalizada, hora, num.InnerText.Trim()));
                        }
                    }
                }
            }

            return resultados;
        }

        public async Task GuardarResultadosEnFirebase(List<(string Loteria, string Fecha, string Hora, string Numero)> resultados)
        {
            foreach (var grupo in resultados.GroupBy(r => r.Loteria))
            {
                var loteriaNombre = grupo.Key;
                var fechaNormalizada = grupo.First().Fecha; // ✅ usar fecha del scraping
                var hora = grupo.First().Hora;

                var nombreNormalizado = NormalizarNombre(loteriaNombre, hora);
                if (string.IsNullOrWhiteSpace(nombreNormalizado)) continue;

                if (!LoteriaClaves.TryGetValue(nombreNormalizado, out var loteriaClave)) continue;

                var numeros = grupo.Select(r => r.Numero).Take(3).ToList();
                var primerPremio = numeros.ElementAtOrDefault(0) ?? "";
                var segundoPremio = numeros.ElementAtOrDefault(1) ?? "";
                var tercerPremio = numeros.ElementAtOrDefault(2) ?? "";

                await _firebaseClient
                    .Child("Resultados")
                    .Child(loteriaClave)
                    .Child(fechaNormalizada)
                    .PutAsync(new
                    {
                        FechaSorteo = fechaNormalizada,
                        LoteriaClave = loteriaClave,
                        LoteriaNombre = nombreNormalizado,
                        PrimerPremio = primerPremio,
                        SegundoPremio = segundoPremio,
                        TercerPremio = tercerPremio
                    });

                _logger.LogInformation($"✅ Guardado {loteriaNombre} ({hora}) en Firebase con fecha {fechaNormalizada}");
            }
        }

        private string? NormalizarNombre(string nombre, string hora)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return null;

            string horaNormalizada;
            if (DateTime.TryParse(hora, out var dt))
                horaNormalizada = dt.ToString("h:mm tt", CultureInfo.InvariantCulture);
            else
                horaNormalizada = hora.Trim();

            // … aquí mantienes tu lógica de normalización (Anguilla, Primera, Suerte, etc.)
            // asegurándote de devolver exactamente las claves que definiste arriba en LoteriaClaves
            // Ejemplo:
            if (nombre.StartsWith("New York"))
                return horaNormalizada.Contains("2:30 PM") ? "NY.Tarde 3:30 PM" : "NY.Noche 11:30 PM";

            // resto de tu lógica…
            _logger.LogWarning($"⚠️ Nombre de lotería no reconocido: {nombre} ({horaNormalizada}), se omite.");
            return null;
        }
    }
}
