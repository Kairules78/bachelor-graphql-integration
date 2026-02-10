#!/bin/bash
set -e

echo "Stopper gammel GraphQL-container (hvis finnes)..."
podman rm -f graphql_dotnet 2>/dev/null || true

echo "Starter GraphQL (.NET + Hot Chocolate) på 127.0.0.1:8080 (kun lokalt)..."
podman run -d \
  --name graphql_dotnet \
  --restart=always \
  -p 127.0.0.1:8080:8080 \
  -v /home/kaiking/bachelor-graphql-integration/myapp:/work \
  -w /work \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  bash -lc 'dotnet run --no-launch-profile --urls "http://0.0.0.0:8080"'

echo ""
echo "GraphQL startet"
echo "Intern (direkte):  http://127.0.0.1:8080/graphql/"
echo "Ekstern (via Nginx): http://37.27.186.225/graphql/"

