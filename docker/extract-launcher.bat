@echo off
for %%R in (win-x64 linux-x64 osx-x64 osx-arm64) do (
  mkdir out\%%R 2>nul
  for /f %%i in ('docker create --entrypoint "" docker-launcher-%%R:latest') do set ID=%%i
  docker export %ID% | tar -x -C out\%%R
  docker rm %ID%
)