# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# kompletter Repo-Inhalt in das Build-Image
COPY . .
# WICHTIG: Pfad zur API-csproj MIT Ordner
RUN dotnet restore "./OnlineBankingApp.API/OnlineBankingApp.API.csproj"
RUN dotnet publish "./OnlineBankingApp.API/OnlineBankingApp.API.csproj" -c Release -o /app/publish

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Ports/Konfiguration kommen über Render-Env-Vars
ENTRYPOINT ["dotnet","OnlineBankingApp.API.dll"]
