#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
#SPDX-FileCopyrightText: Copyright (c) 2021 C-Labs
#SPDX-License-Identifier: MPL-2.0

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["SampleWorkerService/SampleWorkerService.csproj", "SampleWorkerService/"]
RUN dotnet restore "SampleWorkerService/SampleWorkerService.csproj"
COPY . .
WORKDIR "/src/SampleWorkerService"
RUN dotnet build "SampleWorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SampleWorkerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SampleWorkerService.dll"]