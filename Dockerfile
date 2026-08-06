# syntax=docker/dockerfile:1

# --- Etapa 1: build/publish ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia só os .csproj primeiro para aproveitar cache de camadas do Docker
# (restore só roda de novo se as dependências mudarem, não a cada alteração de código).
COPY ["src/TaskFlow.Api/TaskFlow.Api.csproj", "src/TaskFlow.Api/"]
COPY ["src/TaskFlow.Domain/TaskFlow.Domain.csproj", "src/TaskFlow.Domain/"]
COPY ["src/TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj", "src/TaskFlow.Infrastructure/"]
RUN dotnet restore "src/TaskFlow.Api/TaskFlow.Api.csproj"

COPY src/ src/
WORKDIR /src/src/TaskFlow.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# --- Etapa 2: runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Azure Container Apps encaminha tráfego HTTP puro para o container
# (TLS é terminado no ingress da plataforma).
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskFlow.Api.dll"]
