# Build-Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build
WORKDIR /src

COPY ["RaidNotificator/RaidNotificator.csproj", "RaidNotificator/"]
RUN dotnet restore "RaidNotificator/RaidNotificator.csproj"

COPY . .
WORKDIR "/src/RaidNotificator"
RUN dotnet build "./RaidNotificator.csproj" -c Release -o /app/build

# Publish-Stage
FROM build AS publish
RUN dotnet publish "./RaidNotificator.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime-Stage (ARM32v7 für Raspberry Pi 32-bit, oder arm64v8 für 64-bit)
FROM mcr.microsoft.com/dotnet/runtime:9.0-bookworm-slim-arm32v7 AS final
WORKDIR /app

RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

COPY --from=publish /app/publish .

ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "RaidNotificator.dll"]