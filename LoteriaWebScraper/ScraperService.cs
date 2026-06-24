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

        // Diccionario de mapeo entre nombre y clave
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
            { "NY.Tarde 3:30 PM", "NYTarde_330_PM"},
            { "NY.Noche 11:30 PM", "NYNoche_1130_PM" },
            { "King Lotery 12:30 PM", "KingLot_1230_PM"},
            { "King Lotery 7:30 PM", "KingLot_730_PM"},
            { "Suerte 12:30 PM", "Suerte_1230_PM"},
            { "Suerte 6:00 PM", "Suerte_600_PM"},
            { "Primera 12:00 PM", "Primera_1200_PM"},
            { "Primera 7:00 PM", "Primera_700_PM"},
            { "Q.Real Tarde 12:55 PM", "QRealTarde_100_PM" },
            { "FL.Tarde 1:30 PM", "FLTarde_130_PM"},
            { "FL.Noche 9:45 PM", "FLNoche_1025_PM" },
            { "Loteka 7:55 PM", "Loteka_755_PM"},
            { "Gana Mas 2:30 PM", "Gana_Mas_230_PM"},
            { "Nacional", "NacNoche_900_PM"},
            { "Leidsa", "Leidsa_850_PM"},
            { "Lotedom 12:00 PM", "Lotedom_1200_PM" },
            { "Haiti Bolet 9:30 AM", "Haiti_Bolet_930_AM" },
            { "Haiti Bolet 10:30 AM", "Haiti_Bolet_1030_AM" },
            { "Haiti Bolet 11:30 AM", "Haiti_Bolet_1130_AM" },
            { "Haiti Bolet 5:30 PM", "Haiti_Bolet_530_PM" },
            { "Haiti Bolet 6:30 PM", "Haiti_Bolet_630_PM" },
            { "Haiti Bolet 7:30 PM", "Haiti_Bolet_730_PM" }
        };

        // ✅ Constructor correcto con inyección de logger
        public ScraperService(ILogger<ScraperService> logger)
        {
            _logger = logger;

            _firebaseClient = new FirebaseClient(
                "https://bancachupon-default-rtdb.firebaseio.com/",
                new FirebaseOptions
                {
                      AuthTokenAsyncFactory = () => Task.FromResult(Environment.GetEnvironmentVariable("FIREBASE_SECRET"))
                });

            //mun mensahito aquii
        }

        // Aquí van tus métodos ObtenerNumerosGanadoresAsync, GuardarResultadosEnFirebase y NormalizarNombre


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

                    var numeros = bloque.SelectNodes(".//div[contains(@class,'result-number')]");
                    if (numeros != null)
                    {
                        foreach (var num in numeros)
                        {
                            resultados.Add((nombre, fecha, hora, num.InnerText.Trim()));
                        }
                    }
                }
            }

            return resultados;
        }
        private string ParseFechaWeb(string fechaWeb)
        {
            // Ejemplo: "Mar 23 de junio, 2026"
            var cultura = new CultureInfo("es-ES");
            if (DateTime.TryParse(fechaWeb, cultura, DateTimeStyles.None, out var fecha))
            {
                return fecha.ToString("yyyy-MM-dd");
            }

            // fallback: fecha local si no se pudo parsear
            var zonaRD = TimeZoneInfo.FindSystemTimeZoneById("America/Santo_Domingo");
            var horaRD = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaRD);
            return horaRD.ToString("yyyy-MM-dd");
        }

        public async Task GuardarResultadosEnFirebase(List<(string Loteria, string Fecha, string Hora, string Numero)> resultados)
        {
            int guardados = 0, omitidos = 0;

            // 🔹 Fecha local en RD
            var zonaRD = TimeZoneInfo.FindSystemTimeZoneById("America/Santo_Domingo");
            var horaRD = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaRD);
            var fechaHoyRD = horaRD.ToString("yyyy-MM-dd");

            foreach (var grupo in resultados.GroupBy(r => r.Loteria))
            {
                _logger.LogInformation($"UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                _logger.LogInformation($"RD : {horaRD:yyyy-MM-dd HH:mm:ss}");
                _logger.LogInformation($"FechaHelper: {fechaHoyRD}");

                var loteriaNombre = grupo.Key;
                var hora = grupo.First().Hora;
                var fechaWeb = grupo.First().Fecha;
                var fechaPublicacion = ParseFechaWeb(fechaWeb);

                // 🔹 No publicar si la fecha de la página no coincide con la fecha local RD
                if (fechaPublicacion != fechaHoyRD)
                {
                    _logger.LogWarning($"⏩ {loteriaNombre} ({hora}) tiene fecha {fechaPublicacion}, se omite porque no es hoy ({fechaHoyRD}).");
                    omitidos++;
                    continue;
                }

                var nombreNormalizado = NormalizarNombre(loteriaNombre, hora);

                if (string.IsNullOrWhiteSpace(nombreNormalizado) || !LoteriaClaves.TryGetValue(nombreNormalizado, out var loteriaClave))
                {
                    omitidos++;
                    continue;
                }

                var numeros = grupo.Select(r => r.Numero).Take(3).ToList();
                var primerPremio = numeros.ElementAtOrDefault(0) ?? "";
                var segundoPremio = numeros.ElementAtOrDefault(1) ?? "";
                var tercerPremio = numeros.ElementAtOrDefault(2) ?? "";

                // 🔹 Validar premios incompletos
                if (string.IsNullOrEmpty(primerPremio) || string.IsNullOrEmpty(segundoPremio))
                {
                    _logger.LogWarning($"⏩ {nombreNormalizado} detectado pero sin premios completos, se omite publicación.");
                    omitidos++;
                    continue;
                }

                _logger.LogInformation(
                    $"LOTERIA={loteriaNombre} | HORA={hora} | FECHA_WEB={fechaWeb} | FECHA_GUARDADA={fechaPublicacion}"
                );

                var resultado = new
                {
                    FechaSorteo = fechaPublicacion,
                    LoteriaClave = loteriaClave,
                    LoteriaNombre = nombreNormalizado,
                    PrimerPremio = primerPremio,
                    SegundoPremio = segundoPremio,
                    TercerPremio = tercerPremio
                };

                var existente = await _firebaseClient
                    .Child("Resultados")
                    .Child(loteriaClave)
                    .Child(fechaPublicacion)
                    .OnceSingleAsync<object>();

                if (existente != null)
                {
                    _logger.LogInformation($"⏩ Ya existe resultado en {loteriaClave} ({fechaPublicacion}), se omite.");
                    continue;
                }

                await _firebaseClient
                    .Child("Resultados")
                    .Child(loteriaClave)
                    .Child(fechaPublicacion)
                    .PutAsync(resultado);

                _logger.LogInformation($"✅ Guardado {nombreNormalizado} ({fechaPublicacion}) - {primerPremio}, {segundoPremio}, {tercerPremio}");
                guardados++;
            }

            _logger.LogInformation($"📊 Resumen ciclo: Guardados={guardados}, Omitidos={omitidos}");
        }





        // 🔹 Limpiar fechas futuras pero conservar hoy y ayer
        public async Task LimpiarFechasFuturas()
        {
            var fechaHoy = FechaHelper.GetFechaLocal();
            var fechaAyer = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");

            foreach (var kvp in LoteriaClaves)
            {
                var loteriaClave = kvp.Value;
                var fechasGuardadas = await _firebaseClient
                    .Child("Resultados")
                    .Child(loteriaClave)
                    .OnceAsync<object>();

                foreach (var registro in fechasGuardadas)
                {
                    var fecha = registro.Key;

                    // 🔹 Eliminar solo si es mayor a hoy
                    if (string.Compare(fecha, fechaHoy) > 0)
                    {
                        await _firebaseClient
                            .Child("Resultados")
                            .Child(loteriaClave)
                            .Child(fecha)
                            .DeleteAsync();

                        _logger.LogInformation($"🗑️ Eliminada {loteriaClave} en fecha {fecha}");
                    }
                }
            }

            _logger.LogInformation($"✅ Limpieza completada. Se conservaron resultados de {fechaHoy} y {fechaAyer}");
        }




        private string? NormalizarNombre(string nombre, string hora)
        {
            // tu lógica de normalización aquí (copiada del Worker)


            if (string.IsNullOrWhiteSpace(nombre))
                return null; // ⚠️ devolver null explícito si no hay nombre

            // Centralizar la normalización en HoraHelper
            //   string horaNormalizada = HoraHelper.Normalizar(hora);

            string horaNormalizada;
            if (DateTime.TryParse(hora, out var dt))
                horaNormalizada = dt.ToString("h:mm tt"); // ejemplo: "8:00 AM"
            else horaNormalizada = hora.Trim();


            
            if (nombre.StartsWith("Anguilla"))
            {
                if (horaNormalizada == "8:00 AM")
                    return "Anguilla 08:00 AM";
                if (horaNormalizada == "9:00 AM")
                    return "Anguilla 09:00 AM";
                if (horaNormalizada == "10:00 AM")
                    return "Anguilla 10:00 AM";
                if (horaNormalizada == "11:00 AM")
                    return "Anguilla 11:00 AM";
                if (horaNormalizada == "12:00 PM")
                    return "Anguilla 12:00 PM";
                if (horaNormalizada == "1:00 PM")
                    return "Anguilla 1:00 PM";
                if (horaNormalizada == "2:00 PM")
                    return "Anguilla 2:00 PM";
                if (horaNormalizada == "3:00 PM")
                    return "Anguilla 3:00 PM";
                if (horaNormalizada == "4:00 PM")
                    return "Anguilla 4:00 PM";
                if (horaNormalizada == "5:00 PM")
                    return "Anguilla 5:00 PM";
                if (horaNormalizada == "6:00 PM")
                    return "Anguilla 6:00 PM";
                if (horaNormalizada == "7:00 PM")
                    return "Anguilla 7:00 PM";
                if (horaNormalizada == "8:00 PM")
                    return "Anguilla 8:00 PM";
                if (horaNormalizada == "9:00 PM")
                    return "Anguilla 9:00 PM";
                if (horaNormalizada == "10:00 PM")
                    return "Anguilla 10:00 PM";


            }


            // La Primera
            if (nombre.StartsWith("La Primera"))
            {
                if (horaNormalizada == "12:00 PM")
                    return "Primera 12:00 PM";
                else if (horaNormalizada == "7:00 PM")
                    return "Primera 7:00 PM";
                else if (horaNormalizada == "8:00 PM")
                    return "Primera 8 PM";
            }

            // La Suerte
            if (nombre.StartsWith("La Suerte"))
            {
                if (horaNormalizada.Contains("12:30 PM") || horaNormalizada.Contains("12 PM"))
                    return "Suerte 12:30 PM";

                if (horaNormalizada.Contains("6:00 PM") || horaNormalizada.Contains("6PM"))
                    return "Suerte 6:00 PM";

                _logger.LogWarning($"⚠️ No se encontró clave para La Suerte ({horaNormalizada}), se omite.");
                return null;
            }

            // King Lottery Día/Noche
            if (nombre.StartsWith("King Lottery"))
            {
                if (nombre.Contains("Día") || horaNormalizada.Contains("12:30 PM"))
                    return "King Lotery 12:30 PM";
                if (nombre.Contains("Noche") || horaNormalizada.Contains("7:30 PM"))
                    return "King Lotery 7:30 PM";
            }

            // Real → Q.Real
            if (nombre.StartsWith("Real"))
            {
                if (horaNormalizada.Contains("1:00 PM")) return "Q.Real Tarde 1:00 PM";
                if (horaNormalizada.Contains("12:55 PM")) return "Q.Real Tarde 12:55 PM";

                _logger.LogWarning($"⚠️ No se encontró clave para Real ({horaNormalizada}), se omite.");
                return null;
            }

            // Florida
            if (nombre.StartsWith("Florida"))
            {
                if (horaNormalizada.Contains("2:30 PM"))
                    return "FL.Tarde 2:30 PM";
                if (horaNormalizada.Contains("1:30 PM"))
                    return "FL.Tarde 1:30 PM";
                else if (horaNormalizada.Contains("10:25 PM") || horaNormalizada.Contains("9:45 PM"))
                    return "FL.Noche 9:45 PM";  // 👈 siempre normaliza a 10:25 PM
            }



            //  New York
            if (nombre.ToUpperInvariant().StartsWith("NEW YORK"))
            {
                var hora1 = horaNormalizada.Replace(" ", "").ToUpperInvariant();

                if (hora1.Contains("2:30PM"))
                    return "NY.Tarde 3:30 PM";

                if (hora1.Contains("10:30PM") || hora1.Contains("11:30PM"))
                    return "NY.Noche 11:30 PM";
            }

            




            // Loteka
            if (nombre.StartsWith("Loteka"))
                return "Loteka 7:55 PM";

            // Leidsa / Leisa
            //  if (nombre.StartsWith("Leidsa") || nombre.StartsWith("Leisa"))
            //    return "Leisa 8:55 PM";
            // Leidsa / Leisa
            if (nombre.StartsWith("Leidsa") || nombre.StartsWith("Leisa"))
            {
                var hoy = DateTime.Today.DayOfWeek;

                if (hoy == DayOfWeek.Sunday)
                {
                    // ✅ Los domingos se devuelve LeisaDomingo
                    return "Leidsa";
                }
                else
                {
                    // ✅ De lunes a sábado se mantiene la lógica normal
                    return "Leidsa";
                    //  return horaNormalizada.Contains("8:50 PM")
                    // "Leisa";
                }
            }

            if (nombre.StartsWith("LoteDom"))
            {
                if (horaNormalizada.Contains("12:00 PM") || horaNormalizada.Contains("12 PM"))
                    return "Lotedom 12:00 PM";
            }

            // Nacional
            // if (nombre.StartsWith("Nacional"))
            //   return horaNormalizada.Contains("2:55 PM") ? "Nac.Tarde 2:55 PM" : "Nac.Noche 9:00 PM";
            // Nacional
            if (nombre.StartsWith("Nacional"))
            {
                var hoy = DateTime.Today.DayOfWeek;

                if (hoy == DayOfWeek.Sunday)
                {
                    // ✅ Los domingos se devuelve NacDomingo
                    return "Nacional";

                }
                else
                {
                    // ✅ De lunes a sábado se mantiene la lógica normal
                    return horaNormalizada.Contains("2:30 PM")
                        ? "Gana Mas 2:30 PM"
                        : "Nacional";
                }
            }
            if (nombre.StartsWith("Haiti Bolet"))
            {
                if (horaNormalizada.Contains("9:30"))
                    return "Haiti Bolet 9:30 AM";
                if (horaNormalizada.Contains("10:30"))
                    return "Haiti Bolet 10:30 AM";
                if (horaNormalizada.Contains("11:30"))
                    return "Haiti Bolet 11:30 AM";
                if (horaNormalizada.Contains("5:30"))
                    return "Haiti Bolet 5:30 PM";
                if (horaNormalizada.Contains("6:30"))
                    return "Haiti Bolet 6:30 PM";
                if (horaNormalizada.Contains("7:30"))
                    return "Haiti Bolet 7:30 PM";

                _logger.LogWarning($"⚠️ No se encontró clave para Haiti Bolet ({horaNormalizada}), se omite.");
                return null;
            }

           

            // Gana Más → Nac.Tarde
            if (nombre.StartsWith("Gana Más"))
                return "Gana Mas 2:30 PM";

            // 🔹 Si no se reconoce, loguear y devolver null
            _logger.LogWarning($"⚠️ Nombre de lotería no reconocido: {nombre} ({horaNormalizada}), se omite.");
          //  return null;
            return nombre; // simplificado



        }
    }
}
