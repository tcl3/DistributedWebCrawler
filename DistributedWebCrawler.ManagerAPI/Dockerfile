#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DistributedWebCrawler.ManagerAPI/DistributedWebCrawler.ManagerAPI.csproj", "DistributedWebCrawler.ManagerAPI/"]
COPY ["DistributedWebCrawler.Extensions.RabbitMQ/DistributedWebCrawler.Extensions.RabbitMQ.csproj", "DistributedWebCrawler.Extensions.RabbitMQ/"]
COPY ["DistributedWebCrawler.Core/DistributedWebCrawler.Core.csproj", "DistributedWebCrawler.Core/"]
COPY ["DistributedWebCrawler.Extensions.DependencyInjection/DistributedWebCrawler.Extensions.DependencyInjection.csproj", "DistributedWebCrawler.Extensions.DependencyInjection/"]
COPY ["DistributedWebCrawler.Extensions.DnsClient/DistributedWebCrawler.Extensions.DnsClient.csproj", "DistributedWebCrawler.Extensions.DnsClient/"]
RUN dotnet restore "DistributedWebCrawler.ManagerAPI/DistributedWebCrawler.ManagerAPI.csproj"
COPY . .
WORKDIR "/src/DistributedWebCrawler.ManagerAPI"
RUN dotnet build "DistributedWebCrawler.ManagerAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DistributedWebCrawler.ManagerAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DistributedWebCrawler.ManagerAPI.dll"]