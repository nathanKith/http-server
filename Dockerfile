# FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
FROM ubuntu:latest as executor

RUN apt-get update && apt-get install -y wget

RUN wget https://packages.microsoft.com/config/ubuntu/20.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN  dpkg -i packages-microsoft-prod.deb

RUN apt-get update; \
    apt-get install -y apt-transport-https && \
    apt-get update && \
    apt-get install -y dotnet-sdk-5.0

WORKDIR /app

COPY . /app

RUN dotnet build

WORKDIR /app/bin/Debug/net5.0

ENTRYPOINT [ "/app/bin/Debug/net5.0/http-server" ]
