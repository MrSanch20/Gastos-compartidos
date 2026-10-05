# Etapa 1: compilar y publicar la API con el SDK completo.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/GastosCompartidos.Api/GastosCompartidos.Api.csproj", "src/GastosCompartidos.Api/"]
RUN dotnet restore "src/GastosCompartidos.Api/GastosCompartidos.Api.csproj"

COPY src/GastosCompartidos.Api/ src/GastosCompartidos.Api/
RUN dotnet publish "src/GastosCompartidos.Api/GastosCompartidos.Api.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Etapa 2: imagen final, solo con el runtime de ASP.NET (mucho más ligera que el SDK).
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GastosCompartidos.Api.dll"]
