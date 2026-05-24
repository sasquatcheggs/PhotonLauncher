# PhotonLauncher

PhotonLauncher was made to simplify the process of connecting with friends to play your Photon games over LAN. It manages the configuration files of the following software.
- [LuxonServer](https://github.com/niansa/LuxonServer) > Required to host a game.
- [PhotonRedirector](https://github.com/sasquatcheggs/PhotonRedirector) > Required for joining, even if you are the host.

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
  "InjectorDllPath": "PhotonRedirector.dll",
  "EnableDllInjection": true
}
```
The game executable must be specified in the first field.
Right now, the files referenced in the first four fields must exist for all features to work. In the future, the configs may be auto-generated.

DLL injection is implimented, but can be disabled if you choose to inject PhotonRedirector another way.  
Support for more methods of redirecting Photon's traffic (such as hosts file editing) may be implimented in the future.

## Releases:
Releases will be published as an all-in-one drop-in solution.

They will include the following:
- PhotonRedirector.dll and LANSettings.txt
- Luxon Server.exe, config.yml, and LuxonLicense.txt
- PhotonLauncher.exe compiled as a self-contained executable and launcher_config.json

Once these files are placed next to a game executable, the executable must be specified in `launcher_config.json` before it is ready to use.

## Disclaimer:

This project was written using Deepseek. I tinker with it when I get time. It may be junk.

## Credits:

- [***niansa***](https://github.com/niansa) > This launcher only simplifies what his server makes possible.
- [***BTFighter***](https://github.com/BTFighter) > His inspiration, motivation, and assistance have been invaluable.
