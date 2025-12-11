using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using YAAL.Assets.Scripts;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Diagnostics.Debug;
using static YAAL.LauncherSettings;
using System.Collections.ObjectModel;

namespace YAAL;

public partial class CLMakerWindow : ScalableWindow
{
    private void SetEvents(bool autoLoad)
    {
        ModeSelector.ItemsSource = new List<string> { "Game", "Tool" };
        ModeSelector.SelectedIndex = 0;

        AvailableVersions.ItemsSource = new List<string> { "None" };
        AvailableVersions.SelectedIndex = 0;

        GitHubVersions.ItemsSource = new List<string> { "None" };
        GitHubVersions.SelectedIndex = 0;

        CommandSelector.ItemsSource = Templates.commandNames;
        CommandSelector.SelectedIndex = 0;

        List<string> themeList = new List<string> { "None" };

        foreach (var item in IOManager.GetThemeList())
        {
            themeList.Add(item);
        }

        ThemeSelector.ItemsSource = themeList;
        ThemeSelector.SelectedIndex = 0;
        WindowManager.UpdateComboBox(ThemeSelector);

        this.Closing += OnWindowClosed;

        EmptyLauncher.Click += CreateNewLauncher;
        Duplicate.Click += DuplicateLauncher;
        Delete.Click += DeleteLauncher;
        Settings.Click += OpenSettingManager;
        Download.Click += DownloadVersion;
        Remove.Click += RemoveVersion;
        CommandAdder.Click += AddCommand;
        OpenTestWindow.Click += Test;

        Rename.Click += (_, _) =>
        {
            if(NamingBox.IsVisible)
            {
                TurnEventsOff();
                if(LauncherSelector.SelectedItem is Cache_DisplayLauncher cache 
                && NamingBox.Text is string newName 
                && newName != "" 
                && newName != cache.name)
                {
                    string trueName = IOManager.RenameLauncher(cache, newName);
                    if(trueName == cache.name)
                    {
                        return;
                    }
                    ReloadLauncherList(false);
                    if(LauncherSelector.ItemsSource is ObservableCollection<Cache_DisplayLauncher> list)
                    {
                        foreach (var item in list)
                        {
                            if(item.name == trueName)
                            {
                                LauncherSelector.SelectedItem = item;
                                LoadLauncher();
                                break;
                            }
                        }
                    }
                }

                NamingBox.IsVisible = false;
                LauncherSelector.IsVisible = true;
                TurnEventsBackOn();
            } else
            {
                NamingBox.IsVisible = true;
                LauncherSelector.IsVisible = false;
            }
        };

        AddNewVersion.Click += (_, _) =>
        {
            if(LauncherSelector.SelectedItem is Cache_DisplayLauncher cache)
            {
                VersionManager manager = WindowManager.OpenVersionWindow(cache.name);
                manager.Closed += (_, _) =>
                {
                    UpdateAvailableVersion();
                };
            }
        };

        VersionSetting.Click += (_, _) =>
        {
            if(LauncherSelector.SelectedItem is Cache_DisplayLauncher cache && AvailableVersions.SelectedItem is string version && version != "None")
            {
                VersionManager manager = WindowManager.OpenVersionWindow(cache.name, version);
                manager.Closed += (_, _) =>
                {
                    UpdateAvailableVersion();
                };
            }
        };

        //TurnEventsBackOn();
        ReloadLauncherList(autoLoad);
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        if(settingManager != null)
        {
            settingManager.Close();
        }
        Save();
    }

    private void TurnEventsOff()
    {
        // When loading a launcher, we need to turn these off while setting everything up
        // and then turn them back on when we're done
        ModeSelector.SelectionChanged -= ModeSelector_ChangedSelection;
        LauncherSelector.SelectionChanged -= LauncherSelector_ChangedSelection;
    }

    private void TurnEventsOn()
    {
        ModeSelector.SelectionChanged += ModeSelector_ChangedSelection;
        LauncherSelector.SelectionChanged += LauncherSelector_ChangedSelection;
        ThemeSelector.SelectionChanged += (_, _) =>
        {
            if(customLauncher != null && ThemeSelector.SelectedItem is string selection)
            {
                customLauncher.selfsettings[LauncherSettings.customTheme] = selection;
            }
        };
    }

    private async void TurnEventsBackOn()
    {
        // We can't turn events back on immediately, because the UI is in a different thread
        // If we did, they'd all trigger on the change we just did, and we don't want that
        // So instead we tell that particular thread to take care of turning them back on !
        await Dispatcher.UIThread.InvokeAsync(() => {
            TurnEventsOn();
        }, DispatcherPriority.Background);
    }


    // BUTTONS

    private void AddCommand(object? sender, RoutedEventArgs e)
    {
        AddCommand(CommandSelector.SelectedItem.ToString());
    }

    private void Test(object? sender, RoutedEventArgs e)
    {
        List<Interface_Instruction> instructions = new List<Interface_Instruction>();
        foreach (var item in commandList)
        {
            instructions.Add(item.GetInstruction());
        }
        customLauncher.ResetInstructionList(instructions);
        Save();

        if(LauncherSelector.SelectedItem is Cache_DisplayLauncher display)
        {
            TestWindow testWindow = TestWindow.GetTestWindow(this);
            bool requiresPatch = display.cache.settings[LauncherSettings.requiresPatch] == true.ToString();
            bool requiresVersion = display.cache.settings[LauncherSettings.requiresVersion] == true.ToString();
            if (AvailableVersions.ItemsSource is List<string> list)
            {
                testWindow.Setup(display.cache.settings[launcherName], list, requiresPatch, requiresVersion);
            }
            else
            {
                Debug.WriteLine("AvailableVersions isn't a list of string !?");
                testWindow.Setup(display.cache.settings[launcherName], new List<string>(), requiresPatch, requiresVersion);
            }
            testWindow.IsVisible = true;
        }
    }

    private void Save(object? sender = null, RoutedEventArgs e = null)
    {
        ReloadLauncher();
        if(customLauncher != null)
        {
            customLauncher.Save();
            if (LauncherSelector.ItemsSource is ObservableCollection<Cache_DisplayLauncher> list)
            {
                list[previousIndex].cache = customLauncher.WriteCache();
            }
        }
        
    }

    public async void DownloadVersion(object? sender, RoutedEventArgs e)
    {
        if(await WebManager.DownloadUpdatedApworld(customLauncher, GitHubVersions.SelectedItem.ToString()))
        {
            UpdateAvailableVersion();
        }
    }

    public void RemoveVersion(object? sender, RoutedEventArgs e)
    {
        if(WindowManager.OpenWindow(WindowType.ConfirmationWindow, this) is ConfirmationWindow confirm)
        {
            confirm.Setup(customLauncher.GetSetting(launcherName) + " - " + AvailableVersions.SelectedItem.ToString());

            confirm.Closing += (source, args) =>
            {
                if (confirm.confirmed)
                {
                    IOManager.RemoveVersion(customLauncher.GetSetting(launcherName), AvailableVersions.SelectedItem.ToString());
                    UpdateAvailableVersion();
                }
            };
        }
    }

    public void CreateNewLauncher(object? sender, RoutedEventArgs e)
    {
        Save();
        NewLauncher launcher = new NewLauncher(this);
        launcher.Show();

        launcher.Closing += (_, _) =>
        {
            if (!launcher.create)
            {
                return;
            }
            Cache_CustomLauncher cache = launcher.launcher;
            IOManager.SaveCacheLauncher(cache);
            string launcherName = cache.settings[LauncherSettings.launcherName];
            TurnEventsOff();
            ReloadLauncherList(false);
            if(LauncherSelector.ItemsSource is ObservableCollection<Cache_DisplayLauncher> list)
            {
                foreach (var item in list)
                {
                    if(item.name == launcherName)
                    {
                        LauncherSelector.SelectedItem = item;
                        LoadLauncher();
                        return;
                    }
                }
            }
            Debug.WriteLine("Error : couldn't find the newly created launcher !?");
            
        };
    }

    public Cache_CustomLauncher CreateDefaultLauncher(object? sender = null, RoutedEventArgs e = null)
    {
        Cache_CustomLauncher cache = DefaultManager.GetDefault<Cache_CustomLauncher>();
        string trueName = IOManager.FindAvailableLauncherName("NewLauncher");
        cache.settings[LauncherSettings.launcherName] = trueName;
        IOManager.SaveCacheLauncher(cache);
        return cache;
    }

    public void DuplicateLauncher(object? sender, RoutedEventArgs e)
    {
        Save();
        TurnEventsOff();
        if(LauncherSelector.SelectedItem is Cache_DisplayLauncher cache)
        {
            string trueName = IOManager.FindAvailableLauncherName(cache.name);
            customLauncher.SetSetting(launcherName, trueName);
            cache.name = trueName;
            NamingBox.Text = trueName;
            Save();
            ReloadLauncherList(false);
            if(LauncherSelector.ItemsSource is ObservableCollection<Cache_DisplayLauncher> list)
            {
                foreach (var item in list)
                {
                    if (item.name == trueName) {
                        LauncherSelector.SelectedItem = item;
                        break;
                    }
                }
            }
        }
        TurnEventsBackOn();
    }

    public void DeleteLauncher(object? sender, RoutedEventArgs e)
    {
        if(LauncherSelector.SelectedItem is Cache_DisplayLauncher cache && WindowManager.OpenWindow(WindowType.ConfirmationWindow, this) is ConfirmationWindow confirm)
        {
            confirm.Setup(cache.name);
            confirm.Closed += (source, args) =>
            {
                if (confirm.confirmed)
                {
                    TurnEventsOff();
                    customLauncher = null;
                    IOManager.DeleteLauncher(cache.name);
                    ReloadLauncherList(true);
                    TurnEventsBackOn();
                }
            };
        }

        
    }

    public void OpenSettingManager(object? sender, RoutedEventArgs e)
    {
        settingManager = SettingManager.GetSettingsWindow(this, this.customLauncher.selfsettings, this.customLauncher.customSettings);
        settingManager.Show();
        settingManager.Closing += UpdateSettings;
    }

    private void UpdateSettings(object? sender, WindowClosingEventArgs e)
    {
        Dictionary<string, string> newCustomSettings;
        Dictionary<LauncherSettings, string> newSettings = settingManager.OutputLauncherSettings(out newCustomSettings);
        if (this.customLauncher.selfsettings[githubURL] != newSettings[githubURL])
        {
            this.customLauncher.selfsettings[githubURL] = newSettings[githubURL];
            UpdateGitVersions();
        }
        this.customLauncher.selfsettings = newSettings;
        this.customLauncher.customSettings = newCustomSettings;
        Save();
        
        settingManager = null;
    }



    // CHANGED SELECTION
    private void ModeSelector_ChangedSelection(object? sender, SelectionChangedEventArgs e)
    {
        customLauncher.isGame = (ModeSelector.SelectedItem.ToString() == "Game");
        if (customLauncher.isGame)
        {
            IOManager.RemoveTool(customLauncher);
        } else
        {
            IOManager.AddTool(customLauncher);
        }
    }

    private void LauncherSelector_ChangedSelection(object? sender, SelectionChangedEventArgs e)
    {
        if(LauncherSelector.SelectedItem is Cache_DisplayLauncher cache)
        {
            Save();
            LoadLauncher(cache.cache);
            previousIndex = LauncherSelector.SelectedIndex;
        }
    }
}