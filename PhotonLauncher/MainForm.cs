using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuxonLauncher
{
    public partial class MainForm : Form
    {
        private LauncherConfig launcherConfig;
        private Process luxonProcess;
        private string currentServerIP = "127.0.0.1";

        // UI Controls
        private TabControl tabControl;
        private RichTextBox logBox;
        private Button saveLogButton;
        private Button openWebUIButton;
        private Button stopServerButton;
        private ComboBox ipSelectorCombo;
        private TextBox joinIpTextBox;
        private Button hostButton;
        private Button joinButton;
        private CheckBox keepLauncherCheckbox;
        private Label statusLabel;
        private TextBox customIpBox;
        private Panel hostGroup; // Store reference to the panel for potential use

        public MainForm()
        {
            // Check for admin rights
            if (!IsRunningAsAdministrator())
            {
                AppendLog("[WARNING] Not running as Administrator. Injection may fail.\n");
                AppendLog("[INFO] Restart as Administrator for DLL injection to work.\n");
            }

            InitializeComponent();
            LoadConfig();
            PopulateNetworkAdapters();
            LoadPersistedSettings();
        }

        private bool IsRunningAsAdministrator()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        private void InitializeComponent()
        {
            this.Text = "Photon Launcher";
            this.Size = new Size(664, 508);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = DarkColors.Background;
            this.ForeColor = DarkColors.Foreground;

            // Tab Control
            tabControl = new TabControl { Dock = DockStyle.Fill };
            tabControl.BackColor = DarkColors.Background;
            tabControl.ForeColor = DarkColors.Foreground;

            // Tab 1: Play
            var playTab = new TabPage("Play");
            playTab.BackColor = DarkColors.Background;
            playTab.ForeColor = DarkColors.Foreground;
            var playPanel = CreatePlayPanel();
            playTab.Controls.Add(playPanel);

            // Tab 2: Monitoring
            var monitoringTab = new TabPage("Monitoring");
            monitoringTab.BackColor = DarkColors.Background;
            monitoringTab.ForeColor = DarkColors.Foreground;
            var monitoringPanel = CreateMonitoringPanel();
            monitoringTab.Controls.Add(monitoringPanel);

            tabControl.TabPages.Add(playTab);
            tabControl.TabPages.Add(monitoringTab);

            // Status bar
            statusLabel = new Label
            {
                Text = "Ready",
                Dock = DockStyle.Bottom,
                Padding = new Padding(5),
                BackColor = DarkColors.StatusBar,
                ForeColor = DarkColors.Foreground
            };

            this.Controls.Add(tabControl);
            this.Controls.Add(statusLabel);
            this.FormClosing += MainForm_FormClosing;
        }

        private Panel CreatePlayPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = DarkColors.Background };
            hostGroup = panel; // Store reference

            // Host section group box
            var hostGroupBox = new GroupBox { Text = "Host Server && Play", Location = new Point(20, 20), Size = new Size(600, 180), BackColor = DarkColors.Background, ForeColor = DarkColors.Foreground };
            hostGroupBox.FlatStyle = FlatStyle.Flat;

            var hostLabel = new Label { Text = "Network Adapter:", Location = new Point(20, 35), Size = new Size(120, 25), ForeColor = DarkColors.Foreground };
            ipSelectorCombo = new ComboBox
            {
                Location = new Point(150, 32),
                Size = new Size(300, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = DarkColors.InputBackground,
                ForeColor = DarkColors.Foreground,
                FlatStyle = FlatStyle.Flat
            };
            ipSelectorCombo.SelectedIndexChanged += IpSelectorCombo_SelectedIndexChanged;

            var customIpLabel = new Label { Text = "Custom IP (overrides above):", Location = new Point(20, 75), Size = new Size(180, 25), ForeColor = DarkColors.Foreground };
            customIpBox = new TextBox
            {
                Location = new Point(200, 72),
                Size = new Size(150, 25),
                BackColor = DarkColors.InputBackground,
                ForeColor = DarkColors.Foreground,
                BorderStyle = BorderStyle.FixedSingle
            };
            customIpBox.TextChanged += (s, e) =>
            {
                if (launcherConfig != null)
                {
                    launcherConfig.CustomHostIP = customIpBox.Text;
                    SaveConfig();
                }

                // If custom IP is entered, visually indicate it overrides the dropdown
                if (!string.IsNullOrWhiteSpace(customIpBox.Text))
                    customIpBox.BackColor = DarkColors.HighlightBackground;
                else
                    customIpBox.BackColor = DarkColors.InputBackground;
            };

            var priorityLabel = new Label
            {
                Text = "Note: Custom IP takes priority if filled",
                Location = new Point(20, 110),
                Size = new Size(300, 20),
                ForeColor = DarkColors.MutedForeground,
                Font = new Font("Segoe UI", 8)
            };

            hostButton = new Button { Text = "Host && Play", Location = new Point(150, 140), Size = new Size(120, 30), BackColor = DarkColors.SuccessButton, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            hostButton.FlatAppearance.BorderSize = 0;
            hostButton.Click += HostButton_Click;

            stopServerButton = new Button { Text = "Stop Server", Location = new Point(290, 140), Size = new Size(100, 30), Enabled = false, BackColor = DarkColors.DangerButton, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            stopServerButton.FlatAppearance.BorderSize = 0;
            stopServerButton.Click += StopServerButton_Click;

            hostGroupBox.Controls.AddRange(new Control[] {
                hostLabel, ipSelectorCombo, customIpLabel, customIpBox, priorityLabel,
                hostButton, stopServerButton
            });

            // Join section group box
            var joinGroup = new GroupBox { Text = "Join Server && Play", Location = new Point(20, 220), Size = new Size(600, 180), BackColor = DarkColors.Background, ForeColor = DarkColors.Foreground };
            joinGroup.FlatStyle = FlatStyle.Flat;

            var joinLabel = new Label { Text = "Server IP:", Location = new Point(20, 35), Size = new Size(100, 25), ForeColor = DarkColors.Foreground };
            joinIpTextBox = new TextBox { Location = new Point(130, 32), Size = new Size(150, 25), BackColor = DarkColors.InputBackground, ForeColor = DarkColors.Foreground, BorderStyle = BorderStyle.FixedSingle };

            keepLauncherCheckbox = new CheckBox { Text = "Keep launcher open after join", Location = new Point(130, 65), Size = new Size(200, 25), ForeColor = DarkColors.Foreground, BackColor = DarkColors.Background };
            keepLauncherCheckbox.CheckedChanged += (s, e) => { if (launcherConfig != null) launcherConfig.KeepLauncherOpenOnJoin = keepLauncherCheckbox.Checked; };

            joinButton = new Button { Text = "Join && Play", Location = new Point(130, 95), Size = new Size(120, 30), BackColor = DarkColors.PrimaryButton, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            joinButton.FlatAppearance.BorderSize = 0;
            joinButton.Click += JoinButton_Click;

            joinGroup.Controls.AddRange(new Control[] { joinLabel, joinIpTextBox, keepLauncherCheckbox, joinButton });

            panel.Controls.Add(hostGroupBox);
            panel.Controls.Add(joinGroup);

            return panel;
        }

        private Panel CreateMonitoringPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = DarkColors.Background };

            logBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                ForeColor = DarkColors.TerminalGreen,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                WordWrap = false
            };

            var toolStrip = new Panel { Height = 40, Dock = DockStyle.Top, BackColor = DarkColors.Toolbar };

            saveLogButton = new Button { Text = "Save Log", Location = new Point(10, 8), Size = new Size(100, 25), BackColor = DarkColors.ButtonBackground, ForeColor = DarkColors.Foreground, FlatStyle = FlatStyle.Flat };
            saveLogButton.FlatAppearance.BorderSize = 0;
            saveLogButton.Click += SaveLogButton_Click;

            openWebUIButton = new Button { Text = "Open WebUI", Location = new Point(120, 8), Size = new Size(100, 25), BackColor = DarkColors.ButtonBackground, ForeColor = DarkColors.Foreground, FlatStyle = FlatStyle.Flat };
            openWebUIButton.FlatAppearance.BorderSize = 0;
            openWebUIButton.Click += (s, e) =>
            {
                string url = $"http://{currentServerIP}:5088";
                AppendLog($"[INFO] Opening WebUI: {url}\n");
                try
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    AppendLog($"[ERROR] Failed to open WebUI: {ex.Message}\n");
                    MessageBox.Show($"Could not open {url}\nMake sure the server is running and WebUI is enabled.",
                        "WebUI Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            var clearLogButton = new Button { Text = "Clear Log", Location = new Point(230, 8), Size = new Size(100, 25), BackColor = DarkColors.ButtonBackground, ForeColor = DarkColors.Foreground, FlatStyle = FlatStyle.Flat };
            clearLogButton.FlatAppearance.BorderSize = 0;
            clearLogButton.Click += (s, e) => logBox.Clear();

            toolStrip.Controls.AddRange(new Control[] { saveLogButton, openWebUIButton, clearLogButton });

            panel.Controls.Add(logBox);
            panel.Controls.Add(toolStrip);

            return panel;
        }

        private void LoadConfig()
        {
            launcherConfig = ConfigManager.LoadLauncherConfig();
        }

        private void SaveConfig()
        {
            if (launcherConfig != null)
                ConfigManager.SaveLauncherConfig(launcherConfig);
        }

        private void PopulateNetworkAdapters()
        {
            if (ipSelectorCombo == null) return;

            ipSelectorCombo.Items.Clear();

            var adapters = NetworkHelper.GetIPv4Adapters();
            foreach (var adapter in adapters)
            {
                ipSelectorCombo.Items.Add(adapter);
            }

            // Restore last selection
            if (!string.IsNullOrEmpty(launcherConfig?.LastHostIP))
            {
                foreach (var item in ipSelectorCombo.Items)
                {
                    string itemStr = item.ToString();
                    if (itemStr.Contains(launcherConfig.LastHostIP))
                    {
                        ipSelectorCombo.SelectedItem = item;
                        break;
                    }
                }
            }

            if (ipSelectorCombo.SelectedIndex == -1 && ipSelectorCombo.Items.Count > 0)
                ipSelectorCombo.SelectedIndex = 0;
        }

        private void IpSelectorCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ipSelectorCombo?.SelectedItem == null) return;

            string selected = ipSelectorCombo.SelectedItem.ToString();
            string extractedIp = null;

            if (selected.Contains("127.0.0.1"))
            {
                extractedIp = "127.0.0.1";
            }
            else if (selected.Contains("(") && selected.Contains(")"))
            {
                int start = selected.LastIndexOf('(') + 1;
                int end = selected.LastIndexOf(')');
                if (start > 0 && end > start)
                {
                    extractedIp = selected.Substring(start, end - start);
                }
            }

            if (extractedIp != null && launcherConfig != null)
            {
                launcherConfig.LastHostIP = extractedIp;
                SaveConfig();

                // ALWAYS update the custom IP box (overwrite whatever is there)
                if (customIpBox != null)
                {
                    customIpBox.Text = extractedIp;
                    // Reset to normal background since this came from dropdown
                    customIpBox.BackColor = DarkColors.InputBackground;
                }
            }
        }

        private void LoadPersistedSettings()
        {
            if (launcherConfig == null) return;

            if (joinIpTextBox != null)
                joinIpTextBox.Text = launcherConfig.LastJoinIP;

            if (keepLauncherCheckbox != null)
                keepLauncherCheckbox.Checked = launcherConfig.KeepLauncherOpenOnJoin;

            // Restore custom IP box
            if (customIpBox != null)
            {
                customIpBox.Text = launcherConfig.CustomHostIP ?? "";
                if (!string.IsNullOrWhiteSpace(launcherConfig.CustomHostIP))
                    customIpBox.BackColor = DarkColors.HighlightBackground;
            }
        }

        private async void HostButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Get custom IP directly from the field
                string customIp = customIpBox?.Text.Trim() ?? "";
                string ipToUse;

                // Priority 1: Custom IP if filled
                if (!string.IsNullOrWhiteSpace(customIp))
                {
                    ipToUse = customIp;
                    AppendLog($"[INFO] Using custom IP: {ipToUse}\n");
                }
                // Priority 2: Selected adapter's IP
                else if (!string.IsNullOrEmpty(launcherConfig?.LastHostIP))
                {
                    ipToUse = launcherConfig.LastHostIP;
                    AppendLog($"[INFO] Using adapter IP: {ipToUse}\n");
                }
                // Priority 3: Fallback to localhost
                else
                {
                    ipToUse = "127.0.0.1";
                    AppendLog($"[INFO] No IP selected, using localhost\n");
                }

                // Validate IP format (basic check)
                if (!System.Net.IPAddress.TryParse(ipToUse, out _))
                {
                    MessageBox.Show($"Invalid IP address: {ipToUse}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                currentServerIP = ipToUse;

                // Update configuration files
                ConfigManager.UpdateLuxonConfig(launcherConfig.LuxonConfigPath, ipToUse);
                ConfigManager.UpdateRedirectorConfig(launcherConfig.RedirectorConfigPath, ipToUse);

                // Start Luxon Server
                StartLuxonServer();

                // Launch game
                LaunchGameWithInjection();

                if (hostButton != null) hostButton.Enabled = false;
                if (joinButton != null) joinButton.Enabled = false;
                if (statusLabel != null) statusLabel.Text = $"Server running on {ipToUse} | Game launched";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Host Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendLog($"[ERROR] {ex.Message}\n");
            }
        }

        private void StartLuxonServer()
        {
            if (!File.Exists(launcherConfig.LuxonServerPath))
            {
                throw new FileNotFoundException($"LuxonServer not found at: {launcherConfig.LuxonServerPath}");
            }

            string workingDir = Path.GetDirectoryName(launcherConfig.LuxonServerPath);
            if (string.IsNullOrEmpty(workingDir))
                workingDir = Environment.CurrentDirectory;

            AppendLog($"[DEBUG] Working directory: {workingDir}\n");
            AppendLog($"[DEBUG] Config path expected: {Path.Combine(workingDir, "config.yml")}\n");

            var startInfo = new ProcessStartInfo
            {
                FileName = launcherConfig.LuxonServerPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDir,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };

            luxonProcess = new Process { StartInfo = startInfo };

            // Log any startup errors immediately
            luxonProcess.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    this.Invoke(new Action(() => AppendLog($"[STDERR] {e.Data}\n")));
            };

            try
            {
                luxonProcess.Start();
                luxonProcess.BeginErrorReadLine();

                // Start reading output
                _ = Task.Run(() => ReadOutputAsync(luxonProcess.StandardOutput));

                luxonProcess.EnableRaisingEvents = true;
                luxonProcess.Exited += (s, e) =>
                {
                    this.Invoke(new Action(() =>
                    {
                        int exitCode = luxonProcess.ExitCode;
                        AppendLog($"[INFO] Luxon Server process exited with code: {exitCode}\n");
                        if (exitCode != 0)
                            AppendLog($"[ERROR] Server crashed. Check config.yml format\n");

                        if (stopServerButton != null) stopServerButton.Enabled = false;
                        if (hostButton != null) hostButton.Enabled = true;
                        if (joinButton != null) joinButton.Enabled = true;
                        if (statusLabel != null) statusLabel.Text = $"Server stopped (exit code: {exitCode})";
                    }));
                };

                if (stopServerButton != null) stopServerButton.Enabled = true;
                AppendLog($"[INFO] Luxon Server started (PID: {luxonProcess.Id})\n");
            }
            catch (Exception ex)
            {
                AppendLog($"[ERROR] Failed to start process: {ex.Message}\n");
                throw;
            }
        }

        private async Task ReadOutputAsync(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                string line = await reader.ReadLineAsync();
                if (line != null)
                {
                    this.Invoke(new Action(() => AppendLog(line + "\n")));
                }
            }
        }

        private void LaunchGameWithInjection()
        {
            if (!launcherConfig.EnableDllInjection)
            {
                // Normal launch without injection
                if (!string.IsNullOrEmpty(launcherConfig.GameExecutablePath) && File.Exists(launcherConfig.GameExecutablePath))
                {
                    Process.Start(new ProcessStartInfo(launcherConfig.GameExecutablePath) { UseShellExecute = true });
                    AppendLog("[INFO] Game launched (injection disabled)\n");
                }
                return;
            }

            string dllPath = launcherConfig.InjectorDllPath;

            // Resolve relative path
            if (!Path.IsPathRooted(dllPath))
            {
                dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dllPath);
            }

            AppendLog($"[INFO] Checking for DLL at: {dllPath}\n");

            if (!File.Exists(dllPath))
            {
                AppendLog($"[WARNING] DLL not found at: {dllPath}. Launching without injection.\n");

                // Fallback to normal launch
                if (!string.IsNullOrEmpty(launcherConfig.GameExecutablePath) && File.Exists(launcherConfig.GameExecutablePath))
                {
                    Process.Start(new ProcessStartInfo(launcherConfig.GameExecutablePath) { UseShellExecute = true });
                }
                return;
            }

            try
            {
                AppendLog($"[INFO] Launching game: {launcherConfig.GameExecutablePath}\n");
                AppendLog($"[INFO] Preparing to inject: {Path.GetFileName(dllPath)}\n");

                // Launch the game
                var gameProcess = new Process();
                gameProcess.StartInfo.FileName = launcherConfig.GameExecutablePath;
                gameProcess.StartInfo.UseShellExecute = true;
                gameProcess.Start();

                int pid = gameProcess.Id;
                AppendLog($"[INFO] Game started with PID: {pid}\n");

                // Wait a moment for the process to fully initialize
                System.Threading.Thread.Sleep(1500);

                // Inject the DLL
                AppendLog("[INFO] Attempting DLL injection...\n");
                bool success = DllInjector.InjectIntoProcess(gameProcess, dllPath);

                if (success)
                {
                    AppendLog("[SUCCESS] DLL injected successfully!\n");
                }
                else
                {
                    AppendLog("[ERROR] DLL injection failed - see previous error\n");
                }
            }
            catch (Exception ex)
            {
                AppendLog($"[ERROR] Failed to inject: {ex.Message}\n");

                // The game process might still be running, let the user know
                AppendLog("[INFO] Game may still be running without the DLL\n");
            }
        }

        private void AppendLog(string text)
        {
            if (logBox != null)
            {
                logBox.AppendText(text);
                logBox.ScrollToCaret();
            }
        }

        private void StopServerButton_Click(object sender, EventArgs e)
        {
            if (luxonProcess != null && !luxonProcess.HasExited)
            {
                luxonProcess.Kill();
                luxonProcess.WaitForExit(5000);
                AppendLog("[INFO] Server stopped by user\n");
                if (statusLabel != null) statusLabel.Text = "Server stopped by user";
            }
            if (stopServerButton != null) stopServerButton.Enabled = false;
            if (hostButton != null) hostButton.Enabled = true;
            if (joinButton != null) joinButton.Enabled = true;
        }

        private async void JoinButton_Click(object sender, EventArgs e)
        {
            string ipToUse = joinIpTextBox?.Text.Trim() ?? "";
            if (string.IsNullOrEmpty(ipToUse))
            {
                MessageBox.Show("Please enter a server IP address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            currentServerIP = ipToUse;

            try
            {
                // Update redirector config only
                ConfigManager.UpdateRedirectorConfig(launcherConfig.RedirectorConfigPath, ipToUse);

                // Save last used IP
                launcherConfig.LastJoinIP = ipToUse;
                SaveConfig();

                // Launch game
                LaunchGameWithInjection();

                if (!launcherConfig.KeepLauncherOpenOnJoin)
                {
                    await Task.Delay(1000);
                    Application.Exit();
                }
                else
                {
                    if (statusLabel != null) statusLabel.Text = $"Game launched (connecting to {ipToUse})";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Join Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendLog($"[ERROR] {ex.Message}\n");
            }
        }

        private void SaveLogButton_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Text files|*.txt|Log files|*.log|All files|*.*";
                saveDialog.FileName = $"luxon_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveDialog.FileName, logBox?.Text ?? "");
                    AppendLog($"[INFO] Log saved to {saveDialog.FileName}\n");
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (luxonProcess != null && !luxonProcess.HasExited)
            {
                StopServerButton_Click(null, null);
            }
            SaveConfig();
        }
    }

    // Dark theme color definitions
    internal static class DarkColors
    {
        public static readonly Color Background = Color.FromArgb(30, 30, 35);
        public static readonly Color Foreground = Color.FromArgb(220, 220, 220);
        public static readonly Color MutedForeground = Color.FromArgb(150, 150, 150);
        public static readonly Color InputBackground = Color.FromArgb(45, 45, 50);
        public static readonly Color StatusBar = Color.FromArgb(25, 25, 28);
        public static readonly Color Toolbar = Color.FromArgb(40, 40, 45);
        public static readonly Color ButtonBackground = Color.FromArgb(60, 60, 65);
        public static readonly Color HighlightBackground = Color.FromArgb(80, 70, 40);
        public static readonly Color SuccessButton = Color.FromArgb(0, 100, 0);
        public static readonly Color DangerButton = Color.FromArgb(150, 50, 50);
        public static readonly Color PrimaryButton = Color.FromArgb(0, 80, 120);
        public static readonly Color TerminalGreen = Color.FromArgb(0, 255, 0);
    }
}