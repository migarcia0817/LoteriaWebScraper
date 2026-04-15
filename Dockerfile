FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY LoteriaWebScraper/LoteriaWebScraper.csproj LoteriaWebScraper/
RUN dotnet restore LoteriaWebScraper/LoteriaWebScraper.csproj

COPY . .
WORKDIR /src/LoteriaWebScraper
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LoteriaWebScraper.dll"]
