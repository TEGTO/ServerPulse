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
COPY ["ServerMonitorApi/ServerMonitorApi.csproj", "ServerMonitorApi/"]
COPY ["Authentication/Authentication.csproj", "Authentication/"]
COPY ["Helper/Helper.csproj", "Helper/"]
COPY ["MessageBus/MessageBus.csproj", "MessageBus/"]
COPY ["Resilience/Resilience.csproj", "Resilience/"]
COPY ["ServerSlotApi.Dtos/ServerSlotApi.Dtos.csproj", "ServerSlotApi.Dtos/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["ExceptionHandling/ExceptionHandling.csproj", "ExceptionHandling/"]
COPY ["Logging/Logging.csproj", "Logging/"]
RUN dotnet restore "./ServerMonitorApi/ServerMonitorApi.csproj"
COPY . .
WORKDIR "/src/ServerMonitorApi"
RUN dotnet build "./ServerMonitorApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ServerMonitorApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerMonitorApi.dll"]