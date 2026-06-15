# Compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solución y proyecto
COPY LoteriaWebScraper.sln ./
COPY LoteriaWebScraper/*.csproj ./LoteriaWebScraper/

# Restaurar dependencias
RUN dotnet restore LoteriaWebScraper.sln

# Copiar código fuente
COPY . .

# Publicar
WORKDIR /src/LoteriaWebScraper
RUN dotnet publish -c Release -o /app/publish

# Runtime (NO ASP.NET)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "LoteriaWebScraper.dll"]