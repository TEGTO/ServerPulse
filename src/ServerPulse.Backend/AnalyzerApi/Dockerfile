#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["AnalyzerApi/AnalyzerApi.csproj", "AnalyzerApi/"]
COPY ["AnalyzerApi.Infrastructure/AnalyzerApi.Infrastructure.csproj", "AnalyzerApi.Infrastructure/"]
COPY ["Caching/Caching.csproj", "Caching/"]
COPY ["MessageBus/MessageBus.csproj", "MessageBus/"]
COPY ["Resilience/Resilience.csproj", "Resilience/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["ExceptionHandling/ExceptionHandling.csproj", "ExceptionHandling/"]
COPY ["Helper/Helper.csproj", "Helper/"]
COPY ["Logging/Logging.csproj", "Logging/"]
RUN dotnet restore "./AnalyzerApi/AnalyzerApi.csproj"
COPY . .
WORKDIR "/src/AnalyzerApi"
RUN dotnet build "./AnalyzerApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AnalyzerApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AnalyzerApi.dll"]