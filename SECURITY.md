# Security Policy

## Vulnerability scope we acknowledge

Nitrox does not currently satisfy all ideal security goals.
For now authenticity and integrity in the communication between client and server is not implemented (see issue [#1996](https://github.com/SubnauticaNitrox/Nitrox/issues/1996)).
Therefore users should be aware that all packets sent and received could be manipulated to alter the in-game state or the game world independent of access rights. \
The vulnerabilities we are most concerned about (and which we hope we never find) are vulnerabilities where the game or `client->server->other clients` environment is broken out of. \
For any Remote Code Execution vulnerabilities(RCEs) or the gathering of personal information (except IPs required for the system to function) from clients or servers are severe vulnerabilities which will be immediately addressed. This also includes any access to files and folders that Nitrox does not normally makes accessible (for example the world save files are more or less intentionally accessible).

## Supported Versions

As this project is still rapidly evolving and in a beta state we will only provide security updates to the newest version available.

| Version | Supported          |
| ------- | ------------------ |
| Newest  | :white_check_mark: |
| Other   | :x:                |

## Reporting a Vulnerability

To discreetly report a vulnerability please get in touch with one of our support staff on our [discord](https://discord.gg/E8B4X9s).
Your request will be forwarded to one of the devs which will get in touch with you privately to further discuss and investigate the issue. \
Once the issue is reproduced by the team, work will be started on a fix. You will be informed once the issue has been fixed and when the fix was included in a new release. \
You are only allowed to publicly disclose the vulnerability one week after the mentioned release to give the users time to update their software.
