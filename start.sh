#!/bin/bash
set -e

echo "Stopper gammel GraphQL-container (hvis finnes)..."
podman rm -f graphql_dotnet 2>/dev/null || true

echo "Starter GraphQL (.NET + Hot Chocolate)..."
podman run -d \
  --name graphql_dotnet \
  --restart=always \
  -p 127.0.0.1:8081:8081 \
  -v /home/kaiking/bachelor-graphql-integration/myapp:/work \
  -w /work \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  bash -lc 'dotnet run --no-launch-profile --urls "http://0.0.0.0:8081"'

echo ""
echo "GraphQL startet"
echo "Intern:  http://127.0.0.1:8081/graphql"
echo "Ekstern: http://37.27.186.225/graphql"

