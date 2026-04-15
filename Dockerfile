# Usa imagen oficial de .NET 8 SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia la solución y el proyecto
COPY LoteriaWebScraper.sln ./
COPY LoteriaWebScraper/*.csproj ./LoteriaWebScraper/

# Restaura dependencias
RUN dotnet restore LoteriaWebScraper.sln

# Copia todo el código
COPY . .

# Compila y publica en modo Release
WORKDIR /src/LoteriaWebScraper
RUN dotnet publish -c Release -o /app/publish

# Usa imagen runtime para ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Arranca el servicio
ENTRYPOINT ["dotnet", "LoteriaWebScraper.dll"]
