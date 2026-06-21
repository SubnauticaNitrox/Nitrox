# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src
COPY . .

RUN --mount=type=bind,from=subnautica,target=/subnautica \
    SUBNAUTICA_INSTALLATION_PATH=/subnautica \
    dotnet publish Nitrox.Server.Subnautica/Nitrox.Server.Subnautica.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    -o /out

FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /nitrox
COPY --from=build /out ./

VOLUME ["/subnautica", "/data"]
EXPOSE 11000/udp

ENTRYPOINT ["dotnet", "Nitrox.Server.Subnautica.dll", \
            "--game-path=/subnautica", \
            "--data-path=/data"]