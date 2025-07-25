using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using YAAL.Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace YAAL;

public partial class NewLauncher : Window
{
    private CLMakerWindow clmaker;
    public NewLauncher()
    {
        InitializeComponent();
        SetBackground();
    }

    public NewLauncher(CLMakerWindow parent)
    {
        InitializeComponent();
        SetBackground();
        FilePick.Click += _PickFile;
        Create.Click += _CreateLauncher;
        ApworldPath.TextChanged += _ApworldPathTextChanged;
        clmaker = parent;
        AutoRename.ItemsSource = new List<string>() {
            true.ToString(),
            false.ToString()
        };
    }

    private void _ApworldPathTextChanged(object? sender, TextChangedEventArgs e)
    {
        Debouncer.Debounce(ApworldPathTextChanged, 0.5f);
    }

    public void ApworldPathTextChanged()
    {
        if (File.Exists(ApworldPath.Text))
        {
            string gameName = IOManager.GetGameNameFromApworld(ApworldPath.Text);
            if(gameName != "")
            {
                LauncherName.Text = IOManager.FindAvailableDirectoryName(gameName);
                GameName.Text = gameName;
            }

            DirectoryInfo dir = new DirectoryInfo(ApworldPath.Text);
            string parent = dir.Parent.Parent.Name;
            string folder = dir.Parent.Name;
            if (parent == "lib" && folder == "worlds")
            {
                VersionName.Text = "AP " + IOManager.GetArchipelagoVersion();
            }
        }
    }

    private async void _PickFile(object? sender, RoutedEventArgs e)
    {
        ApworldPath.Text = await IOManager.PickFile(this);
    }

    private void _CreateLauncher(object? sender, RoutedEventArgs e)
    {
        if(LauncherName.Text == null || LauncherName.Text == "")
        {
            return;
        }
        Cache_CustomLauncher launcher = new Cache_CustomLauncher();
        launcher.settings[LauncherSettings.launcherName] = LauncherName.Text ?? "";
        launcher.settings[LauncherSettings.gameName] = GameName.Text ?? "";
        launcher.settings[LauncherSettings.githubURL] = GitURL.Text ?? "";
        launcher.settings[LauncherSettings.filters] = GitFilters.Text ?? "";
        launcher.settings[LauncherSettings.apworld] = ApworldPath.Text ?? "";
        launcher.settings[LauncherSettings.Debug_AsyncName] = "Debug_CLMaker_Async";
        launcher.settings[LauncherSettings.Debug_SlotName] = "Debug_CLMaker_Slot";
        launcher.settings[LauncherSettings.Debug_Patch] = "";
        launcher.settings[LauncherSettings.Debug_baseLauncher] = "";
        launcher.settings[LauncherSettings.renamePatch] = AutoRename.SelectedItem.ToString() ?? "True";
        launcher.isGame = true;


        if (ApworldPath.Text != null && ApworldPath.Text != "")
        {
            if(VersionName.Text == null || VersionName.Text == "" || ApworldPath.Text == null || ApworldPath.Text == "")
            {
                VersionName.Text = "Unknown";
            }
            if(!IOManager.AddDefaultVersion(LauncherName.Text, VersionName.Text, ApworldPath.Text))
            {
                ErrorManager.AddNewError(
                    "New Launcher - Couldn't add default version",
                    "Tried to add a new default version with name " + LauncherName.Text + ", version name " + VersionName.Text + " and apworld located at " + ApworldPath.Text + " but something went wrong during that process"
                    );
                return;
            }

            Dictionary<string, string> instructionSettings = new Dictionary<string, string>
            {
                {"apworldTarget", ApworldPath.Text }
            };
            launcher.instructions.Add("0-Apworld", instructionSettings);
            IOManager.CreateNewDownloadCache(LauncherName.Text, VersionName.Text);
        }

        IOManager.SaveCacheLauncher(launcher);
        Window? parentWindow = this.GetVisualRoot() as Window;
        clmaker.LoadLauncher(LauncherName.Text);
        parentWindow.Close();
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
            Background_5.Background = new SolidColorBrush(Color.Parse("#454545"));
            Background_6.Background = new SolidColorBrush(Color.Parse("#454545"));
        }
        else
        {
            Background_0.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_1.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_2.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_3.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_4.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_5.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_6.Background = new SolidColorBrush(Color.Parse("#AAA"));
        }
    }
}