using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using Synapse_UI_WPF.Interfaces;
using Synapse_UI_WPF.Static;

namespace Synapse_UI_WPF
{
    public partial class ScriptHubWindow
    {
        private readonly MainWindow Main;
        private readonly Dictionary<string, ScriptEntry> DictData = new Dictionary<string, ScriptEntry>();
        private ScriptEntry CurrentEntry;
        private bool IsExecuting;
        private BackgroundWorker ExecuteWorker = new BackgroundWorker();

        private class ScriptEntry
        {
            public string Name;
            public string Description;
            public string Script;
            public string Picture;
        }

        public ScriptHubWindow(MainWindow _Main, Data.ScriptHubHolder _Data)
        {
            Main = _Main;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ExecuteWorker.DoWork += ExecuteWorker_DoWork;
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = WebInterface.RandomString(WebInterface.Rnd.Next(10, 32));

            var TScriptHub = Globals.Theme.ScriptHub;
            ThemeInterface.ApplyWindow(this, TScriptHub.Base);
            ThemeInterface.ApplyLogo(IconBox, TScriptHub.Logo);
            ThemeInterface.ApplySeperator(TopBox, TScriptHub.TopBox);
            ThemeInterface.ApplyLabel(TitleBox, TScriptHub.TitleBox);
            ThemeInterface.ApplyListBox(ScriptBox, TScriptHub.ScriptBox);
            ThemeInterface.ApplyTextBox(DescriptionBox, TScriptHub.DescriptionBox);
            ThemeInterface.ApplyButton(MiniButton, TScriptHub.MinimizeButton);
            ThemeInterface.ApplyButton(ExecuteButton, TScriptHub.ExecuteButton);
            ThemeInterface.ApplyButton(CloseButton, TScriptHub.CloseButton);

            ScriptBox.Items.Clear();
            ScriptBox.Items.Add("Loading scripts...");
            DescriptionBox.Text = "Fetching from ScriptBlox...";

            await LoadScriptBlox();
        }

        private async Task LoadScriptBlox()
        {
            try
            {
                using var http = new HttpClient();
                http.Timeout = TimeSpan.FromSeconds(15);
                http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 SynapseX");

                var entries = new List<ScriptEntry>();

                // Fetch 10 pages = up to 200 scripts
                for (int page = 1; page <= 10; page++)
                {
                    try
                    {
                        var url = $"https://scriptblox.com/api/script/fetch?page={page}&max=20&mode=free";
                        var json = await http.GetStringAsync(url);
                        var obj = JObject.Parse(json);
                        var scripts = obj["result"]?["scripts"] as JArray;
                        if (scripts == null || scripts.Count == 0) break;

                        foreach (var s in scripts)
                        {
                            try
                            {
                                var rawScript = s["script"]?.ToString() ?? "";
                                if (string.IsNullOrWhiteSpace(rawScript)) continue;

                                var title = s["title"]?.ToString() ?? "Untitled";
                                var gameName = s["game"]?["name"]?.ToString() ?? "Universal";
                                var views = s["views"]?.ToString() ?? "0";
                                var likes = s["likes"]?.ToString() ?? "0";
                                var img = s["image"]?.ToString() ?? "";

                                if (!string.IsNullOrEmpty(img) && !img.StartsWith("http"))
                                    img = "https://scriptblox.com" + img;

                                entries.Add(new ScriptEntry
                                {
                                    Name = title,
                                    Description = $"Game: {gameName}\nViews: {views}  |  Likes: {likes}\n\n{s["description"]?.ToString() ?? ""}".Trim(),
                                    Script = rawScript,
                                    Picture = img
                                });
                            }
                            catch { }
                        }

                        // Update UI with progress as pages load
                        var loaded = entries.Count;
                        Dispatcher.Invoke(() =>
                        {
                            ScriptBox.Items.Clear();
                            ScriptBox.Items.Add($"Loading... ({loaded} so far)");
                        });
                    }
                    catch { break; }
                }

                Dispatcher.Invoke(() =>
                {
                    ScriptBox.Items.Clear();
                    DictData.Clear();

                    foreach (var entry in entries)
                    {
                        var key = entry.Name;
                        int idx = 1;
                        while (DictData.ContainsKey(key))
                            key = $"{entry.Name} ({++idx})";

                        DictData[key] = entry;
                        ScriptBox.Items.Add(key);
                    }

                    if (ScriptBox.Items.Count == 0)
                    {
                        ScriptBox.Items.Add("No scripts found.");
                        DescriptionBox.Text = "Could not load any scripts.";
                    }
                    else
                    {
                        DescriptionBox.Text = $"Loaded {entries.Count} scripts. Select one to preview.";
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    ScriptBox.Items.Clear();
                    ScriptBox.Items.Add("Failed to load.");
                    DescriptionBox.Text = $"Could not connect to ScriptBlox:\n{ex.Message}";
                });
            }
        }

        private void ScriptBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ScriptBox.SelectedIndex == -1) return;
            var key = ScriptBox.Items[ScriptBox.SelectedIndex]?.ToString();
            if (key == null || !DictData.ContainsKey(key)) return;

            CurrentEntry = DictData[key];
            DescriptionBox.Text = CurrentEntry.Description;

            ScriptPictureBox.Source = null;
            if (!string.IsNullOrEmpty(CurrentEntry.Picture))
            {
                try
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(CurrentEntry.Picture);
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    img.EndInit();
                    ScriptPictureBox.Source = img;
                }
                catch { }
            }
        }

        public bool IsOpen() =>
            Dispatcher.Invoke(() => Application.Current.Windows.Cast<Window>().Any(x => x == this));

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsExecuting) return;
            if (CurrentEntry == null) return;

            if (!Main.Ready())
            {
                ExecuteButton.Content = "Not attached!";
                new Thread(() =>
                {
                    Thread.Sleep(1500);
                    if (!IsOpen()) return;
                    Dispatcher.Invoke(() =>
                        ExecuteButton.Content = Globals.Theme.ScriptHub.ExecuteButton.TextNormal);
                }).Start();
                return;
            }

            ExecuteButton.Content = Globals.Theme.ScriptHub.ExecuteButton.TextYield;
            IsExecuting = true;
            ExecuteWorker.RunWorkerAsync();
        }

        private void ExecuteWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var script = CurrentEntry?.Script ?? "";
            Dispatcher.Invoke(() =>
            {
                if (!IsOpen()) return;
                IsExecuting = false;
                ExecuteButton.Content = Globals.Theme.ScriptHub.ExecuteButton.TextNormal;
                if (!string.IsNullOrEmpty(script))
                    Main.Execute(script);
                else
                    MessageBox.Show("This script has no content.", "Synapse X",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
        private void MiniButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            => Main.ScriptHubOpen = false;
    }
}