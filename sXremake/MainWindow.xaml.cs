using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using QuorumAPI;
using DiscordRPC;
using DiscordRPC.Logging;
using Synapse_UI_WPF.Controls;
using Synapse_UI_WPF.Interfaces;
using Synapse_UI_WPF.Static;

namespace Synapse_UI_WPF
{
    public partial class MainWindow
    {
        public delegate void InteractMessageEventHandler(object sender, string Input);
        public event InteractMessageEventHandler InteractMessageRecieved;

        public bool OptionsOpen;
        public bool ScriptHubOpen;
        private bool ScriptHubInit;
        public bool IsInlineUpdating;

        private static ThemeInterface.TAttachStrings AttachStrings;
        private readonly string BaseDirectory;
        private readonly string ScriptsDirectory;

        public static BackgroundWorker Worker = new BackgroundWorker();
        public static BackgroundWorker HubWorker = new BackgroundWorker();

        private Monaco Browser;
        private QuorumModule _quorum;
        private bool _isAttached = false;
        private System.Windows.Threading.DispatcherTimer _attachCheckTimer;
        private DiscordRpcClient _discord;
        private const string DiscordAppId = "1484139900415508611";

        private readonly string SpoofScript =
            "getgenv().identifyexecutor = function() return 'Synapse X', '1.0' end; " +
            "getgenv().getexecutorname = getgenv().identifyexecutor;";

        public MainWindow()
        {
            InitializeComponent();

            Browser = new Monaco();
            Browser.MonacoReady += Browser_MonacoReady;
            BrowserHost.Content = Browser;

            _quorum = new QuorumModule();
            _quorum.StartCommunication();
            QuorumAPI.QuorumModule.SetAttachNotify("Synapse X", "Successfully Attached!");

            _attachCheckTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _attachCheckTimer.Tick += (s, ev) =>
            {
                if (_quorum == null) return;
                bool attached = _quorum.IsAttached();
                if (attached == _isAttached) return;
                _isAttached = attached;
                if (_isAttached)
                {
                    SetTitle(AttachStrings.Ready, 3000);
                    _quorum.ExecuteScript(SpoofScript);
                    UpdateDiscordPresence("Injected");
                }
                else
                {
                    SetTitle(AttachStrings.NotInjected, 0);
                    UpdateDiscordPresence("Idle");
                }
            };
            _attachCheckTimer.Start();

            InitDiscordRpc();

            Worker.DoWork += Worker_DoWork;
            HubWorker.DoWork += HubWorker_DoWork;

            var TMain = Globals.Theme.Main;
            ThemeInterface.ApplyWindow(this, TMain.Base);
            ThemeInterface.ApplyLogo(IconBox, TMain.Logo);
            ThemeInterface.ApplyFormatLabel(TitleBox, TMain.TitleBox, Globals.Version);
            ThemeInterface.ApplyListBox(ScriptBox, TMain.ScriptBox);
            ThemeInterface.ApplyButton(MiniButton, TMain.MinimizeButton);
            ThemeInterface.ApplyButton(CloseButton, TMain.ExitButton);
            ThemeInterface.ApplyButton(ExecuteButton, TMain.ExecuteButton);
            ThemeInterface.ApplyButton(ClearButton, TMain.ClearButton);
            ThemeInterface.ApplyButton(OpenFileButton, TMain.OpenFileButton);
            ThemeInterface.ApplyButton(ExecuteFileButton, TMain.ExecuteFileButton);
            ThemeInterface.ApplyButton(SaveFileButton, TMain.SaveFileButton);
            ThemeInterface.ApplyButton(OptionsButton, TMain.OptionsButton);
            ThemeInterface.ApplyButton(AttachButton, TMain.AttachButton);
            ThemeInterface.ApplyButton(ScriptHubButton, TMain.ScriptHubButton);

            ScaleTransform.ScaleX = Globals.Options.WindowScale;
            ScaleTransform.ScaleY = Globals.Options.WindowScale;

            AttachStrings = TMain.BaseStrings;

            BaseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(15000);
                    try
                    {
                        string txt = "";
                        Dispatcher.Invoke(() => { txt = Browser.GetText(); });
                        DataInterface.Save("savedws", txt);
                    }
                    catch { }
                }
            })
            { IsBackground = true }.Start();

            ScriptsDirectory = Path.Combine(BaseDirectory, "scripts");
            if (!Directory.Exists(ScriptsDirectory)) Directory.CreateDirectory(ScriptsDirectory);
            RefreshScriptBox();

            ScriptBox.MouseDoubleClick += (s, ev) =>
            {
                if (ScriptBox.SelectedIndex == -1) return;
                try
                {
                    var file = Path.Combine(ScriptsDirectory, ScriptBox.Items[ScriptBox.SelectedIndex].ToString());
                    Browser.SetText(File.ReadAllText(file));
                }
                catch { }
            };

            if (TMain.WebSocket.Enabled)
                WebSocketInterface.Start(24892, this);
        }

        public void SetTitle(string Str, int Delay = 0)
        {
            Dispatcher.Invoke(() =>
            {
                TitleBox.Content = ThemeInterface.ConvertFormatString(
                    Globals.Theme.Main.TitleBox, Globals.Version) + Str;
            });
            if (Delay == 0) return;
            new Thread(() =>
            {
                Thread.Sleep(Delay);
                Dispatcher.Invoke(() =>
                {
                    TitleBox.Content = ThemeInterface.ConvertFormatString(
                        Globals.Theme.Main.TitleBox, Globals.Version);
                });
            })
            { IsBackground = true }.Start();
        }

        public bool Ready() => _isAttached;

        public void Execute(string data)
        {
            if (string.IsNullOrEmpty(data)) return;
            if (!Ready()) { SetTitle(AttachStrings.NotInjected, 3000); return; }
            _quorum.ExecuteScript(data);
            UpdateDiscordPresence("Executing scripts");

            new Thread(() => {
                Thread.Sleep(5000);
                if (_isAttached) Dispatcher.Invoke(() => UpdateDiscordPresence("Injected"));
            })
            { IsBackground = true }.Start();
        }

        private void InitDiscordRpc()
        {
            try
            {
                _discord = new DiscordRpcClient(DiscordAppId) { Logger = new NullLogger() };
                _discord.Initialize();
                UpdateDiscordPresence("Idle");
            }
            catch { }
        }

        private void UpdateDiscordPresence(string state)
        {
            try
            {
                _discord?.SetPresence(new RichPresence
                {
                    Details = "synapse X remake",
                    State = state,
                    Timestamps = Timestamps.Now,
                    Assets = new Assets
                    {
                        LargeImageKey = "synapse_logo",
                        LargeImageText = "Synapse X",
                    }
                });
            }
            catch { }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = WebInterface.RandomString(WebInterface.Rnd.Next(10, 32));
            LoadLogo();
        }

        private void LoadLogo()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var names = new[] { "sxlogosmallwhite_OJJ_icon.ico", "icon.ico", "logo.ico", "logo.png", "logo.jpg" };
            for (int i = 0; i < 6 && dir != null; i++)
            {
                foreach (var name in names)
                {
                    var path = Path.Combine(dir, name);
                    if (!File.Exists(path)) continue;
                    try
                    {
                        var img = new System.Windows.Media.Imaging.BitmapImage();
                        img.BeginInit();
                        img.UriSource = new Uri(path);
                        img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        img.EndInit();
                        IconBox.Source = img;
                        return;
                    }
                    catch { }
                }
                dir = Path.GetDirectoryName(dir);
            }
        }

        private void Browser_MonacoReady()
        {
            Browser.SetTheme(Globals.Theme.Main.Editor.Light ? MonacoTheme.Light : MonacoTheme.Dark);
            string saved = "";
            try { if (DataInterface.Exists("savedws")) saved = DataInterface.Read<string>("savedws"); }
            catch { DataInterface.Save("savedws", ""); }
            if (!string.IsNullOrEmpty(saved))
                Browser.SetText(saved);
        }

        public void Attach() { if (!Worker.IsBusy) Worker.RunWorkerAsync(); }
        public void SetEditor(string t) => Browser.SetText(t);

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.Theme.Main.WebSocket.Enabled) WebSocketInterface.Stop();
            DataInterface.Save("savedws", Browser.GetText());
            _attachCheckTimer?.Stop();
            _discord?.ClearPresence();
            _discord?.Dispose();
            _quorum?.StopCommunication();
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _quorum?.StopCommunication();
            Environment.Exit(0);
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (OptionsOpen) return;
            new OptionsWindow(this).Show();
        }

        private void MiniButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void ClearButton_Click(object sender, RoutedEventArgs e) => Browser.SetText("");

        private void IconBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => MessageBox.Show(
                "Synapse X was developed by 3dsboy08, brack4712, Louka, DefCon42, and Eternal.\r\n\r\nAdditional credits:\r\n    - Rain: Emotional support and testing",
                "Synapse X Credits", MessageBoxButton.OK, MessageBoxImage.Information);

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var d = new OpenFileDialog { Filter = "Script Files (*.lua,*.txt)|*.lua;*.txt", FileName = "" };
            if (d.ShowDialog() != true) return;
            try { Browser.SetText(File.ReadAllText(d.FileName)); } catch { }
        }

        private void ExecuteFileButton_Click(object sender, RoutedEventArgs e)
        {
            var d = new OpenFileDialog { Filter = "Script Files (*.lua,*.txt)|*.lua;*.txt", FileName = "" };
            if (d.ShowDialog() != true) return;
            try { Execute(File.ReadAllText(d.FileName)); }
            catch { MessageBox.Show("Failed to read file.", "Synapse X", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            var d = new SaveFileDialog { Filter = "Script Files (*.lua,*.txt)|*.lua;*.txt", FileName = "" };
            d.FileOk += (o, a) => File.WriteAllText(d.FileName, Browser.GetText());
            d.ShowDialog();
        }

        private async void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isAttached) return;
            SetTitle(AttachStrings.Checking);
            try
            {
                await _quorum.AttachAPI();
            }
            catch (Exception ex)
            {
                SetTitle(AttachStrings.FailedToFindRoblox, 3000);
            }
        }

        private void ScriptHubButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScriptHubOpen || ScriptHubInit) return;
            ScriptHubOpen = ScriptHubInit = true;
            ScriptHubButton.Content = Globals.Theme.Main.ScriptHubButton.TextYield;
            HubWorker.RunWorkerAsync();
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAttached)
            {
                SetTitle(AttachStrings.NotInjected, 3000);
                return;
            }
            var script = Browser.GetText();
            if (string.IsNullOrWhiteSpace(script)) return;
            _quorum.ExecuteScript(script);
            UpdateDiscordPresence("Executing scripts");
            new Thread(() => {
                Thread.Sleep(5000);
                if (_isAttached) Dispatcher.Invoke(() => UpdateDiscordPresence("Injected"));
            })
            { IsBackground = true }.Start();
        }

        private void ExecuteItem_Click(object sender, RoutedEventArgs e)
        {
            if (ScriptBox.SelectedIndex == -1) return;
            try { Execute(File.ReadAllText(Path.Combine(ScriptsDirectory, ScriptBox.Items[ScriptBox.SelectedIndex].ToString()))); }
            catch { MessageBox.Show("Failed to read file.", "Synapse X", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }

        private void LoadItem_Click(object sender, RoutedEventArgs e)
        {
            if (ScriptBox.SelectedIndex == -1) return;
            try { Browser.SetText(File.ReadAllText(Path.Combine(ScriptsDirectory, ScriptBox.Items[ScriptBox.SelectedIndex].ToString()))); }
            catch { MessageBox.Show("Failed to read file.", "Synapse X", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }

        private void RefreshItem_Click(object sender, RoutedEventArgs e) => RefreshScriptBox();

        private void RefreshScriptBox()
        {
            ScriptBox.Items.Clear();
            if (!Directory.Exists(ScriptsDirectory)) return;
            foreach (var f in Directory.GetFiles(ScriptsDirectory, "*.txt")
                .Concat(Directory.GetFiles(ScriptsDirectory, "*.lua"))
                .OrderBy(f => Path.GetFileName(f)))
            {
                ScriptBox.Items.Add(Path.GetFileName(f));
            }
        }

        private void HubWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var stub = new Data.ScriptHubHolder { Entries = new List<Data.ScriptHubEntry>() };
            Dispatcher.Invoke(() =>
            {
                ScriptHubInit = false;
                ScriptHubButton.Content = Globals.Theme.Main.ScriptHubButton.TextNormal;
                new ScriptHubWindow(this, stub).Show();
            });
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e) { }
    }
}