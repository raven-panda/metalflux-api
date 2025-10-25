# Metalflux Web App

## Intro

This project involves a web application of video streaming. It's separated into three subproject : the front-end, the ASP.NET back-end API REST and a S3 service to store files. They are avaiable there :

- [Front-end](https://github.com/raven-panda/metalflux-app-front)
- [Back-end](https://github.com/raven-panda/metalflux-api)
- [S3 Service](https://github.com/raven-panda/metalflux-s3-service)

## How to get started

### Requirements

For this project you'll need to :

- Configure an environment for C# development and choose and IDE, personnaly I use [JetBrains' Rider](https://www.jetbrains.com/rider/).

### Step-by-step

Start the ASP.NET API using your IDE or using this command
```bash
cd ./MetalfluxApi/MetalfluxApi

#These profiles are defined in `launchSettings.json` file

dotnet run --launch-profile "https"
# or if you don't want HTTPS
dotnet run --launch-profile "http"
```
 