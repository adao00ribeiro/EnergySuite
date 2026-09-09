# =========================
# Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

WORKDIR /src

# Copia apenas os arquivos necessários para restore (Aproveitamento de Cache)
COPY EtrmService.API/EtrmService.API.csproj EtrmService.API/
COPY EtrmService.Application/EtrmService.Application.csproj EtrmService.Application/
COPY EtrmService.Domain/EtrmService.Domain.csproj EtrmService.Domain/
COPY EtrmService.Infrastructure/EtrmService.Infrastructure.csproj EtrmService.Infrastructure/

# Restore com cache isolado do NuGet
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore EtrmService.API/EtrmService.API.csproj

# Copia o restante do código fonte
COPY . .

# Publish direto sem restore redundante e sem PDBs
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish EtrmService.API/EtrmService.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    /p:DebugType=None \
    /p:DebugSymbols=false


# =========================
# Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

COPY --from=build /app/publish .

# Executa com o usuário não-root padrão do .NET 8 (Boas práticas K8s / Segurança)
USER $APP_UID

ENTRYPOINT ["dotnet", "EtrmService.API.dll"]

