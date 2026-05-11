# syntax=docker/dockerfile:1

ARG DOTNET_VERSION=10.0
ARG BUILD_CONFIGURATION=Release

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG BUILD_CONFIGURATION
WORKDIR /src
ENV NUGET_PACKAGES=/root/.nuget/packages

# Copy project files first for better Docker layer caching.
# All .csproj files in the transitive closure of Web.csproj must be present
# before `dotnet restore` so that no project is skipped and every
# project.assets.json is generated (required by --no-restore in publish).
COPY Directory.Build.props Directory.Packages.props global.json nuget.config ./
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Infrastructure.Persistence/Infrastructure.Persistence.csproj src/Infrastructure.Persistence/
COPY src/Infrastructure.Rest.Client/Infrastructure.Rest.Client.csproj src/Infrastructure.Rest.Client/
COPY src/Web/Web.csproj src/Web/

RUN --mount=type=cache,id=nuget-packages,target=/root/.nuget/packages,sharing=locked \
    --mount=type=cache,id=nuget-v3-cache,target=/root/.local/share/NuGet/v3-cache,sharing=locked \
    dotnet restore src/Web/Web.csproj

# Copy full source and publish with selected configuration.
COPY . .
RUN --mount=type=cache,id=nuget-packages,target=/root/.nuget/packages,sharing=locked \
    --mount=type=cache,id=nuget-v3-cache,target=/root/.local/share/NuGet/v3-cache,sharing=locked \
    dotnet publish src/Web/Web.csproj -c ${BUILD_CONFIGURATION} -o /app/publish --no-restore /p:SkipNSwag=True

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app

# Run the app as a non-root user.
ARG APP_UID=10001
ARG APP_GID=10001
RUN set -eux; \
    chown -R "${APP_UID}:${APP_GID}" /app

# Runtime URLs can be overridden in deployment environment if needed.
ENV ASPNETCORE_URLS=http://+:8080
ENV HOME=/app
# Ensure the AWS SDK reads ~/.aws/config in addition to credentials.
ENV AWS_SDK_LOAD_CONFIG=1
EXPOSE 8080

COPY --from=build --chown=${APP_UID}:${APP_GID} /app/publish ./

USER ${APP_UID}:${APP_GID}
ENTRYPOINT ["dotnet", "VibraHeka.Web.dll"]
