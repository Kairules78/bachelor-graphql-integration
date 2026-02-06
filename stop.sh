#!/bin/bash

echo "Stopper containere..."
podman stop nginx  graphql_dotnet 2>/dev/null || true
podman rm nginx  graphql_dotnet 2>/dev/null || true
echo "Stoppet"
