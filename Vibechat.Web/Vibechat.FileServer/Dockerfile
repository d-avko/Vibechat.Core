FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 443
ENV ASPNETCORE_URLS=https://fileserver.denisavko.me:443

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
RUN mkdir /app/wwwroot/Uploads
ENTRYPOINT ["dotnet", "Vibechat.FileServer.dll"]