using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using YAAL.Assets.Scripts;

namespace YAAL;

public partial class ColorSelector : Window
{
    public ColorSelector()
    {
        InitializeComponent();
        AutoTheme.SetTheme(BackgroundColor, ThemeSettings.backgroundColor);
    }

    public ColorSelector(string hex)
    {
        
        InitializeComponent();
        AutoTheme.SetTheme(BackgroundColor, ThemeSettings.backgroundColor);
        SetColor(hex);
        OK.Click += (_, _) =>
        {
            this.Close(true);
        };
        Cancel.Click += (_, _) =>
        {
            this.Close(false);
        };
    }

    public void SetColor(string hex)
    {
        View.Color = AutoColor.HexToColor(hex);
    }

    public static async Task<string?> PickColor(Window owner, string hex)
    {
        ColorSelector selector = new ColorSelector(hex);
        bool? result = await selector.ShowDialog<bool?>(owner);
        if (result != null && (bool)result)
        {
            return AutoColor.ColorToHex(selector.View.Color);
        }
        return null;
    }
}