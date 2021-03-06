# build client
FROM node:12 as BUILD_CLIENT
COPY ./promenade-front /app
WORKDIR /app
ARG VUE_APP_MAPBOX_TOKEN=token_unknown
ENV VUE_APP_MAPBOX_TOKEN ${VUE_APP_MAPBOX_TOKEN}
RUN npm install
RUN npm run build

# build server
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as BUILD_SERVER
WORKDIR /app
COPY ./promenade-back /app
RUN dotnet restore
RUN dotnet publish -c Release -o out

# copy client and server
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=BUILD_SERVER /app/out /app
COPY --from=BUILD_CLIENT /app/dist /app/wwwroot

# run
EXPOSE 80
ENTRYPOINT ["dotnet", "Promenade.dll"]