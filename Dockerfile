# Stage 1: Build aplication
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# NuGet paketlerini optimize etmek için önce proje dosyalarını kopyala
COPY *.sln .
COPY src/Core/VidSync.API/*.csproj ./src/Core/VidSync.API/
COPY src/Core/VidSync.Signaling/*.csproj ./src/Core/VidSync.Signaling/
COPY src/Domain/VidSync.Domain/*.csproj ./src/Domain/VidSync.Domain/
COPY src/Infrastructure/VidSync.Persistence/*.csproj ./src/Infrastructure/VidSync.Persistence/

RUN dotnet restore

COPY . .

WORKDIR /app/src/Core/VidSync.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5123
EXPOSE 7123

ENTRYPOINT ["dotnet", "VidSync.API.dll"]