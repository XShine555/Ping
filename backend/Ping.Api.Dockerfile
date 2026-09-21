# Build context: backend/  (docker compose -f deploy/compose.yml --profile apps build)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore: copy central config + csproj files first to cache the layer.
COPY Directory.Build.props Directory.Packages.props nuget.config ./
COPY Core/Ping.Domain/Ping.Domain.csproj                 Core/Ping.Domain/
COPY Core/Ping.Application/Ping.Application.csproj       Core/Ping.Application/
COPY Core/Ping.Infrastructure/Ping.Infrastructure.csproj Core/Ping.Infrastructure/
COPY Hosts/Ping.Api/Ping.Api.csproj                      Hosts/Ping.Api/
RUN dotnet restore Hosts/Ping.Api/Ping.Api.csproj

COPY . .
RUN dotnet publish Hosts/Ping.Api/Ping.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:5211
EXPOSE 5211
ENTRYPOINT ["dotnet", "Ping.Api.dll"]
