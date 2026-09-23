# syntax=docker/dockerfile:1
# Build context esperado: backend/ (ex.: `docker build -f Dockerfile .` a
# partir de dentro de backend/, ou `docker build -f backend/Dockerfile
# backend/` a partir da raiz do mono-repo).

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia só os .csproj primeiro para o cache de camadas do Docker só invalidar
# o restore quando uma dependência muda, não a cada alteração de código.
COPY src/Mesada.Domain/Mesada.Domain.csproj src/Mesada.Domain/
COPY src/Mesada.Application/Mesada.Application.csproj src/Mesada.Application/
COPY src/Mesada.Infrastructure/Mesada.Infrastructure.csproj src/Mesada.Infrastructure/
COPY src/Mesada.Api/Mesada.Api.csproj src/Mesada.Api/
RUN dotnet restore src/Mesada.Api/Mesada.Api.csproj

COPY src/ src/
RUN dotnet publish src/Mesada.Api/Mesada.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN useradd --uid 1654 --create-home mesada
USER mesada

COPY --from=build --chown=mesada:mesada /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Mesada.Api.dll"]
