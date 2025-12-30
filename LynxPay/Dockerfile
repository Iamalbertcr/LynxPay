# ===============================
# BUILD STAGE
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos el csproj y restauramos dependencias
COPY LynxPay.csproj .
RUN dotnet restore LynxPay.csproj

# Copiamos todo el código
COPY . .
RUN dotnet publish LynxPay.csproj -c Release -o /app/publish

# ===============================
# RUNTIME STAGE
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LynxPay.dll"]
