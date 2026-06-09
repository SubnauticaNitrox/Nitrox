# Agent Instructions

- Always commit and push changes to GitHub.
- Keep commit messages short and light.
- If the project is deployed somewhere, make sure production is on the latest live deployment.

## Operations

- When Viktor asks to restart the Subnautica server on `baller`, he means the Docker container `nitrox-subnautica` on host `baller`. Restart it with `ssh baller 'docker restart -t 30 nitrox-subnautica'`, then verify it is running and listening on UDP `11000` and LAN broadcast UDP `1467`.
