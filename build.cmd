@echo off

dotnet run --project build/build.csproj -c Release -- %*
