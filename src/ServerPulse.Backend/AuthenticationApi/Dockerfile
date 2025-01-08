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
COPY ["AuthenticationApi/AuthenticationApi.csproj", "AuthenticationApi/"]
COPY ["AuthenticationApi.Infrastructure/AuthenticationApi.Infrastructure.csproj", "AuthenticationApi.Infrastructure/"]
COPY ["Authentication/Authentication.csproj", "Authentication/"]
COPY ["Helper/Helper.csproj", "Helper/"]
COPY ["BackgroundTask/BackgroundTask.csproj", "BackgroundTask/"]
COPY ["DatabaseControl/DatabaseControl.csproj", "DatabaseControl/"]
COPY ["Resilience/Resilience.csproj", "Resilience/"]
COPY ["EmailControl/EmailControl.csproj", "EmailControl/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["ExceptionHandling/ExceptionHandling.csproj", "ExceptionHandling/"]
COPY ["Logging/Logging.csproj", "Logging/"]
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