# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["PhissingScaming.csproj", "./"]
RUN dotnet restore "PhissingScaming.csproj"

# Copy full source and publish
COPY . .
RUN dotnet publish "PhissingScaming.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render exposes the PORT environment variable (default 10000 or custom)
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "PhissingScaming.dll"]
