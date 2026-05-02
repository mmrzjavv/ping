# --- Stage 1: Build React (ClientApp) ---
FROM node:18 AS node-build
WORKDIR /src

# فقط package.json ها را کپی کن برای cache بهتر
COPY TwitterClone.WebUI/ClientApp/package*.json ./TwitterClone.WebUI/ClientApp/

RUN cd TwitterClone.WebUI/ClientApp && npm install

# حالا کل پروژه را کپی کن
COPY . .

# Build React
RUN cd TwitterClone.WebUI/ClientApp && npm run build


# --- Stage 2: Build .NET Backend ---
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS dotnet-build
WORKDIR /src

COPY . .

# کپی خروجی React build به مسیر درست
COPY --from=node-build /src/TwitterClone.WebUI/ClientApp/build /src/TwitterClone.WebUI/ClientApp/build

# Restore و Publish پروژه
RUN dotnet restore TwitterClone.WebUI/TwitterClone.WebUI.csproj
RUN dotnet publish TwitterClone.WebUI/TwitterClone.WebUI.csproj -c Release -o /app/publish


# --- Stage 3: Final Runtime Image ---
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS final
WORKDIR /app

# کپی خروجی publish شده
COPY --from=dotnet-build /app/publish .

# پورت وب
EXPOSE 80

ENTRYPOINT ["dotnet", "TwitterClone.WebUI.dll"]
