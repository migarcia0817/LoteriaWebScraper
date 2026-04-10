# Imagen base oficial de .NET 8 para construir
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj y restaurar dependencias
COPY LoteriaWebScraper.csproj .
RUN dotnet restore LoteriaWebScraper.csproj

# Copiar todo y publicar en carpeta /app
COPY . .
RUN dotnet publish LoteriaWebScraper.csproj -c Release -o /app

# Imagen runtime más ligera
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "LoteriaWebScraper.dll"]


