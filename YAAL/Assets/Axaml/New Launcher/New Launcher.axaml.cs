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

public partial class NewLauncher : ScalableWindow
{
    public Cache_CustomLauncher launcher;
    public bool create = false;
    public NewLauncher()
    {
        InitializeComponent();
        AutoTheme.SetTheme(TrueBackground, ThemeSettings.backgroundColor);
        FilePick.Click += _PickFile;
        Create.Click += _CreateLauncher;
        ApworldPath.TextChanged += _ApworldPathTextChanged;
        this.Show();
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
                LauncherName.Text = IOManager.FindAvailableLauncherName(gameName);
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
        Cache_CustomLauncher cache = DefaultManager.launcher;
        cache.settings[LauncherSettings.launcherName] = LauncherName.Text ?? "";
        cache.settings[LauncherSettings.gameName] = GameName.Text ?? "";
        cache.settings[LauncherSettings.githubURL] = GitURL.Text ?? "";
        cache.settings[LauncherSettings.filters] = GitFilters.Text ?? "";
        cache.settings[LauncherSettings.apworld] = ApworldPath.Text ?? "";


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

            CommandSetting<ApworldSettings> commandSetting = new CommandSetting<ApworldSettings>();
            commandSetting.SetSetting(ApworldSettings.apworldTarget, ApworldPath.Text);
            cache.instructionList.Add(commandSetting);

            IOManager.CreateNewVersionCache(LauncherName.Text, VersionName.Text);
        }

        this.launcher = cache;
        create = true;
        if(this.GetVisualRoot() is Window parentWindow)
        {
            parentWindow.Close();
        }
    }
}