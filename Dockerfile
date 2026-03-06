# syntax=docker/dockerfile:1

ARG DOTNET_VERSION=10.0
ARG BUILD_CONFIGURATION=Release

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG BUILD_CONFIGURATION
WORKDIR /src

# Copy project files first for better Docker layer caching.
COPY Directory.Build.props Directory.Packages.props global.json nuget.config ./
COPY src/Application/Application.csproj src/Application/
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Web/Web.csproj src/Web/

RUN dotnet restore src/Web/Web.csproj

# Copy full source and publish with selected configuration.
COPY . .
RUN dotnet publish src/Web/Web.csproj -c ${BUILD_CONFIGURATION} -o /app/publish --no-restore /p:SkipNSwag=True

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app

# Runtime URLs can be overridden in deployment environment if needed.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "VibraHeka.Web.dll"]
