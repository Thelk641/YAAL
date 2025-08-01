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
    public MainWindow()
    {
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
            SettingManager manager = SettingManager.GetSettingsWindow(generalSettings, customSetting);
            manager.Closing += (_, _) =>
            {
                Dictionary<string, string> newCustomSetting;
                Dictionary<GeneralSettings, string> newGeneralSettings = manager.ParseSettings(out newCustomSetting);
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
    }

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void AddAsync()
    {
        Cache_Async cache = IOManager.CreateNewAsync("New async");
        AsyncContainer.Children.Add(new AsyncHolder(cache));
    }

    private void AddAsync(string asyncName)
    {
        AsyncContainer.Children.Add(new AsyncHolder(IOManager.GetAsync(asyncName)));
    }
}