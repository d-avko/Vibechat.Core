FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Vibechat.FileServer.csproj", "."]
COPY . .
RUN dotnet build "Vibechat.FileServer.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Vibechat.FileServer.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Vibechat.FileServer.dll"]