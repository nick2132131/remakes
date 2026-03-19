#define USE_UPDATE_CHECKS
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;
using Synapse_UI_WPF.Interfaces;
using Synapse_UI_WPF.Static;

namespace Synapse_UI_WPF
{
    public partial class LoadWindow
    {
        public const string UiVersion = "14";
        public const uint TVersion = 2;

        public static ThemeInterface.TInitStrings InitStrings;
        public static BackgroundWorker LoadWorker = new BackgroundWorker();

        public LoadWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var Result = MessageBox.Show(
                    $"Synapse has encountered an exception. Please report the following text below to 3dsboy08 on Discord (make sure to give the text, not an image):\n\n{((Exception)args.ExceptionObject)}\n\nIf you would like this text copied to your clipboard, press 'Yes'.",
                    "Synapse X",
                    MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No);

                if (Result != MessageBoxResult.Yes) return;

                var STAThread = new Thread(delegate ()
                {
                    Clipboard.SetText(((Exception)args.ExceptionObject).ToString());
                });
                STAThread.SetApartmentState(ApartmentState.STA);
                STAThread.Start();
                STAThread.Join();
                Thread.Sleep(1000);
            };

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            LoadWorker.DoWork += LoadWorker_DoWork;
            InitializeComponent();
        }

        public void SetStatusText(string Status, int Percentage)
        {
            Dispatcher.Invoke(() =>
            {
                StatusBox.Content = Status;
                ProgressBox.Value = Percentage;
            });
        }

        private ThemeInterface.TBase MigrateT1ToT2(ThemeInterface.TBase Old)
        {
            Old.Version = 2;
            Old.Main.ExecuteFileButton = new ThemeInterface.TButton
            {
                BackColor = new ThemeInterface.TColor(255, 60, 60, 60),
                TextColor = new ThemeInterface.TColor(255, 255, 255, 255),
                Font = new ThemeInterface.TFont("Segoe UI", 14f),
                Image = new ThemeInterface.TImage(),
                Text = "Execute File"
            };
            return Old;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Kill duplicate instances
            var ProcList = Process.GetProcessesByName(
                Path.GetFileName(AppDomain.CurrentDomain.FriendlyName));
            var Current = Process.GetCurrentProcess();
            foreach (var Proc in ProcList)
            {
                if (Proc.Id == Current.Id) continue;
                try { Proc.Kill(); }
                catch (Exception) { }
            }

            // Load or create theme
            if (!Directory.Exists("bin")) Directory.CreateDirectory("bin");
            if (!Directory.Exists("auth")) Directory.CreateDirectory("auth");
            if (!Directory.Exists("scripts")) Directory.CreateDirectory("scripts");
            if (!Directory.Exists("workspace")) Directory.CreateDirectory("workspace");

            if (!File.Exists("bin\\theme-wpf.json"))
            {
                File.WriteAllText("bin\\theme-wpf.json",
                    JsonConvert.SerializeObject(ThemeInterface.Default(), Formatting.Indented));
            }

            try
            {
                Globals.Theme =
                    JsonConvert.DeserializeObject<ThemeInterface.TBase>(File.ReadAllText("bin\\theme-wpf.json"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to parse theme.json file.\n\nException details:\n" + ex.Message,
                    "Synapse X Theme Parser", MessageBoxButton.OK, MessageBoxImage.Error);
                Globals.Theme = ThemeInterface.Default();
            }

            if (Globals.Theme.Version != TVersion)
            {
                if (Globals.Theme.Version == 1)
                    Globals.Theme = MigrateT1ToT2(Globals.Theme);

                File.WriteAllText("bin\\theme-wpf.json",
                    JsonConvert.SerializeObject(Globals.Theme, Formatting.Indented));
            }

            // Load or create options
            if (!DataInterface.Exists("options"))
            {
                Globals.Options = new Data.Options
                {
                    AutoLaunch = false,
                    AutoAttach = false,
                    MultiRoblox = false,
                    UnlockFPS = false,
                    IngameChat = false,
                    BetaRelease = false,
                    InternalUI = false,
                    WindowScale = 1d
                };
                DataInterface.Save("options", new Data.OptionsHolder
                {
                    Version = Data.OptionsVersion,
                    Data = JsonConvert.SerializeObject(Globals.Options)
                });
            }
            else
            {
                try
                {
                    var Read = DataInterface.Read<Data.OptionsHolder>("options");
                    if (Read.Version != Data.OptionsVersion)
                    {
                        Globals.Options = new Data.Options
                        {
                            AutoLaunch = false,
                            AutoAttach = false,
                            MultiRoblox = false,
                            UnlockFPS = false,
                            IngameChat = false,
                            InternalUI = false,
                            BetaRelease = false,
                            WindowScale = 1d
                        };
                        DataInterface.Save("options", new Data.OptionsHolder
                        {
                            Version = Data.OptionsVersion,
                            Data = JsonConvert.SerializeObject(Globals.Options)
                        });
                    }
                    else
                    {
                        Globals.Options = JsonConvert.DeserializeObject<Data.Options>(Read.Data);
                    }
                }
                catch (Exception)
                {
                    Globals.Options = new Data.Options
                    {
                        AutoLaunch = false,
                        AutoAttach = false,
                        MultiRoblox = false,
                        UnlockFPS = false,
                        IngameChat = false,
                        InternalUI = false,
                        BetaRelease = false,
                        WindowScale = 1d
                    };
                    DataInterface.Save("options", new Data.OptionsHolder
                    {
                        Version = Data.OptionsVersion,
                        Data = JsonConvert.SerializeObject(Globals.Options)
                    });
                }
            }

            // Apply theme to load window
            var TLoad = Globals.Theme.Load;
            ThemeInterface.ApplyWindow(this, TLoad.Base);
            ThemeInterface.ApplyLogo(IconBox, TLoad.Logo);
            ThemeInterface.ApplyLabel(TitleBox, TLoad.TitleBox);
            ThemeInterface.ApplyLabel(StatusBox, TLoad.StatusBox);
            ThemeInterface.ApplySeperator(TopBox, TLoad.TopBox);
            InitStrings = TLoad.BaseStrings;

            Title = WebInterface.RandomString(WebInterface.Rnd.Next(10, 32));
            Globals.Context = SynchronizationContext.Current;

            LoadWorker.RunWorkerAsync();
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void LoadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Check for a local login token; if absent, show login window
            if (!DataInterface.Exists("token"))
            {
                Dispatcher.Invoke(() =>
                {
                    var LoginUI = new LoginWindow();
                    LoginUI.Show();
                    Close();
                });
                return;
            }

            SetStatusText(InitStrings.CheckingWhitelist, 10);
            Thread.Sleep(300);

            SetStatusText(InitStrings.CheckingWhitelist, 30);
            Thread.Sleep(300);

            // Set a stub version so MainWindow title works
            Globals.Version = "1.0";
            Globals.DllPath = "bin\\Synapse.dll";
            Globals.LauncherPath = "bin\\Synapse Launcher.exe";

            SetStatusText(InitStrings.DownloadingData, 50);
            Thread.Sleep(300);

            SetStatusText(InitStrings.CheckingData, 75);
            Thread.Sleep(300);

            SetStatusText(InitStrings.Ready, 100);
            Thread.Sleep(1000);

            Dispatcher.Invoke(() =>
            {
                try
                {
                    var Main = new MainWindow();
                    Main.Show();
                    Close();
                }
                catch (Exception ex)
                {
                    var Result = MessageBox.Show(
                        $"Synapse has encountered an exception during UI initialization. Please report the following text below to 3dsboy08 on Discord (make sure to give the text, not an image):\n\n{ex}\n\nIf you would like this text copied to your clipboard, press 'Yes'.",
                        "Synapse X",
                        MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No);

                    if (Result != MessageBoxResult.Yes) return;

                    var STAThread = new Thread(delegate ()
                    {
                        Clipboard.SetText(ex.ToString());
                    });
                    STAThread.SetApartmentState(ApartmentState.STA);
                    STAThread.Start();
                    STAThread.Join();
                    Thread.Sleep(1000);
                    Environment.Exit(0);
                }
            });
        }
    }
}