using LoteriaWebScraper; // ✅ importa tu namespace para ScraperService

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ registra tu ScraperService con inyección de dependencias
builder.Services.AddSingleton<ScraperService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// 🔹 Habilitar Swagger siempre (no solo en Development)
app.UseSwagger();
app.UseSwaggerUI();

// ❌ Quitar redirección HTTPS porque Render ya maneja HTTPS externamente
// app.UseHttpsRedirection();

app.UseAuthorization();

// ✅ mapea tus controladores (ScraperController)
app.MapControllers();

// 🔹 Endpoint raíz opcional para evitar 404 en "/"
app.MapGet("/", () => "API Lotería WebScraper funcionando 🚀");

app.Run();
