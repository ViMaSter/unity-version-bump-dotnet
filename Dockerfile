FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env

WORKDIR /app
COPY . ./
RUN dotnet publish ./UnityVersionBump.Action/UnityVersionBump.Action.csproj -c Release -o out --no-self-contained

LABEL maintainer="Vincent Mahnke <vincent@mahn.ke>"
LABEL repository="https://github.com/ViMaSter/unity-version-bump"
LABEL homepage="https://github.com/ViMaSter/unity-version-bump"

LABEL com.github.actions.name="Unity Version Bump"
LABEL com.github.actions.description="Creates pull requests for outdated Unity Editor and UPM package versions."
LABEL com.github.actions.icon="download-cloud"
LABEL com.github.actions.color="gray-dark"

FROM mcr.microsoft.com/dotnet/sdk:7.0
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "/UnityVersionBump.Action.dll" ]