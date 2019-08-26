FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
ENV GOOGLE_APPLICATION_CREDENTIALS="/app/vibechat-ng-firebase-adminsdk-te5qc-a9cfc0974d.json"

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["Vibechat.Web/Vibechat.Web.csproj", "Vibechat.Web/"]
COPY . .
WORKDIR "/src/Vibechat.Web"
RUN dotnet build "Vibechat.Web.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Vibechat.Web.csproj" -c Release -o /app

FROM base AS published
WORKDIR /app
COPY --from=publish /app .

#Angular build
FROM node as nodebuilder

# set working directory
RUN mkdir /usr/src/app
WORKDIR /usr/src/app

# add `/usr/src/app/node_modules/.bin` to $PATH
ENV PATH /usr/src/app/node_modules/.bin:$PATH


# install and cache app dependencies
COPY Vibechat.Web/ClientApp/package.json /usr/src/app/package.json
RUN npm install
RUN npm install -g @angular/cli@8.1.3 --unsafe

# add app

COPY Vibechat.Web/ClientApp/. /usr/src/app

RUN npm run build.prod
RUN npm run build.prod:ru

#End Angular build

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN mkdir -p /app/ClientApp/dist

# install libgdiplus for System.Drawing
RUN apt-get update && \
    apt-get install -y --allow-unauthenticated libgdiplus libc6-dev

# install x11 for System.Drawing
RUN apt-get update && \
    apt-get install -y --allow-unauthenticated libx11-dev

COPY --from=nodebuilder /usr/src/app/dist/. /app/ClientApp/dist/

RUN apt-get install -y --allow-unauthenticated python3
RUN python3 fixpwa.py

ENTRYPOINT ["dotnet", "Vibechat.Web.dll"]