# Multi-stage build for .NET 8 ETRM Service
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["EtrmService.API/EtrmService.API.csproj", "EtrmService.API/"]
COPY ["EtrmService.Application/EtrmService.Application.csproj", "EtrmService.Application/"]
COPY ["EtrmService.Domain/EtrmService.Domain.csproj", "EtrmService.Domain/"]
COPY ["EtrmService.Infrastructure/EtrmService.Infrastructure.csproj", "EtrmService.Infrastructure/"]
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore "EtrmService.API/EtrmService.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/EtrmService.API"
RUN dotnet build "EtrmService.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "EtrmService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EtrmService.API.dll"]
