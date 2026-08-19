# NG Kütahya Seramik Admin Panel — çok aşamalı (multi-stage) Docker build.
# Yalnızca Presentation projesi (ve onun proje referansları) publish edilir; test projeleri image'a dahil edilmez.

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Önce yalnızca .csproj dosyaları kopyalanır — kaynak kod değişse bile bağımlılıklar aynıysa
# `dotnet restore` katmanı Docker layer cache'inden yeniden kullanılır.
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Presentation/Presentation.csproj src/Presentation/
RUN dotnet restore src/Presentation/Presentation.csproj

COPY src/ src/
RUN dotnet publish src/Presentation/Presentation.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Container içinde root olmayan bir kullanıcı ile çalışır (güvenlik sertleştirmesi).
RUN adduser --disabled-password --gecos "" --home /app appuser

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# Yüklenen dosyaların (ADR-006/ADR-013) kalıcı volume ile eşleneceği klasör — önceden oluşturulur
# ki uygulama kullanıcısı (appuser) buraya yazabilsin.
RUN mkdir -p /app/wwwroot/uploads /app/data-protection-keys && chown -R appuser:appuser /app

USER appuser

ENTRYPOINT ["dotnet", "Presentation.dll"]
