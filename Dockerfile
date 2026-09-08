# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY GastAi.sln ./
COPY src/GastAi.Domain/GastAi.Domain.csproj src/GastAi.Domain/
COPY src/GastAi.Contracts/GastAi.Contracts.csproj src/GastAi.Contracts/
COPY src/GastAi.Application/GastAi.Application.csproj src/GastAi.Application/
COPY src/GastAi.Infrastructure/GastAi.Infrastructure.csproj src/GastAi.Infrastructure/
COPY src/GastAi.Api/GastAi.Api.csproj src/GastAi.Api/
COPY src/GastAi.Web/GastAi.Web.csproj src/GastAi.Web/
RUN dotnet restore src/GastAi.Api/GastAi.Api.csproj

COPY src/ src/
RUN dotnet publish src/GastAi.Api/GastAi.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GastAi.Api.dll"]
