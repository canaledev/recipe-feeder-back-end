# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Feedy.Domain/Feedy.Domain.csproj", "src/Feedy.Domain/"]
COPY ["src/Feedy.Application/Feedy.Application.csproj", "src/Feedy.Application/"]
COPY ["src/Feedy.Infrastructure/Feedy.Infrastructure.csproj", "src/Feedy.Infrastructure/"]
COPY ["src/Feedy.Api/Feedy.Api.csproj", "src/Feedy.Api/"]
COPY ["nuget.config", "."]

RUN dotnet restore "src/Feedy.Api/Feedy.Api.csproj"

COPY . .
RUN dotnet build "src/Feedy.Api/Feedy.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "src/Feedy.Api/Feedy.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Feedy.Api.dll"]
