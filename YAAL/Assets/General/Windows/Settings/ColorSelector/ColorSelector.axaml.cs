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

public partial class ColorSelector : ScalableWindow
{
    public event Action? ChangedColor;
    public event Action? CancelSelection;
    public ColorSelector()
    {
        InitializeComponent();
        AutoTheme.SetTheme(BackgroundColor, ThemeSettings.backgroundColor);
        AutoTheme.SetTheme(View, ThemeSettings.foregroundColor);
        View.ColorChanged += (_, _) => ChangedColor?.Invoke();
    }

    public ColorSelector(string hex)
    {
        
        InitializeComponent();
        AutoTheme.SetTheme(BackgroundColor, ThemeSettings.backgroundColor);
        AutoTheme.SetTheme(View, ThemeSettings.foregroundColor);
        SetColor(hex);
        OK.Click += (_, _) =>
        {
            this.Close(true);
        };
        Cancel.Click += (_, _) =>
        {
            CancelSelection?.Invoke();
            this.Close(false);
        };
        View.ColorChanged += (_, _) => ChangedColor?.Invoke();
    }

    public void SetColor(string hex)
    {
        View.Color = AutoColor.HexToColor(hex);
    }

    public Color GetColor()
    {
        return View.Color;
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