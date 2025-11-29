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

public partial class ThemeSlot : UserControl
{
    public Cache_DisplaySlot selectedSlot;
    public Cache_CustomLauncher currentLauncher;
    public event Action<double,double> ChangedHeight;
    public int hardCodedHeight = 112;
    public double previousHeight = 0;
    public int topOffset = 0;
    public int bottomOffset = 0;

    private List<string> fakeItemList = new List<string>()
    {
        "1 - Sword",
        "2 - Oak's Parcel",
        "5 - Potion",
        "6 - Marines"
    };
    
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

        AutomaticPatchButton.Click += (_, _) =>
        {
            SwitchPatchMode();
        };

        ManualPatchButton.Click += (_, _) =>
        {
            SwitchPatchMode();
        };

        foreach (var item in fakeItemList)
        {
            TextBlock box = new TextBlock();
            box.Text = item;
            box.IsVisible = true;
            TrackerItemHolder.Children.Add(box);
            AutoTheme.SetTheme(box, ThemeSettings.transparent);
        }

        Cache_DisplaySlot display = new Cache_DisplaySlot();
        display.slotName = "A selected slot";
        display.isHeader = false;
        SlotSelector.ItemsSource = new List<Cache_DisplaySlot>() { display };
        SlotSelector.SelectedItem = display;
        Resize();
    }

    public void Resize()
    {
        Vector2 size = WindowManager.GetSlotSize();
        double newHeight = size.Y;

        newHeight += topOffset + bottomOffset;

        string combined = topOffset.ToString() + ",*," + bottomOffset.ToString();
        PlayEmptySpace.RowDefinitions = new RowDefinitions(combined);
        EditEmptySpace.RowDefinitions = new RowDefinitions(combined);

        this.Height = newHeight;
        ChangedHeight?.Invoke(previousHeight, newHeight);
        previousHeight = newHeight;
        this.Width = size.X;
    }

    public void Resize(int top, int bottom)
    {
        topOffset = top;
        bottomOffset = bottom;
        Resize();
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
                if(EditMode.Background is ImageBrush oldBackgroundBrush && oldBackgroundBrush.Source is Bitmap oldBackgroundBitmap)
                {
                    oldBackgroundBitmap.Dispose();
                }
                EditMode.Background = brush;
                break;
            case ThemeSettings.foregroundColor:
                if (PlayMode.Background is ImageBrush oldForegroundBrush && oldForegroundBrush.Source is Bitmap oldForegroundBitmap)
                {
                    oldForegroundBitmap.Dispose();
                }
                PlayMode.Background = brush;
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
}