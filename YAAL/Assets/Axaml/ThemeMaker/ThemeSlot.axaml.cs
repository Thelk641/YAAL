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
        ToolSelect.SelectedIndex = 0;

        SlotSelector.ItemsSource = new List<string> { "My slot n°1" };
        SlotSelector.SelectedIndex = 0;

        SelectedLauncher.ItemsSource = new List<string> { "An amazing game" };
        SelectedLauncher.SelectedIndex = 0;

        SelectedVersion.ItemsSource = new List<string> { "Version 641" };
        SelectedVersion.SelectedIndex = 0;

        AutoTheme.SetTheme(Transparent, ThemeSettings.transparent);

        AutomaticPatchButton.Click += (_, _) =>
        {
            SwitchPatchMode();
        };

        ManualPatchButton.Click += (_, _) =>
        {
            SwitchPatchMode();
        };
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

    public void SwitchPatchMode()
    {
        if (AutomaticPatch.IsVisible)
        {
            AutomaticPatch.IsVisible = false;
            SlotSelector.IsVisible = false;
            SlotName.IsVisible = true;
            ManualPatch.IsVisible = true;
            AutomaticPatchButton.IsVisible = true;
            ManualPatchButton.IsVisible = false;
        }
        else
        {
            AutomaticPatch.IsVisible = true;
            SlotSelector.IsVisible = true;
            SlotName.IsVisible = false;
            ManualPatch.IsVisible = false;
            AutomaticPatchButton.IsVisible = false;
            ManualPatchButton.IsVisible = true;
        }
    }
}