FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar o arquivo de projeto primeiro (para melhor cache)
COPY ["InventoryService.csproj", "."]
RUN dotnet restore "InventoryService.csproj"

# Copiar o resto do código fonte
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:5002
EXPOSE 5002
ENTRYPOINT ["dotnet", "InventoryService.dll"]