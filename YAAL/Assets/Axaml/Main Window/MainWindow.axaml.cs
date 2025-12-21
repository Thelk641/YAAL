using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using YAAL.Assets.Scripts;

namespace YAAL;

public partial class MainWindow : ScalableWindow
{
    List<AsyncHolder> waitingToClose = new List<AsyncHolder>();
    private bool readyToClose = false;

    public MainWindow()
    {
        InitializeComponent();
        AutoTheme.SetTheme(BackgroundColor, ThemeSettings.backgroundColor);
        AutoTheme.SetTheme(AsyncBackground, ThemeSettings.backgroundColor);
        CLMakerButton.Click += (_, _) =>
        {
            WindowManager.OpenWindow(WindowType.CLMaker, this);
        };

        AddNewAsync.Click += (_, _) =>
        {
            AddAsync();
        };

        SettingsButton.Click += (_, _) =>
        {
            Dictionary<string, string> customSetting;
            Dictionary<GeneralSettings, string> generalSettings = IOManager.GetUserSettings(out customSetting);
            string allowMulti = "";
            if (generalSettings.ContainsKey(GeneralSettings.allowMultislot))
            {
                allowMulti = generalSettings[GeneralSettings.allowMultislot];
            }

            SettingManager manager = SettingManager.GetSettingsWindow(this, generalSettings, customSetting);
            manager.Closing += (_, _) =>
            {
                Dictionary<string, string> newCustomSetting;
                Dictionary<GeneralSettings, string> newGeneralSettings = manager.OutputGeneralSettings(out newCustomSetting);
                IOManager.SetUserSettings(newGeneralSettings, newCustomSetting);
                this.Topmost = true;
                this.Topmost = false;
                if(newGeneralSettings.ContainsKey(GeneralSettings.zoom) && float.TryParse(newGeneralSettings[GeneralSettings.zoom], out float newZoom))
                {
                    App.Settings.Zoom = newZoom;
                }

                if(newGeneralSettings.ContainsKey(GeneralSettings.allowMultislot) && newGeneralSettings[GeneralSettings.allowMultislot] != allowMulti)
                {
                    foreach (var item in AsyncContainer.Children)
                    {
                        if(item is AsyncHolder holder)
                        {
                            holder.UpdateSlotSelection();
                        }
                    }
                }
            };
            manager.IsVisible = true;
        };

        ThemeButton.Click += (_, _) =>
        {
            WindowManager.OpenWindow(WindowType.CustomThemeMaker, this);
        };

        UpdateButton.Click += (_, _) =>
        {
            WindowManager.OpenWindow(WindowType.UpdateWindow, this);
        };


        foreach (var item in IOManager.GetAsyncList())
        {
            AddAsync(item);
        }

        this.Closing += (_, e) =>
        {
            if (readyToClose || AsyncContainer.Children.Count == 0)
            {
                return;
            }

            this.IsVisible = false;
            foreach (var item in AsyncContainer.Children)
            {
                if(item is AsyncHolder holder)
                {
                    holder.ClosingSave();
                    if (holder.isParsingUrl)
                    {
                        waitingToClose.Add(holder);
                        holder.DoneClosing += () =>
                        {
                            waitingToClose.Remove(holder);
                            if (waitingToClose.Count == 0)
                            {
                                readyToClose = true;
                                this.Close();
                            }
                        };
                    }
                }
            }

            if (waitingToClose.Count > 0)
            {
                e.Cancel = true;
            }
        };
    }

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void AddAsync()
    {
        Cache_Async cache = IOManager.CreateNewAsync("New async");
        AsyncHolder holder = AddAsync(cache);
        holder.SwitchMode();
    }

    private void AddAsync(string asyncName)
    {
        Cache_Async cache = IOManager.GetAsync(asyncName);
        if(cache.settings.ContainsKey(AsyncSettings.isTemporary) && cache.settings[AsyncSettings.isTemporary] == true.ToString())
        {
            IOManager.DeleteAsync(asyncName);
            return;
        }

        if (cache.settings[AsyncSettings.isHidden] != true.ToString())
        {
            AddAsync(cache);
        }
    }

    private AsyncHolder AddAsync(Cache_Async cache)
    {
        AsyncHolder holder = new AsyncHolder(cache);
        AsyncContainer.Children.Add(holder);
        holder.RequestRemoval += () =>
        {
            AsyncContainer.Children.Remove(holder);
        };
        return holder;
    }

    public void UpdateToolList()
    {
        foreach (var item in AsyncContainer.Children)
        {
            if(item is AsyncHolder async)
            {
                async.UpdateToolList();
            }
        }
    }
}