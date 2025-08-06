# Base image with ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ./LightNotes.API/LightNotes.API.csproj ./LightNotes.API/
COPY ./LightNotes.Application/LightNotes.Application.csproj ./LightNotes.Application/
COPY ./LightNotes.Domain/LightNotes.Domain.csproj ./LightNotes.Domain/
COPY ./LightNotes.Infrastructure/LightNotes.Infrastructure.csproj ./LightNotes.Infrastructure/

# Restore NuGet packages
RUN dotnet restore ./LightNotes.API/LightNotes.API.csproj

# Copy all source files and build the app
COPY . .
WORKDIR /src/LightNotes.API
RUN dotnet publish -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "LightNotes.API.dll"]
