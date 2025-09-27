FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar o arquivo de projeto primeiro (para melhor cache)
COPY ["AuthService.csproj", "."]
RUN dotnet restore "AuthService.csproj"

# Copiar o resto do código fonte
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:5001
EXPOSE 5001
ENTRYPOINT ["dotnet", "AuthService.dll"]