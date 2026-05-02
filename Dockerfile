# Stage 1: Build React app
FROM node:18 AS node-build
WORKDIR /src

# Copy only package files first for better cache
COPY src/WebUI/ClientApp/package*.json ./src/WebUI/ClientApp/
RUN cd src/WebUI/ClientApp && npm install

# Copy the rest of the source
COPY . .

# Build React app
RUN cd src/WebUI/ClientApp && npm run build

# Stage 2: Build .NET backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore src/WebUI/WebUI.csproj
RUN dotnet publish src/WebUI/WebUI.csproj -c Release -o /app/publish

# Stage 3: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
COPY --from=node-build /src/src/WebUI/ClientApp/build ./wwwroot

ENTRYPOINT ["dotnet", "WebUI.dll"]
