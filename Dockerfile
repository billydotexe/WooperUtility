ARG DOTNET_VERSION=8.0
ARG ALPINE_VERSION=3.20

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS restore
WORKDIR /src
COPY ["WooperUtility.csproj", "./"]
COPY ["Directory.Build.props", "./"]
RUN dotnet restore

FROM restore AS build
RUN mkdir -p /app
COPY Directory.Build.props /app
COPY . /app/src
RUN dotnet publish "/app/src/WooperUtility.csproj" --configuration Release --output /dist

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine${ALPINE_VERSION}
COPY --from=build /dist /app
WORKDIR /app
ENTRYPOINT ["sh", "-c", "dotnet WooperUtility.dll database update && dotnet WooperUtility.dll"]
