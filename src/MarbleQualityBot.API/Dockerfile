FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MarbleQualityBot.API/MarbleQualityBot.API.csproj", "MarbleQualityBot.API/"]
RUN dotnet restore "./MarbleQualityBot.API/MarbleQualityBot.API.csproj"
COPY . .
WORKDIR "/src/MarbleQualityBot.API"
RUN dotnet build "./MarbleQualityBot.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MarbleQualityBot.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MarbleQualityBot.API.dll"]