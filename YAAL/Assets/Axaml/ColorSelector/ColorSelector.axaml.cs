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
        BackgroundSetter.SetBackground(BackgroundColor, GeneralSettings.foregroundColor);
    }

    public ColorSelector(string hex)
    {
        
        InitializeComponent();
        BackgroundSetter.SetBackground(BackgroundColor, GeneralSettings.foregroundColor);
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
        View.Color = HexToColor(hex);
    }

    public static async Task<string?> PickColor(Window owner, string hex)
    {
        ColorSelector selector = new ColorSelector(hex);
        bool? result = await selector.ShowDialog<bool?>(owner);
        if (result != null && (bool)result)
        {
            return ColorToHex(selector.View.Color);
        }
        return null;
    }


    public static Color HexToColor(string hex)
    {
        return Color.Parse(hex);
    }

    public static string ColorToHex(Color color)
    {
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}