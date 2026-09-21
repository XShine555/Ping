# Build context: backend/  (docker compose -f deploy/compose.yml up migrate)
#
# Applies EF Core migrations, then exits. Ping.Infrastructure is both the migrations and the
# design-time startup project: it owns the IDesignTimeDbContextFactory and the EFCore.Design
# package. The connection string comes from the Database__ConnectionString env var (compose
# sets it), read via DatabaseDesignTimeFactory's AddEnvironmentVariables().
FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props nuget.config ./
COPY Core/Ping.Domain/Ping.Domain.csproj                 Core/Ping.Domain/
COPY Core/Ping.Application/Ping.Application.csproj       Core/Ping.Application/
COPY Core/Ping.Infrastructure/Ping.Infrastructure.csproj Core/Ping.Infrastructure/
RUN dotnet restore Core/Ping.Infrastructure/Ping.Infrastructure.csproj

COPY Core/Ping.Domain         Core/Ping.Domain/
COPY Core/Ping.Application    Core/Ping.Application/
COPY Core/Ping.Infrastructure Core/Ping.Infrastructure/

RUN dotnet tool install --global dotnet-ef --version 10.0.11
ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR /src/Core/Ping.Infrastructure
ENTRYPOINT ["dotnet", "ef", "database", "update", "--context", "Database"]
