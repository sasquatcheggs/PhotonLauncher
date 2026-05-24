# PhotonLauncher

This project was written using Deepseek.
It allows you to conveniently launch and play Photon games over LAN. [LuxonServer](https://github.com/niansa/LuxonServer) is required to host a game using this launcher.

## Usage:
On first launch, `launcher_config.json` will be created. It will look like this:
```
{
  "GameExecutablePath": "",
  "LuxonServerPath": "Luxon Server.exe",
  "LuxonConfigPath": "config.yml",
  "RedirectorConfigPath": "LANSettings.txt",
  "LastHostIP": "",
  "LastJoinIP": "127.0.0.1",
  "KeepLauncherOpenOnJoin": false,
  "CustomHostIP": ""
}
```
The game executable must be specified in the first field.

For now, the files specified in the first four fields must exist for the launcher to work properly. In they future, the configs may be auto-generated.

Redirect injection functionality is planned, but for now, [PhotonRedirector](https://github.com/sasquatcheggs/PhotonRedirector) is required.

## Releases:
Releases will be published as an all-in-one drop-in solution.

They will include the following:
- PhotonRedirector.dll and LANSettings.txt
- winmm.dll and winmm.txt (for injecting PhotonRedirector)
- Luxon Server.exe, config.yml, and LuxonLicense.txt
- PhotonLauncher.exe compiled as a self-contained executable and launcher_config.json

Once these files are placed next to a game executable, the executable must be specified in `launcher_config.json` before it is ready to use.
