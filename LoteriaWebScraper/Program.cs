using LoteriaWebScraper; // ✅ importa tu namespace para ScraperService

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger/OpenAPI (habilitado siempre)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ registra tu ScraperService con inyección de dependencias
builder.Services.AddSingleton<ScraperService>();

var app = builder.Build();

// ✅ Swagger siempre activo (no solo en Development)
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

app.UseAuthorization();

// ✅ mapea tus controladores (ScraperController)
app.MapControllers();

// 🔹 Endpoint de bienvenida en la raíz
app.MapGet("/", () => "LoteriaWebScraper API está en línea ✅ Usa /api/scraper/run para ejecutar el scraper.");

// 🔹 Ajuste para Render: escuchar en el puerto asignado
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");

// 🔹 Verificación de FIREBASE_SECRET
var firebaseSecret = Environment.GetEnvironmentVariable("FIREBASE_SECRET");
if (string.IsNullOrWhiteSpace(firebaseSecret))
{
    Console.WriteLine("⚠️ Advertencia: FIREBASE_SECRET no está configurado. Firebase no podrá actualizar resultados.");
}

// ✅ arranca la aplicación
app.Run();
