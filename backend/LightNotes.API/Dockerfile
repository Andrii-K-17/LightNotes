FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["LightNotes.API/LightNotes.API.csproj", "LightNotes.API/"]
COPY ["LightNotes.Application/LightNotes.Application.csproj", "LightNotes.Application/"]
COPY ["LightNotes.Domain/LightNotes.Domain.csproj", "LightNotes.Domain/"]
COPY ["LightNotes.Infrastructure/LightNotes.Infrastructure.csproj", "LightNotes.Infrastructure/"]
COPY Directory.Packages.props .

RUN dotnet restore "LightNotes.API/LightNotes.API.csproj"

COPY . .

WORKDIR "/src/LightNotes.API"

RUN dotnet publish "LightNotes.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "LightNotes.API.dll"]
