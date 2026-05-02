FROM node:16-bullseye-slim AS node

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY --from=node /usr/local/ /usr/local/

RUN node --version \
    && npm --version

COPY ["src/WebUI/WebUI.csproj", "src/WebUI/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]

RUN dotnet restore "src/WebUI/WebUI.csproj"

COPY ["src/WebUI/ClientApp/package.json", "src/WebUI/ClientApp/package-lock.json", "src/WebUI/ClientApp/"]
RUN cd /src/src/WebUI/ClientApp && npm ci

COPY ["src/", "src/"]

RUN dotnet publish "src/WebUI/WebUI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TwitterClone.WebUI.dll"]
