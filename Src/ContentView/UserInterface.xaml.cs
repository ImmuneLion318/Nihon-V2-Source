using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Linq;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Newtonsoft.Json;

using Nihon.Interfaces;
using Nihon.Static;
using Nihon.UserControlView;
using Nihon.Watcher;

using static Nihon.Static.Native;
using static Nihon.Interfaces.ModuleBase;
using static Nihon.UserControlView.WebView;

namespace Nihon.ContentView;

public partial class UserInterface : Window
{
    public Module Library = new Module(async () =>
    {
        //HttpRequest Request = new HttpRequest
        //{
        //    Url = "[PRIVATE_INFO]",
        //    Method = HttpMethod.Get,
        //    Body = string.Empty,

        //    Headers = new WebHeaderCollection
        //    { { HttpRequestHeader.UserAgent, "Nihon-3.0" } }
        //};
        //HttpResponse Response = await HttpService.CreateRequest(Request);

        //Data.ModuleConfig Config = JsonConvert.DeserializeObject<Data.ModuleConfig>
        //    (Response.Text);

        //if (!File.Exists("Import-Module.dll"))
        //    await HttpService.DownloadFileAsync(Config.InjectDependency, "Import-Module.dll");

        //if (!File.Exists("Module.dll"))
        //    await HttpService.DownloadFileAsync(Config.ModuleInfo.DownloadLink, "Module.dll");

        //if (Config.ModuleInfo.IsPatched == true)
        //    MessageBox.Show("It Appears Nihon Is Currently Patched Right Now Please Try Again Later.", "Error - Patched",
        //         MessageBoxButton.OK, MessageBoxImage.Error);

        //if (Config != null && Config.ModuleInfo.IsPatched != true)
        //    await HttpService.DownloadFileAsync(Config.ModuleInfo.DownloadLink, "Module.dll");
    })
    {
        ModulePath = "Nihon-Module.dll"
    };

    private Data.ScriptObjectHolder ScriptData;
    public IntPtr WindowHandle = IntPtr.Zero;

    public FileSystemWatcher FileWatcher = new FileSystemWatcher
    {
        Path = "./Scripts",
        Filter = "*.*",
        EnableRaisingEvents = true,
        IncludeSubdirectories = true,

        NotifyFilter = NotifyFilters.Attributes
         | NotifyFilters.CreationTime
         | NotifyFilters.DirectoryName
         | NotifyFilters.FileName
         | NotifyFilters.LastAccess
         | NotifyFilters.LastWrite
         | NotifyFilters.Security
         | NotifyFilters.Size
    };

    public TreeView ScriptTree = new TreeView
    {
        Name = "ScriptTree",
        BorderThickness = new Thickness(0),
        Background = Brushes.Transparent,
        Margin = new Thickness(5, 45, 405, 5),
        Width = 180,
        Height = double.NaN,
        HorizontalAlignment = HorizontalAlignment.Left,
        Visibility = Visibility.Hidden,
    };

    public UserInterface() => this.InitializeComponent();

    private async void OnLoad(object Source, RoutedEventArgs e)
    {
        this.WindowHandle = new WindowInteropHelper(this).Handle;

        OsVersionInfo OsInfo; RtlGetVersion(out OsInfo);
        if (OsInfo.MajorVersion == 10 && OsInfo.BuildNumber >= 22000)
        {
            WindowAttribute WindowAttribute = WindowAttribute.SmallRound;
            DwmSetWindowAttribute(this.WindowHandle, 33, ref WindowAttribute, sizeof(uint));
        }

        #region Directory Creation

        string BaseDirectory = string.Empty;

        Directory.EnumerateDirectories($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Packages")
            .ToList()
            .ForEach(x =>
            {
                if (x.Contains("ROBLOXCORPORATION.ROBLOX_"))
                    BaseDirectory = x + "\\AC";
            });

        string[] Directories = { "Bin", "Scripts", $"{BaseDirectory}\\Workspace", $"{BaseDirectory}\\AutoExec" };

        for (int i = 0; i < Directories.Length; ++i)
            Directory.CreateDirectory(Directories[i]);

        string[] Shortcuts = { "Workspace.lnk", "AutoExec.lnk" };
        
        for (int i = 0; i < Shortcuts.Length; ++i)
            if (!File.Exists(Shortcuts[i]))
            {
                IWshRuntimeLibrary.WshShell Shell = new IWshRuntimeLibrary.WshShell { };

                IWshRuntimeLibrary.IWshShortcut Shortcut = (IWshRuntimeLibrary.IWshShortcut)Shell.CreateShortcut(Shortcuts[i]);

                Shortcut.TargetPath = $"{BaseDirectory}\\{Shortcuts[i].Replace(".lnk", string.Empty)}";
                Shortcut.Description = $"Open's The Working Directory To {Shortcuts[i].Replace(".lnk", string.Empty)}.";

                Shortcut.Save();
            }

        #endregion

        this.InitScriptTree(new string[] { "./Scripts", $"{BaseDirectory}\\Workspace", $"{BaseDirectory}\\AutoExec" });

        //try
        //{
        //    HttpRequest Request = new HttpRequest
        //    {
        //        Url = "[PRIVATE_INFO]",
        //        Method = HttpMethod.Get,

        //        Headers = new WebHeaderCollection
        //        { { HttpRequestHeader.Cookie, "[PRIVATE_INFO]" } }
        //    };
        //    HttpResponse Response = await HttpService.CreateRequest(Request);

        //    this.ScriptData = JsonConvert.DeserializeObject<Data.ScriptObjectHolder>
        //        (Response.Text);

        //    foreach (Data.ScriptObject ScriptObject in this.ScriptData.ScriptEntries)
        //    {
        //        ScriptItem Object = new ScriptItem
        //        {
        //            Width = 195,
        //            Height = 115,
        //            Margin = new Thickness(-3),
        //            Script = ScriptObject
        //        };

        //        Object.Executed += async (object Source, RoutedEventArgs Event) =>
        //        {
        //            HttpRequest Request = new HttpRequest
        //            {
        //                Url = "[PRIVATE_INFO]",
        //                Method = HttpMethod.Get,

        //                Headers = new WebHeaderCollection
        //                { { HttpRequestHeader.Cookie, "[PRIVATE_INFO]" } }
        //            };
        //            HttpResponse Response = await HttpService.CreateRequest(Request);

        //            await Library.RunScript(Response.Text);
        //        };

        //        this.ScriptHubPanel.Children.Add(Object);

        //        this.SearchBox.TextChanged += (object Source, TextChangedEventArgs Event) =>
        //        {
        //            ICollectionView View =
        //            CollectionViewSource.GetDefaultView(Object.Title.Text.ToLower());

        //            if (View != null) View.Filter = (object O) =>
        //            {
        //                Object.Visibility = Object.Title.Text.ToLower().Contains(this.SearchBox.Text.ToLower())
        //                    ? Visibility.Visible : Visibility.Collapsed;

        //                return O.ToString().ToLower().Contains(this.SearchBox.Text.ToLower());
        //            };
        //        };
        //    }
        //}
        //catch
        //{
        //    MessageBox.Show("Script Cloud Requests Quota (50,000) Reached, Script Cloud Temporarily Unavailable.", "Error - Quota Max",
        //        MessageBoxButton.OK, MessageBoxImage.Warning);
        //}

        //await WebSocket.StartAsync(
        //    24960,
        //    this);
    }

    private async void KeyHandler(object Source, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && this.IsMouseOver == true)
        {
            switch (e.Key)
            {
                case Key.E:
                    await this.Library.RunScript(await this.Editor.ExecuteScript(CommandType.GetText, string.Empty));
                    break;
                case Key.I:
                    await this.Library.Inject(0);
                    break;
                case Key.M:
                    this.WindowState = WindowState.Minimized;
                    break;
            }
        }
    }

    private void MouseHandler(object Source, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
        {
            if (this.StealthMode.IsChecked != true)
                this.Cursor = Cursors.Hand;

            this.DragMove();
        }
        this.Cursor = Cursors.Arrow;
    }

    private void MinimizeButton_Click(object Source, RoutedEventArgs e) =>
        this.WindowState = WindowState.Minimized;

    private async void CloseButton_Click(object Source, RoutedEventArgs e)
    {
        /*
        if (this.EditorControl.SelectedIndex != 0)
        {
            string Path = "Interface.json";

            List<Data.InterfaceSettings.TabData> Tabs = new List<Data.InterfaceSettings.TabData> { };

            for (int i = 0; i < this.EditorControl.Items.Count; ++i)
            {
                TabItem Tab = (TabItem)this.EditorControl.Items[i];

                Tabs.Add(new Data.InterfaceSettings.TabData
                {
                    Name = Tab.Header.ToString(),
                    Text = Compressor.Compress(Compressor.Type.Lz4, await ((WebView)Tab.Content).ExecuteScript(CommandType.GetText, string.Empty))
                });
            }

            File.WriteAllText(Path, JsonConvert.SerializeObject(new Data.InterfaceSettings
            {
                Top = Top,
                Left = Left,
                Height = Height,
                Width = Width,
                Tabs = Tabs,

            }, Formatting.Indented));
        }

        if (this.Client != null)
            this.Client.SignOut();
        */

        Application.Current.Shutdown();
    }

    private async void ExecuteButton_Click(object Source, RoutedEventArgs e) =>
        await Library.RunScript(await this.Editor.ExecuteScript(CommandType.GetText, string.Empty));

    private async void ClearButton_Click(object Source, RoutedEventArgs e) =>
        await this.Editor.ExecuteScript(CommandType.SetText, string.Empty);

    private async void OpenFileButton_Click(object Source, RoutedEventArgs e)
    {
        OpenFileDialog OpenFile = new OpenFileDialog
        {
            Title = "Nihon - Open File",
            Filter = "Script Files (*.lua, *.txt)|*.lua;*.txt",
            InitialDirectory = $"{Environment.CurrentDirectory}\\Scripts\\"
        };

        if (OpenFile.ShowDialog() == true)
            await this.Editor.ExecuteScript(CommandType.SetText, File.ReadAllText(OpenFile.FileName));
    }

    private async void SaveFileButton_Click(object Source, RoutedEventArgs e)
    {
        SaveFileDialog SaveFile = new SaveFileDialog
        {
            Title = "Nihon - Save File",
            Filter = "Script Files (*.lua, *.txt)|*.lua;*.txt",
            FileName = "",
            InitialDirectory = $"{Environment.CurrentDirectory}\\Scripts\\"
        };

        if (SaveFile.ShowDialog() == true)
            File.WriteAllText(SaveFile.FileName, await this.Editor.ExecuteScript(CommandType.GetText, string.Empty));
    }

    private async void AttachButton_Click(object Source, RoutedEventArgs e)
    {
        Library.ModuleReady += (object Source, InjectorOutput Output) =>
        {
            Library.IsLoaded = true;
        };

        InjectorOutput InjectOutput = await Library.Inject(0);

        if (InjectOutput.Output != InjectorEvent.Success)
        {
            MessageBox.Show(InjectOutput.Message, "Error - Injection Failure",
             MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PageHandler(object Source, RoutedEventArgs e)
    {
        RadioButton Menu = (RadioButton)Source;

        _ = Menu.Name switch
        {
            "SettingsButton" => this.MenuControl.SelectedItem = this.SettingsTab,
            "GameHubButton" => this.MenuControl.SelectedItem = this.ScriptHubTab,
            "ExecutionButton" => this.MenuControl.SelectedItem = this.ExecutionTab,
            "InstallationButton" => this.MenuControl.SelectedItem = this.IntegrationTab,
            _ => throw new NotImplementedException()
        };
    }

    private void StayOnTopHandler(object Source, RoutedEventArgs e)
    {
        CheckBox TopMostToggle = (CheckBox)Source;

        bool Out = TopMostToggle.IsChecked == true ?
            this.Topmost = true : this.Topmost = false;
    }

    private void KillRoblox_Click(object Source, RoutedEventArgs e)
    {
        foreach (Process Proc in Process.GetProcessesByName("Windows10Universal"))
            Proc.Kill();
    }

    private void MultiRoblox_Click(object Source, RoutedEventArgs e)
    {
        /* Disabled Until We Can Find A Method To Do In C# For Uwp. */
    }

    private void StealthModeHandler(object Source, RoutedEventArgs e)
    {
        CheckBox StealthToggle = (CheckBox)Source;

        if (StealthToggle.IsChecked == true)
        {
            this.MainWindow.ShowInTaskbar = false;
            this.StayOnTop.IsChecked = true;
            SetWindowDisplayAffinity(this.WindowHandle, DisplayAffinity.Exclude);
            this.StayOnTop.IsEnabled = false;
        }
        else if (StealthToggle.IsChecked != true)
        {
            this.MainWindow.ShowInTaskbar = true;
            SetWindowDisplayAffinity(this.WindowHandle, DisplayAffinity.None);
            this.StayOnTop.IsEnabled = true;
        }
    }

    ProcessWatcher Watcher = new ProcessWatcher("Windows10Universal") { Interval = 500 };

    private void AutoAttachHandler(object Source, RoutedEventArgs e)
    {
        CheckBox AutoToggle = (CheckBox)Source;

        if (AutoToggle.IsChecked == true)
        {
            this.Watcher = new ProcessWatcher("Windows10Universal") { Interval = 500 };

            this.Watcher.Created += async (Process Proc) =>
            {
                while (Proc.MainWindowHandle == IntPtr.Zero)
                    await Task.Delay(250);

                if (Proc.MainWindowTitle == string.Empty && Proc.MainWindowHandle == IntPtr.Zero)
                    await this.Library.Inject(3);
            };
            this.Watcher.Init();
        }
        else if (AutoToggle.IsChecked != true)
            this.Watcher.Dispose();
    }

    private void SafetyNetHandler(object Source, RoutedEventArgs e)
    {
        CheckBox SafetyToggle = (CheckBox)Source;

        /*
        Thread SafetyThread = new Thread(async () =>
        {
            while (Process.GetProcessesByName("RobloxPlayerBeta").Length != 2)
                await Task.Delay(TimeSpan.FromSeconds(3));

            Process Daemon = Process.GetProcessesByName("RobloxPlayerBeta").Single
            (x => x.WorkingSet64 / Math.Pow(1024.0, 2.0) < 50.0);

            IntPtr hProc = OpenProcess(
                ProcessAccess.All,
                false,
                Daemon.Id);

            SuspendProcess(hProc);
            
            Console.WriteLine($"Disabled Anti-Cheat In Process - {Daemon.Id}.");
        });

        if (SafetyToggle.IsChecked == true)
            SafetyThread.Start();
        
        else if (SafetyToggle.IsChecked != true && SafetyThread != null)
            SafetyThread.Suspend();    
        */

        if (SafetyToggle.IsChecked == true)
        {
            MessageBox.Show("Due To Security Vulnerabilities In The Safety-Net Feature, It Has Been Temporarily Disabled.", "Error - N/A",
                MessageBoxButton.OK, MessageBoxImage.Warning);

            SafetyToggle.IsChecked = false;
        }

        else if (SafetyToggle.IsChecked != true)
            return;
    }

    private void SaveSettings_Click(object Source, RoutedEventArgs e)
    {
        DataInterface.Save("Settings", new Data.InterfaceOptions
        {
            AutoAttach = this.AutoAttach.IsChecked.Value,
            AutoExecute = this.AutoExecute.IsChecked.Value,
            StealthMode = this.StealthMode.IsChecked.Value,
            TopMost = this.StayOnTop.IsChecked.Value
        });
    }

    public void RetrieveContent(TreeView TreeView, string Path) => TreeView.Items.Add(this.DirectoryNode(new DirectoryInfo(Path)));

    private TreeViewItem GetTreeView(string Tag, string Text, string ImagePath, bool IsSubFolder)
    {
        SolidColorBrush Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#56B4DA"));
        switch (ImagePath)
        {
            case "Auto-Execute.png":
                Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#85E775"));
                break;
            case "Github.png":
                Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7C7F8A"));
                break;
            case "Folder.png":
                Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCFE8F"));
                break;
        }

        StackPanel Panel = new StackPanel { Orientation = Orientation.Horizontal };

        TreeViewItem Item = new TreeViewItem
        {
            Foreground = Brushes.Gray,
            Tag = Tag,
            IsExpanded = true,
            Header = Panel,
            ToolTip = null,
            FontFamily = (FontFamily)this.TryFindResource("Poppins"),
            Style = (Style)this.FindResource("TreeViewItemStyle"),
        };

        Item.Selected += async (object Source, RoutedEventArgs Event) =>
        {
            try
            {
                TreeViewItem Item = this.ScriptTree.SelectedItem as TreeViewItem;

                if (this.ScriptTree.SelectedItem != null)
                    using (StreamReader Reader = new StreamReader(Item.Tag as string))
                        await this.Editor.ExecuteScript(CommandType.SetText, Reader.ReadToEnd());
            }
            catch { }
        };

        Image FileImage = new Image
        {
            Source = new BitmapImage(new Uri($"pack://application:,,/Graphic's/Internal Graphic's/{ImagePath}")),
            Width = 20,
            Height = 20
        };
        RenderOptions.SetBitmapScalingMode(FileImage, BitmapScalingMode.HighQuality);

        Label Label = new Label
        {
            FontFamily = (FontFamily)this.TryFindResource("Poppins"),
            Content = Text,
            Foreground = Color
        };

        if (IsSubFolder == true)
            Label.Foreground = Brushes.Gray;

        Panel.Children.Add(FileImage);
        Panel.Children.Add(Label);

        ToolTipService.SetIsEnabled(FileImage, true);
        return Item;
    }

    private TreeViewItem DirectoryNode(DirectoryInfo DirectoryInfo)
    {
        string FolderIcon = "Folder.png";
        switch (DirectoryInfo.Name)
        {
            case "Scripts":
                FolderIcon = "Lua.png";
                break;
            case "AutoExec":
                FolderIcon = "Auto-Execute.png";
                break;
            case "Workspace":
                FolderIcon = "Folder.png";
                break;
            case "Github":
                FolderIcon = "Github.png";
                break;
            case "Pastebin":
                FolderIcon = "Text Document.png";
                break;
        }

        TreeViewItem Node = this.GetTreeView(DirectoryInfo.FullName, DirectoryInfo.Name, FolderIcon, false);

        foreach (DirectoryInfo Directory in DirectoryInfo.GetDirectories())
            Node.Items.Add(this.DirectoryNode(Directory));

        foreach (FileInfo Files in DirectoryInfo.GetFiles())
        {
            if (Files.Extension == ".lua")
                Node.Items.Add(this.GetTreeView(Files.FullName, Files.Name, "Lua.png", true));

            else if (Files.Extension == ".txt")
                Node.Items.Add(this.GetTreeView(Files.FullName, Files.Name, "Text Document.png", true));

            else Node.Items.Add(this.GetTreeView(Files.FullName, Files.Name, "Unknown File.png", true));
        }

        return Node;
    }

    private void SetupWatcher(FileSystemWatcher Watcher, string FolderPath, string[] Directories)
    {
        Watcher = new FileSystemWatcher
        {
            Path = FolderPath,
            Filter = "*.*",
            EnableRaisingEvents = true,
            IncludeSubdirectories = false,

            NotifyFilter = NotifyFilters.Attributes
               | NotifyFilters.CreationTime
               | NotifyFilters.DirectoryName
               | NotifyFilters.FileName
               | NotifyFilters.LastAccess
               | NotifyFilters.LastWrite
               | NotifyFilters.Security
               | NotifyFilters.Size
        };

        Watcher.Created += (s, e) =>
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                this.ScriptTree.Dispatcher.Invoke(delegate
                {
                    this.ScriptTree.Items.Clear();

                    for (int i = 0; i < Directories.Length; ++i)
                        this.RetrieveContent(this.ScriptTree, Directories[i]);
                });
            }
        };
        Watcher.Renamed += (s, e) =>
        {
            if (e.ChangeType == WatcherChangeTypes.Renamed)
                File.Move(e.OldFullPath, e.FullPath);
        };
        Watcher.Deleted += (s, e) =>
        {
            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                this.ScriptTree.Dispatcher.Invoke(delegate
                {
                    this.ScriptTree.Items.Clear();

                    for (int i = 0; i < Directories.Length; ++i)
                        this.RetrieveContent(this.ScriptTree, Directories[i]);
                });
            }
        };
        Watcher.Changed += (s, e) =>
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                this.ScriptTree.Dispatcher.Invoke(delegate
                {
                    this.ScriptTree.Items.Clear();

                    for (int i = 0; i < Directories.Length; ++i)
                        this.RetrieveContent(this.ScriptTree, Directories[i]);
                });
            }
        };
    }

    public void InitScriptTree(string[] Directories)
    {
        this.Execution.Children.Add(this.ScriptTree);
        this.ScriptTree.Items.Clear();

        for (int i = 0; i < Directories.Length; ++i)
        {
            this.RetrieveContent(this.ScriptTree, Directories[i]);
            this.SetupWatcher(this.FileWatcher, Directories[i], Directories);
        }
    }

    private void TreeViewer_Click(object Source, RoutedEventArgs e)
    {
        if (this.ScriptTree.Visibility == Visibility.Visible)
        {
            Animations.Shift(this.EditorControl, new Thickness(190, 45, 5, 5), new Thickness(0, 50, 0, 0), 0.5, Ease.QuarticEase);
            Animations.Fade(this.ScriptTree, 1, 0, 0.5, Ease.QuinticEase);
            this.ScriptTree.Visibility = Visibility.Hidden;
            return;
        }

        Animations.Shift(this.EditorControl, new Thickness(0, 50, 0, 0), new Thickness(190, 45, 5, 5), 0.5, Ease.QuarticEase);
        Animations.Fade(this.ScriptTree, 0, 1, 0.5, Ease.QuinticEase);
        this.ScriptTree.Visibility = Visibility.Visible;
    }

    private void AdvertiseButton_Click(object Source, RoutedEventArgs e) =>
        Process.Start("https://wearedevs.net/exploits");

    private void RefreshButton_Click(object Source, RoutedEventArgs e)
    {
        /* Needs Rewriting */
    }

    private void AddTabButton_Click(object Source, RoutedEventArgs e) =>
        this.CreateItem($"Script #{this.EditorControl.Items.Count + 1}", string.Empty);

    private void CloseTabButton_Click(object Source, RoutedEventArgs e)
    {
        WebView Object = (WebView)this.EditorControl.SelectedContent;

        if (Object != null)
            Object.Dispose();

        this.EditorControl.Items.Remove(this.EditorControl.SelectedItem);
    }

    public WebView Editor
    {
        get
        {
            if (this.EditorControl.SelectedItem != null)
            {
                TabItem Tab = (TabItem)this.EditorControl.SelectedItem;

                if (Tab.Content != null)
                    return (WebView)Tab.Content;
            }

            MessageBox.Show("No Editor Found, Or Tab Item Exists Create An Item To Resolve This Issue.", "Error - Editor/Tab Item Missing",
                MessageBoxButton.OK, MessageBoxImage.Error);

            WebView Object = new WebView
            {
                EditorOptions =
                {
                    Links = true,
                    MiniMap = true,
                    FontSize = 14,
                    ReadOnly = false,
                    Theme = "Dark"
                }
            };

            Object.Dispose();

            return Object;
        }
    }

    public WebView CreateItem(string Title, string Text)
    {
        WebView Editor = new WebView
        {
            Width = double.NaN,
            Height = double.NaN,
            Margin = new Thickness(0),
            DefaultBackgroundColor = System.Drawing.Color.FromArgb(0, 255, 255, 255),
            Visibility = Visibility.Visible,
        };

        TabItem Item = new TabItem
        {
            Style = (Style)this.TryFindResource("TabItemStyle"),
            Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
            Header = Title,
            Content = Editor,
            Foreground = new SolidColorBrush(Color.FromRgb(112, 115, 134)),
            Padding = new Thickness(3),
            FontFamily = (FontFamily)this.TryFindResource("Poppins"),
        };

        Item.Loaded += delegate (object Source, RoutedEventArgs Event)
        {
            Editor.EditorReady += async (object Source, EventArgs Event) =>
            {
                Editor.EditorOptions = new Data.EditorOptions
                {
                    Links = true,
                    MiniMap = true,
                    FontSize = 14,
                    ReadOnly = false,
                    Theme = "Dark"
                };

                await Editor.ExecuteScript(CommandType.SetText, Text);
            };
        };

        Item.MouseDown += delegate (object Sender, MouseButtonEventArgs ButtonEvent)
        {
            if (ButtonEvent.OriginalSource is Border)
            {
                if (ButtonEvent.ChangedButton == MouseButton.Middle)
                {
                    WebView Object = (WebView)this.EditorControl.SelectedContent;
                    Object.Dispose();

                    this.EditorControl.Items.Remove(this.EditorControl.SelectedItem);
                }
            }
            else return;
        };

        this.EditorControl.Items.Add(Item);
        this.EditorControl.SelectedItem = Item;

        return Editor;
    }

    private void LoginButton_Click(object Source, RoutedEventArgs e)
    {
        MessageBox.Show("Unfortunately We Have Disabled Our Uploader While We Migrate To Nihon 3.0 Hope To See You There ~ Immune", "Error: Unavailable",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}