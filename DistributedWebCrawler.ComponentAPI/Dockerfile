#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DistributedWebCrawler.ComponentAPI/DistributedWebCrawler.ComponentAPI.csproj", "DistributedWebCrawler.ComponentAPI/"]
COPY ["DistributedWebCrawler.Extensions.RabbitMQ/DistributedWebCrawler.Extensions.RabbitMQ.csproj", "DistributedWebCrawler.Extensions.RabbitMQ/"]
COPY ["DistributedWebCrawler.Core/DistributedWebCrawler.Core.csproj", "DistributedWebCrawler.Core/"]
COPY ["DistributedWebCrawler.Extensions.DependencyInjection/DistributedWebCrawler.Extensions.DependencyInjection.csproj", "DistributedWebCrawler.Extensions.DependencyInjection/"]
COPY ["DistributedWebCrawler.Extensions.Redis/DistributedWebCrawler.Extensions.Redis.csproj", "DistributedWebCrawler.Extensions.Redis/"]
RUN dotnet restore "DistributedWebCrawler.ComponentAPI/DistributedWebCrawler.ComponentAPI.csproj"
COPY . .
WORKDIR "/src/DistributedWebCrawler.ComponentAPI"
RUN dotnet build "DistributedWebCrawler.ComponentAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DistributedWebCrawler.ComponentAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DistributedWebCrawler.ComponentAPI.dll"]