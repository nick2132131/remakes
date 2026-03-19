using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using DiscordRPC;
using DiscordRPC.Logging;
using QuorumAPI;
using System.Drawing;
using System.Diagnostics;

namespace KRNL
{
    public partial class Form1 : Form
    {
        private readonly string scriptsPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts");

        private readonly string editorHtml =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "monaco_editor.html");

        private readonly System.Windows.Forms.Timer _fadeTimer =
            new System.Windows.Forms.Timer { Interval = 12 };
        private float _targetOpacity = 1f;

        private readonly System.Windows.Forms.Timer _injectionCheckTimer = 
            new System.Windows.Forms.Timer { Interval = 1000 };

        private DiscordRpcClient? _discord;
        private const string DiscordAppId = "1483002290913677433";

        private QuorumModule? quorum;
        private bool isInjected = false;

        private Size normalSize;
        private Point normalLocation;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        public Form1()
        {
            InitializeComponent();

            normalSize = this.Size;
            normalLocation = this.Location;

            Opacity = 0;
            _fadeTimer.Tick += FadeTick;
            Activated += (s, e) => BeginFade(1.00f);
            Deactivate += (s, e) => BeginFade(0.72f);

            _injectionCheckTimer.Tick += CheckInjectionTick;
            _injectionCheckTimer.Start();

            HookDrag(panel1);

            InitializeQuorum();

            Load += async (s, e) =>
            {
                InitDiscordRpc();
                EnsureScriptsFolder();
                LoadScriptList();
                await InitMonacoAsync();
                BeginFade(1.00f);
            };

            FormClosing += (s, e) =>
            {
                _discord?.ClearPresence();
                _discord?.Dispose();
                quorum?.StopCommunication();
            };
        }

        private void InitializeQuorum()
        {
            try
            {
                QuorumAPI.QuorumModule._AutoUpdateLogs = true;
                quorum = new QuorumModule();
                quorum.StartCommunication();
                QuorumAPI.QuorumModule.SetAttachNotify("KRNL Executor", "Successfully Injected!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize Quorum API: {ex.Message}\n\n" +
                    "Make sure QuorumAPI.dll and the bin folder are in the same directory as KRNL.exe", 
                    "KRNL Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HookDrag(Control ctrl)
        {
            if (ctrl == button6 || ctrl == button13)
                return;

            ctrl.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };
            foreach (Control child in ctrl.Controls)
                HookDrag(child);
        }

        private void BeginFade(float target)
        {
            _targetOpacity = target;
            _fadeTimer.Start();
        }

        private void FadeTick(object? sender, EventArgs e)
        {
            const float step = 0.06f;
            if (Math.Abs(Opacity - _targetOpacity) < step)
            {
                Opacity = _targetOpacity;
                _fadeTimer.Stop();
                return;
            }
            Opacity += (Opacity < _targetOpacity) ? step : -step;
        }

        private void InitDiscordRpc()
        {
            try
            {
                _discord = new DiscordRpcClient(DiscordAppId) { Logger = new NullLogger() };
                _discord.OnReady += (s, e) => { };
                _discord.Initialize();
                UpdateDiscordPresence("Ready");
            }
            catch { }
        }

        private void UpdateDiscordPresence(string state)
        {
            try
            {
                _discord?.SetPresence(new RichPresence
                {
                    Details = "Exploiting with KRNL",
                    State = state,
                    Timestamps = Timestamps.Now,
                    Assets = new Assets
                    {
                        LargeImageKey = "krnl_logo",
                        LargeImageText = "KRNL"
                    }
                });
            }
            catch { }
        }

        private async Task InitMonacoAsync()
        {
            try
            {
                await webView21.EnsureCoreWebView2Async(null);
                webView21.CoreWebView2.WebMessageReceived += OnWebMessage;

                if (File.Exists(editorHtml))
                    webView21.CoreWebView2.Navigate("file:///" + editorHtml.Replace("\\", "/"));
                else
                    MessageBox.Show("monaco_editor.html not found with .exe!", "KRNL",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("WebView2 is not installed.\n" + ex.Message, "KRNL",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<string> GetEditorContentAsync()
        {
            try
            {
                var json = await webView21.ExecuteScriptAsync("getEditorContent()");
                return JsonSerializer.Deserialize<string>(json) ?? "";
            }
            catch { return ""; }
        }

        private void SetEditorContent(string text)
        {
            var escaped = JsonSerializer.Serialize(text);
            _ = webView21.ExecuteScriptAsync($"setEditorContent({escaped})");
        }

        private void OnWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e) { }

        private void EnsureScriptsFolder()
        {
            if (!Directory.Exists(scriptsPath))
                Directory.CreateDirectory(scriptsPath);
        }

        private void LoadScriptList()
        {
            listBox2.Items.Clear();
            EnsureScriptsFolder();
            foreach (var ext in new[] { "*.lua", "*.txt" })
                foreach (var f in Directory.GetFiles(scriptsPath, ext))
                    listBox2.Items.Add(Path.GetFileName(f));
        }

        private async void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem == null) return;
            var filePath = Path.Combine(scriptsPath, listBox2.SelectedItem.ToString()!);
            if (!File.Exists(filePath)) return;
            await webView21.EnsureCoreWebView2Async(null);
            SetEditorContent(File.ReadAllText(filePath));
        }

        private void button13_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void panel1_DoubleClick(object sender, EventArgs e)
        {
            ToggleMaximize();
        }

        private void ToggleMaximize()
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                this.Size = normalSize;
                this.Location = normalLocation;
            }
            else
            {
                normalSize = this.Size;
                normalLocation = this.Location;
                
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var fileMenu = new ContextMenuStrip();
            fileMenu.Items.Add("Open Script...", null, (s, ev) => opn_Click(s!, ev!));
            fileMenu.Items.Add("Save Script...", null, (s, ev) => sve_Click(s!, ev!));
            fileMenu.Items.Add(new ToolStripSeparator());
            fileMenu.Items.Add("Open Scripts Folder", null, (s, ev) => OpenScriptsFolder());
            fileMenu.Items.Add("Reload Script List", null, (s, ev) => LoadScriptList());
            fileMenu.Items.Add(new ToolStripSeparator());
            fileMenu.Items.Add("Exit", null, (s, ev) => Application.Exit());
            
            fileMenu.Show(button1, new Point(0, button1.Height));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var creditsForm = new Form
            {
                Text = "Credits",
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(33, 33, 33)
            };

            var creditsText = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(33, 33, 33),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Font = new Font("Segoe UI", 10),
                Text = @"KRNL

made by rayquaz8601

uses quorum for injection, monaco for the editor"
            };

            creditsForm.Controls.Add(creditsText);
            creditsForm.ShowDialog(this);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var gamesMenu = new ContextMenuStrip();
            gamesMenu.Items.Add("Universal Scripts", null, (s, ev) => LoadUniversalScripts());
            gamesMenu.Items.Add(new ToolStripSeparator());
            gamesMenu.Items.Add("Blox Fruits", null, (s, ev) => LoadGameScript("Blox Fruits"));
            gamesMenu.Items.Add("Arsenal", null, (s, ev) => LoadGameScript("Arsenal"));
            gamesMenu.Items.Add("Phantom Forces", null, (s, ev) => LoadGameScript("Phantom Forces"));
            gamesMenu.Items.Add("Murder Mystery 2", null, (s, ev) => LoadGameScript("Murder Mystery 2"));
            gamesMenu.Items.Add("Jailbreak", null, (s, ev) => LoadGameScript("Jailbreak"));
            gamesMenu.Items.Add("Adopt Me", null, (s, ev) => LoadGameScript("Adopt Me"));
            gamesMenu.Items.Add(new ToolStripSeparator());
            gamesMenu.Items.Add("Search Game...", null, (s, ev) => SearchGame());
            
            gamesMenu.Show(button4, new Point(0, button4.Height));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var hotMenu = new ContextMenuStrip();
            hotMenu.Items.Add("Infinite Yield", null, (s, ev) => LoadHotScript("IY"));
            hotMenu.Items.Add("Dark Dex", null, (s, ev) => LoadHotScript("DarkDex"));
            hotMenu.Items.Add("Remote Spy", null, (s, ev) => LoadHotScript("RemoteSpy"));
            hotMenu.Items.Add("Unnamed ESP", null, (s, ev) => LoadHotScript("ESP"));
            hotMenu.Items.Add(new ToolStripSeparator());
            hotMenu.Items.Add("Owl Hub", null, (s, ev) => LoadHotScript("OwlHub"));
            hotMenu.Items.Add("V.G Hub", null, (s, ev) => LoadHotScript("VGHub"));
            hotMenu.Items.Add(new ToolStripSeparator());
            hotMenu.Items.Add("Script Hub (scriptblox)", null, (s, ev) => OpenScriptHub());
            
            hotMenu.Show(button3, new Point(0, button3.Height));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var othersMenu = new ContextMenuStrip();
            othersMenu.Items.Add("Settings", null, (s, ev) => ShowSettings());

            var autoAttachItem = new ToolStripMenuItem("Auto-Attach");
            autoAttachItem.CheckOnClick = true;
            autoAttachItem.Click += (s, ev) => ToggleAutoAttach();
            othersMenu.Items.Add(autoAttachItem);

            othersMenu.Items.Add(new ToolStripSeparator());
            othersMenu.Items.Add("Kill Roblox", null, (s, ev) => KillRoblox());
            othersMenu.Items.Add("Restart KRNL", null, (s, ev) => RestartApp());
            othersMenu.Items.Add(new ToolStripSeparator());
            othersMenu.Items.Add("Scripts Folder", null, (s, ev) => OpenScriptsFolder());
            othersMenu.Items.Add("Credits", null, (s, ev) => button2_Click(s!, ev));
            
            othersMenu.Show(button5, new Point(0, button5.Height));
        }

        private void LoadUniversalScripts()
        {
            var scriptsForm = new Form
            {
                Text = "Universal Scripts",
                Size = new Size(500, 400),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(33, 33, 33),
                ForeColor = Color.White
            };

            var listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(29, 29, 29),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            listBox.Items.Add("Infinite Yield - Admin Commands");
            listBox.Items.Add("Dark Dex - Explorer");
            listBox.Items.Add("Remote Spy - Network Monitor");
            listBox.Items.Add("Unnamed ESP - Player ESP");

            listBox.DoubleClick += (s, ev) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    var scripts = new[] { "IY", "DarkDex", "RemoteSpy", "ESP" };
                    LoadHotScript(scripts[listBox.SelectedIndex]);
                    scriptsForm.Close();
                }
            };

            scriptsForm.Controls.Add(listBox);
            scriptsForm.ShowDialog(this);
        }

        private void LoadGameScript(string gameName)
        {
            var gameScripts = new Dictionary<string, string>
            {
                ["Blox Fruits"] = "-- Blox Fruits Script\nloadstring(game:HttpGet('https://raw.githubusercontent.com/scripthubc/scripts/main/bloxfruits.lua'))()",
                ["Arsenal"] = "-- Arsenal Script\nloadstring(game:HttpGet('https://raw.githubusercontent.com/tbao143/thaibao/main/TbaoHubArsenal'))()",
                ["Phantom Forces"] = "-- Phantom Forces ESP\nloadstring(game:HttpGet('https://raw.githubusercontent.com/Exunys/Phantom-Forces-ESP/main/main.lua'))()",
                ["Murder Mystery 2"] = "-- MM2 Script\nloadstring(game:HttpGet('https://raw.githubusercontent.com/Ethanoj1/Eclipse-Hub/master/script'))()",
                ["Jailbreak"] = "-- Jailbreak Script\nloadstring(game:HttpGet('https://raw.githubusercontent.com/ThatGuyYouKnow1/Vynixu-Jailbreak/main/Main'))()",
                ["Adopt Me"] = "-- Adopt Me Script\nloadstring(game:HttpGet('https://raw.githubusercontent.com/tbao143/thaibao/main/TbaoHubAdoptMe'))()"
            };

            if (gameScripts.ContainsKey(gameName))
            {
                SetEditorContent(gameScripts[gameName]);
                
            }
            else
            {
                MessageBox.Show($"Script for {gameName} not found.", "KRNL",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SearchGame()
        {
            var input = new Form
            {
                Text = "Search Game",
                Size = new Size(350, 120),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(33, 33, 33)
            };

            var label = new Label
            {
                Text = "Enter game name:",
                ForeColor = Color.White,
                Location = new Point(10, 10),
                AutoSize = true
            };

            var textBox = new TextBox
            {
                Location = new Point(10, 35),
                Width = 310,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var searchBtn = new System.Windows.Forms.Button
            {
                Text = "Search",
                Location = new Point(230, 65),
                BackColor = Color.FromArgb(139, 92, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            searchBtn.Click += (s, ev) =>
            {
                MessageBox.Show($"Searching scripts for: {textBox.Text}\n\nFeature coming soon!", "KRNL",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                input.Close();
            };

            input.Controls.AddRange(new Control[] { label, textBox, searchBtn });
            input.ShowDialog(this);
        }

        private void LoadHotScript(string scriptName)
        {
            var scripts = new Dictionary<string, string>
            {
                ["IY"] = "-- Infinite Yield\nloadstring(game:HttpGet('https://raw.githubusercontent.com/EdgeIY/infiniteyield/master/source'))()",
                ["DarkDex"] = "-- Dark Dex\nloadstring(game:HttpGet('https://raw.githubusercontent.com/Babyhamsta/RBLX_Scripts/main/Universal/BypassedDarkDexV3.lua', true))()",
                ["RemoteSpy"] = "-- Remote Spy\nloadstring(game:HttpGet('https://raw.githubusercontent.com/exxtremestuffs/SimpleSpySource/master/SimpleSpy.lua'))()",
                ["ESP"] = "-- Unnamed ESP\nloadstring(game:HttpGet('https://raw.githubusercontent.com/ic3w0lf22/Unnamed-ESP/master/UnnamedESP.lua'))()",
                ["OwlHub"] = "-- Owl Hub\nloadstring(game:HttpGet('https://raw.githubusercontent.com/CriShoux/OwlHub/master/OwlHub.txt'))()",
                ["VGHub"] = "-- V.G Hub\nloadstring(game:HttpGet('https://raw.githubusercontent.com/1201for/V.G-Hub/main/V.Ghub'))()"
            };

            if (scripts.ContainsKey(scriptName))
            {
                SetEditorContent(scripts[scriptName]);
            }
        }

        private void OpenScriptHub()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://scriptblox.com",
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show("Failed to open browser", "KRNL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowSettings()
        {
            var settingsForm = new Form
            {
                Text = "Settings",
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(33, 33, 33)
            };

            var autoAttachCheck = new CheckBox
            {
                Text = "Auto-Attach",
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true
            };

            var autoExecCheck = new CheckBox
            {
                Text = "Auto-Execute",
                ForeColor = Color.White,
                Location = new Point(20, 50),
                AutoSize = true
            };

            var saveBtn = new System.Windows.Forms.Button
            {
                Text = "Save Settings",
                Location = new Point(150, 220),
                BackColor = Color.FromArgb(139, 92, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 30)
            };

            saveBtn.Click += (s, ev) =>
            {
                if (autoAttachCheck.Checked)
                    quorum?.SetAutoAttach(true);
                MessageBox.Show("Settings saved!", "KRNL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                settingsForm.Close();
            };

            settingsForm.Controls.AddRange(new Control[] { autoAttachCheck, autoExecCheck, saveBtn });
            settingsForm.ShowDialog(this);
        }

        private void ToggleAutoAttach()
        {
            try
            {
                var currentState = quorum?.IsAttached() ?? false;
                quorum?.SetAutoAttach(!currentState);
                MessageBox.Show("Auto-attach toggled", "KRNL", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "KRNL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void KillRoblox()
        {
            try
            {
                QuorumAPI.QuorumModule.KillRoblox();
                isInjected = false;
                inj.Text = "INJECT";
                inj.BackColor = Color.FromArgb(40, 40, 40);
                inj.Enabled = true;
                MessageBox.Show("Roblox has been terminated", "KRNL",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateDiscordPresence("Ready");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to kill Roblox: {ex.Message}", "KRNL Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RestartApp()
        {
            Application.Restart();
        }

        private void OpenScriptsFolder()
        {
            try
            {
                EnsureScriptsFolder();
                Process.Start(new ProcessStartInfo
                {
                    FileName = scriptsPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open folder: {ex.Message}", "KRNL",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void clr_Click(object sender, EventArgs e) => SetEditorContent("");

        private async void opn_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Lua Scripts (*.lua)|*.lua|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                InitialDirectory = Directory.Exists(scriptsPath) ? scriptsPath : "",
                Title = "Open Script"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                await webView21.EnsureCoreWebView2Async(null);
                SetEditorContent(File.ReadAllText(ofd.FileName));
            }
        }

        private async void sve_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "Lua Scripts (*.lua)|*.lua|Text Files (*.txt)|*.txt",
                InitialDirectory = Directory.Exists(scriptsPath) ? scriptsPath : "",
                DefaultExt = "lua",
                Title = "Save Script"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, await GetEditorContentAsync());
                LoadScriptList();
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void flowLayoutPanel2_Paint(object sender, PaintEventArgs e) { }

        private async void exec_Click(object sender, EventArgs e)
        {
            try
            {
                if (!isInjected)
                {
                    MessageBox.Show("Please inject first!", "KRNL",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string script = await GetEditorContentAsync();

                if (string.IsNullOrWhiteSpace(script))
                {
                    MessageBox.Show("Script is empty!", "KRNL",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                exec.Enabled = false;
                exec.Text = "EXECUTING...";

                quorum!.ExecuteScript(script);

                exec.Enabled = true;
                exec.Text = "EXECUTE";

                UpdateDiscordPresence("Executing scripts");
            }
            catch (Exception ex)
            {
                exec.Enabled = true;
                exec.Text = "EXECUTE";
                MessageBox.Show($"Execution failed: {ex.Message}", "KRNL Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private readonly string SpoofScript =
            "getgenv().identifyexecutor = function() return 'KRNL', '1.0.0' end; " +
            "getgenv().getexecutorname = getgenv().identifyexecutor;";

        private void CheckInjectionTick(object? sender, EventArgs e)
        {
            if (quorum == null) return;

            bool currentlyAttached = quorum.IsAttached();
            if (currentlyAttached == isInjected) return;

            isInjected = currentlyAttached;

            if (isInjected)
            {
                inj.Text = "INJECTED";
                inj.Enabled = false;
                UpdateDiscordPresence("Injected");
                quorum.ExecuteScript(SpoofScript);
            }
            else
            {
                inj.Text = "INJECT";
                inj.Enabled = true;
                UpdateDiscordPresence("Ready");
            }
        }

        private async void inj_Click(object sender, EventArgs e)
        {
            if (isInjected || quorum == null) return;

            try
            {
                inj.Enabled = false;
                inj.Text = "INJECTING...";
                await quorum.AttachAPI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Injection failed: {ex.Message}",
                    "KRNL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                inj.Enabled = true;
                inj.Text = "INJECT";
            }
        }
    }
}