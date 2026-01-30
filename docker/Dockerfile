# =============================================================================
# Volue Forecast Service - Multi-stage Dockerfile
# =============================================================================
# Stage 1: Build (SDK)
# Stage 2: Runtime (ASP.NET)
# =============================================================================

# -----------------------------------------------------------------------------
# Stage 1: Build
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy solution and project files (for layer caching)
COPY *.sln ./
COPY Directory.Build.props ./
COPY src/Volue.ForecastService.Api/*.csproj ./src/Volue.ForecastService.Api/
COPY src/Volue.ForecastService.Services/*.csproj ./src/Volue.ForecastService.Services/
COPY src/Volue.ForecastService.Repositories/*.csproj ./src/Volue.ForecastService.Repositories/
COPY src/Volue.ForecastService.Contracts/*.csproj ./src/Volue.ForecastService.Contracts/
COPY tests/Volue.ForecastService.UnitTests/*.csproj ./tests/Volue.ForecastService.UnitTests/
COPY tests/Volue.ForecastService.IntegrationTests/*.csproj ./tests/Volue.ForecastService.IntegrationTests/

# Restore dependencies (includes test projects for dependency resolution)
RUN dotnet restore src/Volue.ForecastService.Api/Volue.ForecastService.Api.csproj

# Copy all source code
COPY src/ ./src/

# Build and publish
WORKDIR /src/src/Volue.ForecastService.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# -----------------------------------------------------------------------------
# Stage 2: Runtime
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Install curl for health checks
RUN apk add --no-cache curl

# Security: Create non-root user
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

# Copy published application
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8080/health/live || exit 1

# Environment
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Entry point
ENTRYPOINT ["dotnet", "Volue.ForecastService.Api.dll"]
