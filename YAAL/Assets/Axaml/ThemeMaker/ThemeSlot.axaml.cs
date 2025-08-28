using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YAAL.Assets.Scripts;
using static YAAL.ApworldSettings;
using static YAAL.SlotSettings;
using static YAAL.AsyncSettings;
using System.Linq;
using System.Collections.ObjectModel;

namespace YAAL;

public partial class ThemeSlot : UserControl
{
    public Cache_DisplaySlot selectedSlot;
    public int baseHeight = 52;
    public int heightDifference = 38;
    
    public ThemeSlot()
    {
        InitializeComponent();
        ToolSelect.ItemsSource = new List<string> { "A tool" };
    }

    public void SwitchMode()
    {
        if (PlayMode.IsVisible)
        {
            PlayMode.IsVisible = false;
            EditMode.IsVisible = true;
            this.Height = baseHeight + heightDifference;
        } else
        {
            PlayMode.IsVisible = true;
            EditMode.IsVisible = false;
            this.Height = baseHeight;
        }
    }
}