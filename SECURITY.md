# Security Policy

## Vulnerability scope we acknowledge

Nitrox in itself does currently not satisfy all ideal security goals.
For now authenticity and integrity in the communication between client and server is not implemented (see issue [#1996](https://github.com/SubnauticaNitrox/Nitrox/issues/1996)).
Therefor users should be aware that all packets send and received can and could be manipulated to alter the in-game state or the game world independent of access rights. \
The vulnerabilities we are looking for (or better which should not be there) are where the insecure `client<->serer<->client` environment is broken out of. \
For example we don't tolerate any RCEs or gathering personal information (except IPs required for the system to function) from clients or servers. This also includes information beyond the files and folders Nitrox normally makes accessible (for example the world save files are more or less intentionally accessible).

## Supported Versions

As this project is still rapidly evolving and in a beta state we limit ourselves to only provide security updates to the newest version available.

| Version | Supported          |
| ------- | ------------------ |
| Newest  | :white_check_mark: |
| Other   | :x:                |

## Reporting a Vulnerability

To discreetly report a vulnerability please get in touch with one of our support staff at our [discord](https://discord.gg/E8B4X9s).
They will forward your request to one of the devs which will get in touch with you privately. \
Once the issue is acknowledged by the team they will inform you once the issue has been fixed and when the fix was included in a new release. \
We would like that you only publicly disclose the vulnerability one week after the mentioned release to give the users time to update their software.
