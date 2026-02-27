FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files for layer caching
COPY src/Scheduly.Domain/Scheduly.Domain.csproj src/Scheduly.Domain/
COPY src/Scheduly.Application/Scheduly.Application.csproj src/Scheduly.Application/
COPY src/Scheduly.Infrastructure/Scheduly.Infrastructure.csproj src/Scheduly.Infrastructure/
COPY src/Scheduly.Api/Scheduly.Api.csproj src/Scheduly.Api/

RUN dotnet restore src/Scheduly.Api/Scheduly.Api.csproj

# Copy all source code
COPY src/ src/

# Build and publish
RUN dotnet publish src/Scheduly.Api/Scheduly.Api.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Scheduly.Api.dll"]
