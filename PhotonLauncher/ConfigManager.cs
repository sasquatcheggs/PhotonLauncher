using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace LuxonLauncher
{
    public static class ConfigManager
    {
        private static readonly string LauncherConfigFile = "launcher_config.json";

        public static LauncherConfig LoadLauncherConfig()
        {
            if (File.Exists(LauncherConfigFile))
            {
                var json = File.ReadAllText(LauncherConfigFile);
                return JsonConvert.DeserializeObject<LauncherConfig>(json) ?? new LauncherConfig();
            }
            return new LauncherConfig();
        }

        public static void SaveLauncherConfig(LauncherConfig config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(LauncherConfigFile, json);
        }

        public static void UpdateLuxonConfig(string configPath, string ipAddress)
        {
            var content = File.ReadAllText(configPath);

            // Pattern matches: "address: anything:port" or "address: anything"
            // Preserves everything else including comments and formatting
            var updated = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"(address:\s*)([^\s:]+)(:\d+)?",
                match =>
                {
                    string prefix = match.Groups[1].Value;
                    string oldValue = match.Groups[2].Value;
                    string port = match.Groups[3].Success ? match.Groups[3].Value : "";

                    // Don't replace if it's already the target IP (skip to avoid unnecessary writes)
                    if (oldValue == ipAddress)
                        return match.Value;

                    return $"{prefix}{ipAddress}{port}";
                }
            );

            // Only write if changes were made
            if (updated != content)
                File.WriteAllText(configPath, updated);
        }

        public static void UpdateRedirectorConfig(string configPath, string ipAddress)
        {
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Redirector config not found: {configPath}");
            }

            var lines = File.ReadAllLines(configPath, Encoding.UTF8);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains('|'))
                {
                    var parts = lines[i].Split('|');
                    if (parts.Length == 2)
                    {
                        var domain = parts[0].Trim();
                        var portPart = parts[1].Trim().Split(':');
                        var port = portPart.Length > 1 ? portPart[1] : "5058";
                        lines[i] = $"{domain} | {ipAddress}:{port}";
                    }
                }
            }

            File.WriteAllLines(configPath, lines, Encoding.UTF8);
        }
    }
}