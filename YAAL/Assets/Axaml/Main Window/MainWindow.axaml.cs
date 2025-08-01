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
            CLMakerWindow.GetCLMakerWindow();
        };

        AddNewAsync.Click += (_, _) =>
        {
            AddAsync();
        };

        SettingsButton.Click += (_, _) =>
        {

        };


        foreach (var item in IOManager.GetAsyncList())
        {
            AddAsync(item);
        }
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