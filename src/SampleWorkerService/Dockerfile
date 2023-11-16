#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
#SPDX-FileCopyrightText: Copyright (c) 2021-2023 C-Labs
#SPDX-License-Identifier: MPL-2.0

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8710
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SampleWorkerService/SampleWorkerService.csproj", "SampleWorkerService/"]
COPY . .
COPY "./Nugets" "SampleWorkerService"
COPY "SampleWorkerService/NuGet.config" "SampleWorkerService"
RUN dotnet restore "SampleWorkerService/SampleWorkerService.csproj" --configfile "SampleWorkerService/NuGet.config"
WORKDIR "/src/SampleWorkerService"
RUN dotnet build "SampleWorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SampleWorkerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SampleWorkerService.dll"]