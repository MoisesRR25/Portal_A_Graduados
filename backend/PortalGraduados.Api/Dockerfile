
# Etapa 1: Compilar y publicar la API con .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY PortalGraduados.Api.csproj ./
RUN dotnet restore PortalGraduados.Api.csproj

COPY . .
RUN dotnet publish PortalGraduados.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Etapa 2: Ejecutar la API
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PortalGraduados.Api.dll"]
