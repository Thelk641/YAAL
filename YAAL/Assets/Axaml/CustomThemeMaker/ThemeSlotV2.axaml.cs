using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YAAL.Assets.Scripts;
using static YAAL.ApworldSettings;
using static YAAL.AsyncSettings;
using static YAAL.SlotSettings;

namespace YAAL;

public partial class ThemeSlotV2 : UserControl
{
    public Cache_DisplaySlot selectedSlot;
    public Cache_CustomLauncher currentLauncher;
    public event Action<double,double> ChangedHeight;
    public int hardCodedHeight = 52;
    public double previousHeight = 0;
    public int topOffset = 0;
    public int bottomOffset = 0;
    
    public ThemeSlotV2()
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

        AutomaticPatchButton.Click += (_, _) =>
        {
            SwitchPatchMode();
        };

        ManualPatchButton.Click += (_, _) =>
        {
            SwitchPatchMode();
        };
    }

    public void Resize()
    {
        double newHeight = hardCodedHeight;

        newHeight += topOffset + bottomOffset;

        string combined = topOffset.ToString() + ",*," + bottomOffset.ToString();
        PlayEmptySpace.RowDefinitions = new RowDefinitions(combined);
        EditEmptySpace1.RowDefinitions = new RowDefinitions(combined);
        EditEmptySpace2.RowDefinitions = new RowDefinitions(combined);

        if (EditMode.IsVisible)
        {
            newHeight += hardCodedHeight + 8; // one more line, plus spacing
        }

        this.Height = newHeight;
        ChangedHeight?.Invoke(previousHeight, newHeight);
        previousHeight = newHeight;
    }

    public void SwitchMode()
    {
        if (PlayMode.IsVisible)
        {
            PlayMode.IsVisible = false;
            EditMode.IsVisible = true;
        }
        else
        {
            PlayMode.IsVisible = true;
            EditMode.IsVisible = false;
        }
        Resize();
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

    public void SetTheme(ThemeSettings category, IBrush brush)
    {
        Transparent1.Background = new SolidColorBrush(Colors.Transparent);
        switch (category)
        {
            case ThemeSettings.backgroundColor:
                EditMode.Background = brush;
                break;
            case ThemeSettings.foregroundColor:
                PlayMode.Background = brush;
                EditRow1.Background = brush;
                EditRow2.Background = brush;
                break;
            case ThemeSettings.buttonColor:
                RealPlay.Background = brush;
                StartTool.Background = brush;
                Edit.Background = brush;
                FakePlay.Background = brush;
                PatchSelect.Background = brush;
                DownloadPatch.Background = brush;
                ReDownloadPatch.Background = brush;
                DoneEditing.Background = brush;
                ManualPatchButton.Background = brush;
                AutomaticPatchButton.Background = brush;
                DeleteSlot.Background = brush;

                SetComboBoxBackground(ToolSelect, brush);
                SetComboBoxBackground(SlotSelector, brush);
                SetComboBoxBackground(SelectedLauncher, brush);
                SetComboBoxBackground(SelectedVersion, brush);
                break;
        }
    }

    public void SetComboBoxBackground(ComboBox comboBox, IBrush brush)
    {
        comboBox.Background = brush;
        if (comboBox.GetVisualDescendants().OfType<ContentPresenter>().FirstOrDefault() is ContentPresenter presenter)
        {
            if (brush is SolidColorBrush solid)
            {
                presenter.Background = new SolidColorBrush(AutoColor.Darken(solid.Color));
            }
        }
    }

    public void SetOffset(int top, int bottom)
    {
        topOffset = top;
        bottomOffset = bottom;
        Resize();
    }
}