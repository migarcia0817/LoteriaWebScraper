using LoteriaWebScraper; // ✅ importa tu namespace para ScraperService

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger/OpenAPI (ya viene por defecto)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ registra tu ScraperService con inyección de dependencias
builder.Services.AddSingleton<ScraperService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ❌ Quitar redirección HTTPS porque Render ya maneja HTTPS externamente
// app.UseHttpsRedirection();

app.UseAuthorization();

// ✅ mapea tus controladores (ScraperController)
app.MapControllers();

app.Run();
