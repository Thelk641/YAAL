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
            _testWindow.Closing += (object? sender, WindowClosingEventArgs e) => { _testWindow = null; };
            return _testWindow;
        }
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
        this.LauncherSelect.SelectedIndex = 0;
        this.Background_LauncherSelect.IsVisible = !newLauncher.isGame;
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
        }
        else
        {
            Background_0.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_1.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_2.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_3.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_4.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_LauncherSelect.Background = new SolidColorBrush(Color.Parse("#AAA"));
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
        launcher.ReadSettings(launcher.GetSetting(Debug_AsyncName), launcher.GetSetting(Debug_SlotName));

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

    private void _Restore(object? sender, RoutedEventArgs e)
    {
        ProcessManager.StartProcess(Environment.ProcessPath, ("--restore --exit"));
    }

    private async void _FileSelect(object? sender, RoutedEventArgs e)
    {
        Patch.Text = await IOManager.PickFile(this);
    }
}