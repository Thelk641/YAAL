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

public partial class CLMakerWindow : Window
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

        TurnEventsBackOn();
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
        string version = GetLatestAvailableVersion();

        TestWindow testWindow = TestWindow.GetTestWindow();
        testWindow.Setup(customLauncher, AvailableVersions.ItemsSource);
        testWindow.IsVisible = true;
    }

    private void Save(object? sender = null, RoutedEventArgs e = null)
    {
        ReloadLauncher();
        if(customLauncher != null)
        {
            customLauncher.Save();
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
        ConfirmationWindow confirm = new ConfirmationWindow(customLauncher.GetSetting(launcherName) + " - " + AvailableVersions.SelectedItem.ToString());
        confirm.IsVisible = true;

        confirm.Closing += (source, args) =>
        {
            if (confirm.confirmed)
            {
                IOManager.RemoveDownloadedVersion(customLauncher.GetSetting(launcherName), AvailableVersions.SelectedItem.ToString());
                UpdateAvailableVersion();
            }
        };
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

    public void CreateEmptyLauncher(object? sender = null, RoutedEventArgs e = null)
    {
        if(customLauncher != null)
        {
            Save();
        }
        
        TurnEventsOff();
        customLauncher = new CustomLauncher();
        string trueName = IOManager.FindAvailableLauncherName("NewLauncher");
        customLauncher.SetSetting(launcherName, trueName);
        ModeSelector.SelectedIndex = 0;

        AvailableVersions.ItemsSource = new List<string> { "None" };
        AvailableVersions.SelectedIndex = 0;

        GitHubVersions.ItemsSource = new List<string> { "None" };
        GitHubVersions.SelectedIndex = 0;

        CommandSelector.ItemsSource = Templates.commandNames;
        CommandSelector.SelectedIndex = 0;

        foreach (var item in commandList)
        {
            item.DeleteComponent(false);
        }
        commandList = new List<Command>();
        Save();
        ReloadLauncherList();
        TurnEventsBackOn();
    }

    public void DuplicateLauncher(object? sender, RoutedEventArgs e)
    {
        Save();
        TurnEventsOff();
        if(LauncherSelector.SelectedItem is Cache_DisplayLauncher cache)
        {
            string trueName = IOManager.FindAvailableLauncherName(cache.name);
            customLauncher.SetSetting(launcherName, trueName);
        }
        Save();
        TurnEventsBackOn();
    }

    public void DeleteLauncher(object? sender, RoutedEventArgs e)
    {
        //TODO : this needs looking at, cf Pingu async feedback
        if(LauncherSelector.SelectedItem is Cache_DisplayLauncher cache)
        {
            ConfirmationWindow confirm = new ConfirmationWindow(cache.name);
            confirm.IsVisible = true;
            confirm.Closed += (source, args) =>
            {
                if (confirm.confirmed)
                {
                    TurnEventsOff();
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
            LoadLauncher(cache.cache);
        }
    }
}