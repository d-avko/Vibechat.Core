FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
ENV GOOGLE_APPLICATION_CREDENTIALS="/app/vibechat-ng-firebase-adminsdk-te5qc-a9cfc0974d.json"
ENV ASPNETCORE_URLS=https://denisavko.me:443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Vibechat.Web/Vibechat.Web.csproj", "Vibechat.Web/"]
COPY . .
WORKDIR "/src/Vibechat.Web"
RUN dotnet build "Vibechat.Web.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Vibechat.Web.csproj" -c Release -r linux-x64 -o /app

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
RUN npm install -g @angular/cli@7.2.0 --unsafe

# add app

COPY Vibechat.Web/ClientApp/. /usr/src/app

RUN npm run build

#End Angular build

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN mkdir -p /app/ClientApp/dist
#Add skiasharp references
RUN apt-get update
RUN apt-get install -y libfreetype6
RUN apt-get install -y libfontconfig1
COPY --from=nodebuilder /usr/src/app/dist/. /app/ClientApp/dist/
ENTRYPOINT ["dotnet", "Vibechat.Web.dll"]