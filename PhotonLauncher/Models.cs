using System.Collections.Generic;
using Newtonsoft.Json;

namespace LuxonLauncher
{
    public class LauncherConfig
    {
        public string GameExecutablePath { get; set; } = "";
        public string LuxonServerPath { get; set; } = "LuxonServer.exe";
        public string LuxonConfigPath { get; set; } = "config.yml";
        public string RedirectorConfigPath { get; set; } = "LANSettings.txt";
        public string LastHostIP { get; set; } = "127.0.0.1";
        public string LastJoinIP { get; set; } = "";
        public bool KeepLauncherOpenOnJoin { get; set; } = false;
        public string CustomHostIP { get; set; } = "";
        public string InjectorDllPath { get; set; } = "PhotonRedirector.dll";
        public bool EnableDllInjection { get; set; } = true;
    }

    // Simplified YAML configuration structure - using Dictionary for flexibility
    public class LuxonConfig
    {
        public List<Dictionary<string, object>> NameServer { get; set; }
        public List<Dictionary<string, object>> MasterServer { get; set; }
        public List<Dictionary<string, object>> GameServer { get; set; }
        public List<Dictionary<string, object>> HTTP { get; set; }
    }

    public class NetworkAdapterInfo
    {
        public string Name { get; set; }
        public string IPAddress { get; set; }
        public override string ToString() => $"{Name} ({IPAddress})";
    }
}