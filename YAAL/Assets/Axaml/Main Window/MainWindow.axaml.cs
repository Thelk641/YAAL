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

public partial class MainWindow : Window
{
    List<AsyncHolder> waitingToClose = new List<AsyncHolder>();
    private bool readyToClose = false;

    public MainWindow()
    {
        BackgroundSetter.SetWindowBackground(this);
        InitializeComponent();
        CLMakerButton.Click += (_, _) =>
        {
            CLMakerWindow window = CLMakerWindow.GetCLMakerWindow();
            window.IsVisible = true;
            window.Closing += (_, _) =>
            {
                this.Topmost = true;
                this.Topmost = false;
            };
        };

        AddNewAsync.Click += (_, _) =>
        {
            AddAsync();
        };

        SettingsButton.Click += (_, _) =>
        {
            Dictionary<string, string> customSetting;
            Dictionary<GeneralSettings, string> generalSettings = IOManager.GetUserSettings(out customSetting);
            /*SettingManager manager = SettingManager.GetSettingsWindow(generalSettings, customSetting);
            manager.Closing += (_, _) =>
            {
                Dictionary<string, string> newCustomSetting;
                Dictionary<GeneralSettings, string> newGeneralSettings = manager.ParseSettings(out newCustomSetting);
                IOManager.SetUserSettings(newGeneralSettings, newCustomSetting);
                this.Topmost = true;
                this.Topmost = false;
            };
            manager.IsVisible = true;*/
            SettingManager manager = SettingManager.GetSettingsWindow(this, generalSettings, customSetting);
            manager.Closing += (_, _) =>
            {
                Dictionary<string, string> newCustomSetting;
                Dictionary<GeneralSettings, string> newGeneralSettings = manager.OutputGeneralSettings(out newCustomSetting);
                IOManager.SetUserSettings(newGeneralSettings, newCustomSetting);
                this.Topmost = true;
                this.Topmost = false;
            };
            manager.IsVisible = true;
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
}