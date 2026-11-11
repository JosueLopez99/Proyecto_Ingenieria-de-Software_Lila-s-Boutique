# Etapa 1: Compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivo csproj y restaurar
COPY ["LilasBoutique/LilasBoutique.csproj", "LilasBoutique/"]
RUN dotnet restore "LilasBoutique/LilasBoutique.csproj"

# Copiar todo el código y publicar
COPY . .
WORKDIR "/src/LilasBoutique"
RUN dotnet publish "LilasBoutique.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Imagen para ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "LilasBoutique.dll"]