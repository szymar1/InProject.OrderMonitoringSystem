﻿FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY OrderConsumer.csproj ./
RUN dotnet restore

COPY . .

RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app

COPY --from=build /app/out .

RUN mkdir -p Logs

ENTRYPOINT ["dotnet", "OrderConsumer.dll"]