# ===============================
# BUILD STAGE
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos solo el csproj y restauramos dependencias
COPY LynxPay/LynxPay.csproj LynxPay/
RUN dotnet restore LynxPay/LynxPay.csproj

# Copiamos el resto del código
COPY LynxPay/ LynxPay/
WORKDIR /src/LynxPay

RUN dotnet publish LynxPay.csproj -c Release -o /app/publish

# ===============================
# RUNTIME STAGE
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LynxPay.dll"]
