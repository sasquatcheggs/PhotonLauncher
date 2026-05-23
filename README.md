##PhotonLauncher

This project was written using Deepseek.
It allows you to conveniently launch and play Photon games over LAN. LuxonServer is required to host a game using this launcher.

#Usage:
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
For now, the files specified in the first four fields must exist for the launcher to work properly. In they future, they may be auto-generated.

Redirect injection functionality is planned, but for now, PhotonRedirector is required.
