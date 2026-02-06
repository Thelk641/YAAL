using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using YAAL.Assets.Scripts;
using static YAAL.ApworldSettings;
using static YAAL.AsyncSettings;
using static YAAL.SlotSettings;

namespace YAAL;

public partial class ThemeHolder : Window
{

    public ThemeHolder(bool isForeground)
    {
        InitializeComponent();

        Vector2 slotSize = WindowManager.GetSlotSize();
        this.Height = slotSize.Y;
        this.Width = slotSize.X;
    }

    public ThemeHolder(bool isForeground, int newTop, int newBottom)
    {
        InitializeComponent();
        AdjustSize(isForeground, newTop, newBottom);
    }

    public void AdjustSize(bool isForeground, int newTop, int newBottom)
    {
        Vector2 baseSize;
        if (isForeground)
        {
            baseSize = WindowManager.GetSlotForegroundSize();
            baseSize = new Vector2(baseSize.X, baseSize.Y - (int)Math.Round(4 * App.Settings.Zoom)); // shorter height to let theme bleed into the separation layer
        }
        else
        {
            baseSize = WindowManager.GetSlotSize();
            baseSize.Y += newTop + newBottom;
        }

        this.Height = baseSize.Y;
        this.Width = baseSize.X;
    }
}