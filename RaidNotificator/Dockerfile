FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build
WORKDIR /src

COPY ["RaidNotificator/RaidNotificator.csproj", "RaidNotificator/"]
RUN dotnet restore "RaidNotificator/RaidNotificator.csproj"

COPY . .
WORKDIR "/src/RaidNotificator"
RUN dotnet publish "./RaidNotificator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:9.0-bookworm-slim AS final
WORKDIR /app

COPY --from=build /app/publish .
ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "RaidNotificator.dll"]
