#!/bin/bash
set -e

echo "Stopper gamle containere..."
podman rm -f graphql_dotnet graphql_java nginx 2>/dev/null || true

echo "Sørger for nettverk..."
podman network inspect webnet >/dev/null 2>&1 || podman network create webnet

echo "Starter C# GraphQL (Hot Chocolate)..."
podman run -d --restart=always --name graphql_dotnet \
  --network webnet \
  -v /home/kaiking//bachelor-graphql-integration/myapp:/work \
  -w /work \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  bash -lc 'dotnet run --no-launch-profile --urls "http://0.0.0.0:8081"'

#echo "Starter Java GraphQL (Spring Boot)..."
# Forutsetter at du har bygget en image som heter graphql-java:latest
# og at appen lytter på 8080 i containeren.
#podman run -d --restart=always --name graphql_java \
#  --network webnet \
#  graphql-java:latest

echo "Starter Nginx..."
podman run -d --restart=always --name nginx \
  --network webnet \
  -p 0.0.0.0:8080:80 \
  -v /home/kaiking//bachelor-graphql-integration/nginx/default.conf:/etc/nginx/conf.d/default.conf:ro \
  docker.io/library/nginx:alpine

echo "Alt startet"
echo "Dotnet: http://37.27.186.225/graphql-dotnet/"
echo "Java:   http://37.27.186.225/graphql-java/"

