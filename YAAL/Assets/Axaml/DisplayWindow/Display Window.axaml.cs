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
using Avalonia.Input.Platform;

namespace YAAL;

public partial class DisplayWindow : ScalableWindow
{
    public DisplayWindow()
    {
        InitializeComponent();
        AutoTheme.SetTheme(BackgroundColor, ThemeSettings.backgroundColor);
    }

    public void AddInfo(string tag, string info)
    {
        Info toAdd = new Info(tag, info);
        InfoContainer.Children.Add(toAdd);
    }
}