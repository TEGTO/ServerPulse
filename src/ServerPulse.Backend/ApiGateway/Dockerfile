#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["ApiGateway/ApiGateway.csproj", "ApiGateway/"]
COPY ["Authentication/Authentication.csproj", "Authentication/"]
COPY ["Helper/Helper.csproj", "Helper/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["ExceptionHandling/ExceptionHandling.csproj", "ExceptionHandling/"]
COPY ["Logging/Logging.csproj", "Logging/"]
COPY ["Resilience/Resilience.csproj", "Resilience/"]
RUN dotnet restore "./ApiGateway/ApiGateway.csproj"
COPY . .
WORKDIR "/src/ApiGateway"
RUN dotnet build "./ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]