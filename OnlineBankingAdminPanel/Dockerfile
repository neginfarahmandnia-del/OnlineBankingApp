# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "./OnlineBankingAdminPanel/OnlineBankingAdminPanel.csproj"
RUN dotnet publish "./OnlineBankingAdminPanel/OnlineBankingAdminPanel.csproj" -c Release -o /app/publish

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://0.0.0.0:
ENV DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080
ENTRYPOINT ["dotnet", "OnlineBankingAdminPanel.dll"]