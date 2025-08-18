# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

# Copy csproj and restore
COPY src/SearchEngine_.csproj ./SearchEngine_.csproj
RUN dotnet restore ./SearchEngine_.csproj

# Copy the rest and publish
COPY src/ .
RUN dotnet publish ./SearchEngine_.csproj -c Release -o /out /p:UseAppHost=false

# Runtime stage (Debian-based to satisfy native deps for PDF/Office libraries)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 libfreetype6 libharfbuzz0b libpng16-16 libx11-6 fonts-dejavu ca-certificates \
  && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://0.0.0.0:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
WORKDIR /app
COPY --from=build /out ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "SearchEngine_.dll"]