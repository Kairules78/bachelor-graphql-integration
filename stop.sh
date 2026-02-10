#!/bin/bash

echo "Stopper containere..."
podman stop graphql_dotnet 2>/dev/null || true
podman rm graphql_dotnet 2>/dev/null || true
echo "Stoppet"
