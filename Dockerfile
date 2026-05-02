# ---------------- Stage 1: Build (.NET 5 + Node) ----------------
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

# نصب Node.js در محیط دات‌نت 5
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_16.x | bash - && \
    apt-get install -y nodejs

# کپی کل پروژه
COPY . .

# رستور پکیج‌ها
RUN dotnet restore src/WebUI/WebUI.csproj

# پابلیش پروژه (این مرحله npm install و npm run build را اجرا می‌کند)
RUN dotnet publish src/WebUI/WebUI.csproj -c Release -o /app/publish


# ---------------- Stage 2: Runtime (.NET 5) ----------------
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TwitterClone.WebUI.dll"]
