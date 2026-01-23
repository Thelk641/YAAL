using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using YAAL.Assets.Scripts;
using static YAAL.ApworldSettings;

namespace YAAL;

public partial class CLM_Selector : UserControl
{
    private CLM clm;
    private bool loadOnChangedSelection = false;
    private string previousName = "";

    public CLM_Selector()
    {
        InitializeComponent();
    }

    public CLM_Selector(CLM newClm) : this()
    {
        this.clm = newClm;
        ReloadList();
        LauncherSelector.SelectedIndex = 0;
        if(LauncherSelector.SelectedItem is Cache_DisplayLauncher display)
        {
            NamingBox.Text = display.name;
            previousName = display.name;
        }

        LauncherSelector.SelectionChanged += (_, _) =>
        {
            if (loadOnChangedSelection && LauncherSelector.SelectedItem is Cache_DisplayLauncher display)
            {
                clm.LoadLauncher(display.cache);
                previousName = display.name;
                NamingBox.Text = display.name;
            }
        };

        NamingBox.AddHandler(InputElement.KeyDownEvent, (sender, e) =>
        {
            if(e.Key == Key.Enter)
            {
                SwitchMode();
                e.Handled = true;
            }
                
        }, RoutingStrategies.Tunnel);
    }

    public Cache_DisplayLauncher GetCache()
    {
        if(LauncherSelector.SelectedItem is Cache_DisplayLauncher display)
        {
            return display;
        }

        ErrorManager.ThrowError(
            "CLM_Selector - Couldn't find a Cache_CustomLauncher",
            "LauncherSelector.SelectedItem isn't a Cache_DisplayLauncher, please report this.");

        return new Cache_DisplayLauncher();
    }

    public void ReloadList(string toSelect = "")
    {
        List<string> launcherList = IOManager.GetLauncherList();
        ObservableCollection<Cache_DisplayLauncher> list = new ObservableCollection<Cache_DisplayLauncher>();

        if (launcherList.Count == 0)
        {
            Cache_DisplayLauncher defaultLauncher = new Cache_DisplayLauncher();
            defaultLauncher.cache = DefaultManager.GetDefault<Cache_CustomLauncher>();
            defaultLauncher.name = defaultLauncher.cache.settings[LauncherSettings.launcherName];
            IOManager.SaveCacheLauncher(defaultLauncher.cache);
            list.Add(defaultLauncher);
            LauncherSelector.ItemsSource = list;
            LauncherSelector.SelectedItem = defaultLauncher;
            NamingBox.Text = defaultLauncher.name;
            previousName = defaultLauncher.name;
            return;
        }

        Cache_DisplayLauncher? selection = null;

        foreach (var item in launcherList)
        {
            Cache_DisplayLauncher cache = new Cache_DisplayLauncher();
            cache.name = item;
            cache.cache = IOManager.LoadCacheLauncher(item);
            list.Add(cache);
            if(cache.name == toSelect)
            {
                selection = cache;
            }
        }

        LauncherSelector.ItemsSource = list;
        if(selection is Cache_DisplayLauncher found)
        {
            LauncherSelector.SelectedItem = found;
        }
    }

    public bool Save()
    {
        bool output = NamingBox.IsVisible;
        if (output)
        {
            SwitchMode();
        }
        return output;
    }

    public void SelectFirst()
    {
        if(LauncherSelector.SelectedIndex != 0)
        {
            LauncherSelector.SelectedIndex = 0;
        }
    }

    public void SetFocus()
    {
        NamingBox.Focus();
    }

    public void SwitchMode()
    {
        LauncherSelector.IsVisible = !LauncherSelector.IsVisible;
        NamingBox.IsVisible = !NamingBox.IsVisible;

        if (LauncherSelector.IsVisible && LauncherSelector.SelectedItem is Cache_DisplayLauncher display)
        {
            display.name = NamingBox.Text!;
            if(display.name != previousName)
            {
                clm.SaveLauncher(display.name);
            }
        }
    }

    public void UpdateCache(Cache_CustomLauncher newCache)
    {
        if(LauncherSelector.SelectedItem is Cache_DisplayLauncher display)
        {
            display.cache = newCache;
        }
    }
}