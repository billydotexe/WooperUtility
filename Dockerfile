FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /app

COPY WooperUtility.csproj .
RUN dotnet restore WooperUtility.csproj

COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
WORKDIR /app
COPY --from=build-env /app/out ./

ENTRYPOINT ["dotnet", "WooperUtility.dll"]