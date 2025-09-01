using YAAL.Assets.Script.Cache;
using YAAL.Assets.Scripts;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static YAAL.LauncherSettings;
using static YAAL.SlotSettings;

namespace YAAL;

public partial class TestWindow : Window
{
    private CustomLauncher launcher;
    private static TestWindow _testWindow;
    public TestWindow()
    {
        InitializeComponent();
        SetBackground();
        this.Closing += _Save;
        this.File.Click += _FileSelect;
        this.Launch.Click += _Launch;
        this.Restore.Click += _Restore;
        _testWindow = this;
        this.Closed += (sender, args) => { _testWindow = null; };
        AutoTheme.SetTheme(TrueBackground, ThemeSettings.backgroundColor);
    }

    public static TestWindow GetTestWindow()
    {
        if (_testWindow == null)
        {
            return new TestWindow();
        }
        else
        {
            _testWindow.Activate();
            _testWindow.Topmost = true;
            _testWindow.Topmost = false;
            return _testWindow;
        }
    }

    private static void RemoveWindow(object? sender, EventArgs e)
    {
        _testWindow = null;
    }

    public void Setup(CustomLauncher newLauncher, System.Collections.IEnumerable versions)
    {
        this.launcher = newLauncher;
        this.LauncherName.Text = newLauncher.GetSetting(launcherName);
        this.AsyncName.Text = newLauncher.GetSetting(Debug_AsyncName);
        this.SlotName.Text = newLauncher.GetSetting(Debug_SlotName);
        this.Patch.Text = newLauncher.GetSetting(Debug_Patch);
        this.VersionSelect.ItemsSource = versions;
        this.VersionSelect.SelectedIndex = 0;
        this.LauncherSelect.ItemsSource = IOManager.GetLauncherList();
        this.Background_LauncherSelect.IsVisible = !newLauncher.isGame;
        this.Background_BaseVersion.IsVisible = !newLauncher.isGame;

        Cache_Slot cache = IOManager.GetSlot(newLauncher.GetSetting(Debug_AsyncName), newLauncher.GetSetting(Debug_SlotName));
        if (cache.settings[baseLauncher] != "")
        {
            try
            {
                this.LauncherSelect.SelectedItem = cache.settings[baseLauncher];
            }
            catch (Exception)
            {
                this.LauncherSelect.SelectedIndex = 0;
            }
        }
        else
        {
            this.LauncherSelect.SelectedIndex = 0;
        }

        this.BaseVersionSelect.ItemsSource = IOManager.GetDownloadedVersions(this.LauncherSelect.SelectedItem.ToString());
        if (cache.settings[version] != "")
        {
            try
            {
                this.BaseVersionSelect.SelectedItem = cache.settings[version];
            }
            catch (Exception)
            {
                this.BaseVersionSelect.SelectedIndex = 0;
            }
        } else
        {
            this.BaseVersionSelect.SelectedIndex = 0;
        }
        
        string savedLauncher = newLauncher.GetSetting(Debug_baseLauncher);

        if (savedLauncher != "")
        {
            int i = 0;
            foreach (var item in this.LauncherSelect.ItemsSource)
            {
                if (item.ToString() == savedLauncher)
                {
                    this.LauncherSelect.SelectedIndex = i;
                }
                ++i;
            }
        }

    }

    public void SetBackground()
    {
        var theme = Application.Current.ActualThemeVariant;
        if (theme == ThemeVariant.Dark)
        {
            Background_0.Background = new SolidColorBrush(Color.Parse("#454545"));
            Background_1.Background = new SolidColorBrush(Color.Parse("#454545"));
            Background_2.Background = new SolidColorBrush(Color.Parse("#454545"));
            Background_3.Background = new SolidColorBrush(Color.Parse("#454545"));
            Background_4.Background = new SolidColorBrush(Color.Parse("#454545"));
            Background_LauncherSelect.Background = new SolidColorBrush(Color.Parse("#454545"));
            Background_BaseVersion.Background = new SolidColorBrush(Color.Parse("#454545"));
        }
        else
        {
            Background_0.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_1.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_2.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_3.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_4.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_LauncherSelect.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_BaseVersion.Background = new SolidColorBrush(Color.Parse("#AAA"));
        }
    }

    public void Save()
    {
        string asyncName = this.AsyncName.Text;
        string slotName = this.SlotName.Text;

        if(asyncName == "")
        {
            asyncName = "Debug_CLMaker_Async";
        }

        if(slotName == "")
        {
            slotName = "Debug_CLMaker_Slot";
        }


        launcher.SetSetting(Debug_AsyncName, asyncName);
        launcher.SetSetting(Debug_SlotName, slotName);
        launcher.SetSetting(Debug_Patch, this.Patch.Text);

        if (!launcher.isGame)
        {
            launcher.SetSetting(Debug_baseLauncher, this.LauncherSelect.SelectedItem.ToString());
        }
        launcher.Save();
    }

    // -- events

    // on closing
    private void _Save(object? sender, WindowClosingEventArgs e)
    {
        Save();
    }

    // on button pushed
    private void _Launch(object? sender, RoutedEventArgs e)
    {
        Save();
        string asyncName = launcher.GetSetting(Debug_AsyncName);
        string slotName = launcher.GetSetting(Debug_SlotName);
        launcher.ReadSettings(asyncName, slotName);

        if (launcher.isGame)
        {
            SetGameSettings(asyncName, slotName);
        } else
        {
            if(!SetToolSettings(asyncName, slotName))
            {
                ErrorManager.ThrowError();
                return;
            }
        }

        Cache_PreviousSlot cache = IOManager.GetLastAsync(launcher.GetSetting(launcherName));
        string debugPatch = launcher.GetSetting(Debug_Patch);

        if (cache.previousPatch != debugPatch)
        {
            launcher.settings[rom] = "";
        }

        launcher.settings[patch] = debugPatch;
        launcher.settings[version] = this.VersionSelect.SelectedItem.ToString();
        if (!launcher.isGame)
        {
            launcher.settings[baseLauncher] = this.LauncherSelect.SelectedItem.ToString();
        }
        launcher.Execute();
        this.Patch.Text = launcher.GetSetting(Debug_Patch);
    }

    public void SetGameSettings(string asyncName, string slotName)
    {
        if (launcher.settings[baseLauncher] == "")
        {
            IOManager.SetSlotSetting(asyncName, slotName, baseLauncher, launcher.selfsettings[launcherName]);
            launcher.settings[baseLauncher] = launcher.settings[launcherName];
        }

        if (launcher.settings[version] == "")
        {
            List<string> available = IOManager.GetDownloadedVersions(launcher.settings[launcherName]);
            IOManager.SetSlotSetting(asyncName, slotName, version, available[0]);
        }
    }

    public bool SetToolSettings(string asyncName, string slotName)
    {
        CustomLauncher thisBaseLauncher = launcher.GetBaseLauncher();
        if(thisBaseLauncher == null)
        {
            return false;
        }
        thisBaseLauncher.settings[version] = BaseVersionSelect.SelectedItem.ToString();

        if (launcher.settings[baseLauncher] == "")
        {
            List<string> launcherList = IOManager.GetLauncherList();
            if (launcherList.Count > 0)
            {
                CustomLauncher firstLauncher;
                int i = 0;
                do
                {
                    firstLauncher = IOManager.LoadLauncher(launcherList[i]);
                    ++i;
                } while (!firstLauncher.isGame && i < launcherList.Count);

                if (firstLauncher != null)
                {
                    IOManager.SetSlotSetting(asyncName, slotName, baseLauncher, firstLauncher.selfsettings[launcherName]);
                    launcher.settings[baseLauncher] = firstLauncher.selfsettings[launcherName];
                }
                else
                {
                    ErrorManager.AddNewError(
                        "TestWindow - Couldn't find a non-tool launcher",
                        "Tried to test in slot " + slotName + " from async " + asyncName + ", which doesn't have a baseLauncher set. Couldn't set one automatically, because no non-tool launcher exist. This is not allowed."
                        );
                    return false;
                }
            }
        }

        if (launcher.settings[version] == "")
        {
            CustomLauncher baseLauncher = launcher.GetBaseLauncher();
            if (baseLauncher != null)
            {
                List<string> available = IOManager.GetDownloadedVersions(baseLauncher.settings[launcherName]);
                if (available.Count == 0)
                {
                    ErrorManager.AddNewError(
                        "TestWindow - Couldn't find a version",
                        "Tried to test in slot " + slotName + " from async " + asyncName + ", which doesn't have a version set. Couldn't set one automatically, because the (maybe automatically ?) selected baseLauncher, " + baseLauncher.selfsettings[launcherName] + ", doesn't have any available version."
                        );
                    return false;
                }
                else
                {
                    IOManager.SetSlotSetting(asyncName, slotName, version, available[0]);
                }
            }
        }

        return true;
    }

    private void _Restore(object? sender, RoutedEventArgs e)
    {
        ProcessManager.StartProcess(Environment.ProcessPath, ("--restore --exit"));
    }

    private async void _FileSelect(object? sender, RoutedEventArgs e)
    {
        Patch.Text = await IOManager.PickFile(this);
    }
}