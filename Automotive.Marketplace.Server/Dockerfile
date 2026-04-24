# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Automotive.Marketplace.Server/Automotive.Marketplace.Server.csproj", "Automotive.Marketplace.Server/"]
COPY ["Automotive.Marketplace.Application/Automotive.Marketplace.Application.csproj", "Automotive.Marketplace.Application/"]
COPY ["Automotive.Marketplace.Infrastructure/Automotive.Marketplace.Infrastructure.csproj", "Automotive.Marketplace.Infrastructure/"]
COPY ["Automotive.Marketplace.Domain/Automotive.Marketplace.Domain.csproj", "Automotive.Marketplace.Domain/"]

RUN dotnet restore "Automotive.Marketplace.Server/Automotive.Marketplace.Server.csproj"

COPY . .

WORKDIR "/src/Automotive.Marketplace.Server"
RUN dotnet build "Automotive.Marketplace.Server.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Automotive.Marketplace.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Automotive.Marketplace.Server.dll"]

# Development stage (for hot-reloading)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev

# Install Visual Studio Debugger (in order to have the ability to attach to it)
RUN apt-get update && apt-get install -y curl unzip && \
    curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /remote_debugger && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY ["Automotive.Marketplace.Server/Automotive.Marketplace.Server.csproj", "Automotive.Marketplace.Server/"]
COPY ["Automotive.Marketplace.Application/Automotive.Marketplace.Application.csproj", "Automotive.Marketplace.Application/"]
COPY ["Automotive.Marketplace.Infrastructure/Automotive.Marketplace.Infrastructure.csproj", "Automotive.Marketplace.Infrastructure/"]
COPY ["Automotive.Marketplace.Domain/Automotive.Marketplace.Domain.csproj", "Automotive.Marketplace.Domain/"]
RUN dotnet restore "./Automotive.Marketplace.Server/Automotive.Marketplace.Server.csproj"

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "watch", "run", "--no-restore", "--project", "/app/Automotive.Marketplace.Server/Automotive.Marketplace.Server.csproj"]