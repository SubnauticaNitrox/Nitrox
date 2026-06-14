#!/usr/bin/env sh
for RID in win-x64 linux-x64 osx-x64 osx-arm64; do
  mkdir -p out/$RID
  id=$(docker create --entrypoint /bin/sh docker-launcher-$RID:latest 2>/dev/null || \
       docker create --entrypoint "" docker-launcher-$RID:latest)
  docker export $id | tar -x -C out/$RID
  docker rm $id
done