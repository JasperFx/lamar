#!/usr/bin/env bash
set -euo pipefail


dotnet run -p build/build.csproj -c Release -- "$@"
