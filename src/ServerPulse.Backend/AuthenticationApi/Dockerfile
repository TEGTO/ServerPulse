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
COPY ["Directory.Build.props", "."]
COPY ["AuthenticationApi/AuthenticationApi.csproj", "AuthenticationApi/"]
COPY ["AuthenticationApi.Application/AuthenticationApi.Application.csproj", "AuthenticationApi.Application/"]
COPY ["AuthenticationApi.Core/AuthenticationApi.Core.csproj", "AuthenticationApi.Core/"]
COPY ["Authentication/Authentication.csproj", "Authentication/"]
COPY ["Helper/Helper.csproj", "Helper/"]
COPY ["HttpCommunication/HttpCommunication.csproj", "HttpCommunication/"]
COPY ["BackgroundTask/BackgroundTask.csproj", "BackgroundTask/"]
COPY ["EmailControl/EmailControl.csproj", "EmailControl/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["ExceptionHandling/ExceptionHandling.csproj", "ExceptionHandling/"]
COPY ["Logging/Logging.csproj", "Logging/"]
COPY ["Resilience/Resilience.csproj", "Resilience/"]
COPY ["AuthenticationApi.Infrastructure/AuthenticationApi.Infrastructure.csproj", "AuthenticationApi.Infrastructure/"]
COPY ["Caching/Caching.csproj", "Caching/"]
COPY ["DatabaseControl/DatabaseControl.csproj", "DatabaseControl/"]
COPY ["Proxies/Proxies.csproj", "Proxies/"]
COPY ["Documentation/Documentation.csproj", "Documentation/"]
RUN dotnet restore "./AuthenticationApi/AuthenticationApi.csproj"
COPY . .
WORKDIR "/src/AuthenticationApi"
RUN dotnet build "./AuthenticationApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AuthenticationApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthenticationApi.dll"]