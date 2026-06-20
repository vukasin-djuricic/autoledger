# syntax=docker/dockerfile:1

# ---- Stage 1: build the Tailwind stylesheet -------------------------------------
FROM node:20-alpine AS css
WORKDIR /web
# Only the files Tailwind needs to scan + its config/input.
COPY src/AutoLedger.Web/tailwind.config.js ./tailwind.config.js
COPY src/AutoLedger.Web/Styles ./Styles
COPY src/AutoLedger.Web/Views ./Views
COPY src/AutoLedger.Web/Components ./Components
RUN npx -y tailwindcss@3.4.17 -i ./Styles/app.css -o ./site.css --minify

# ---- Stage 2: restore, build and publish the app --------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY *.sln Directory.Build.props ./
COPY src/AutoLedger.Domain/*.csproj src/AutoLedger.Domain/
COPY src/AutoLedger.Infrastructure/*.csproj src/AutoLedger.Infrastructure/
COPY src/AutoLedger.Web/*.csproj src/AutoLedger.Web/
RUN dotnet restore src/AutoLedger.Web/AutoLedger.Web.csproj

COPY src/ src/
# Drop in the compiled stylesheet produced by the css stage.
COPY --from=css /web/site.css src/AutoLedger.Web/wwwroot/css/site.css
RUN dotnet publish src/AutoLedger.Web/AutoLedger.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Stage 3: runtime -----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AutoLedger.Web.dll"]
