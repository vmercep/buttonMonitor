#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim-arm32v7 AS base

WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["buttonMonitor.csproj", "./"]
RUN dotnet restore "buttonMonitor.csproj" -r linux-arm
COPY . .
WORKDIR "/src/"
RUN dotnet build "buttonMonitor.csproj" -c Release -o /app/build -r linux-arm

FROM build AS publish
RUN dotnet publish "buttonMonitor.csproj" -c Release -o /app/publish -r linux-arm

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "buttonMonitor.dll"]
