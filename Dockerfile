# ---------- Node base (mirror)
FROM hub.hamdocker.ir/library/node:16-bullseye-slim AS node


# ---------- Build Stage (.NET SDK)
FROM mcr.hamdocker.ir/dotnet/sdk:5.0 AS build
WORKDIR /src


COPY --from=node /usr/local/ /usr/local/

# تنظیم mirror برای npm
RUN npm config set registry https://repo.hmirror.ir/npm/

# تست نصب node
RUN node --version && npm --version


# ---------- copy csproj for restore cache
COPY ["src/WebUI/WebUI.csproj", "src/WebUI/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]

RUN dotnet restore "src/WebUI/WebUI.csproj"


# ---------- install frontend deps
COPY ["src/WebUI/ClientApp/package.json", "src/WebUI/ClientApp/package-lock.json", "src/WebUI/ClientApp/"]

RUN cd src/WebUI/ClientApp \
    && npm ci


# ---------- copy source
COPY src/ src/


# ---------- publish
RUN dotnet publish "src/WebUI/WebUI.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# ---------- Runtime image
FROM mcr.hamdocker.ir/dotnet/aspnet:5.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TwitterClone.WebUI.dll"]
