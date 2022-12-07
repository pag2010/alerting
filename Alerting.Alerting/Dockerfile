#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Alerting.Alerting/Alerting.TelegramBot.csproj", "Alerting.Alerting/"]
RUN dotnet restore "Alerting.Alerting/Alerting.TelegramBot.csproj"
COPY . .
WORKDIR "/src/Alerting.Alerting"
RUN dotnet build "Alerting.TelegramBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Alerting.TelegramBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Alerting.TelegramBot.dll"]