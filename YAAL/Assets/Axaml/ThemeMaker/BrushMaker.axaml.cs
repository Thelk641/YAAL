using YAAL.Assets.Script.Cache;
using YAAL.Assets.Scripts;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static YAAL.LauncherSettings;
using static YAAL.SlotSettings;
using System.Numerics;

namespace YAAL;

public partial class BrushMaker : Window
{
    public BrushMaker()
    {
        InitializeComponent();
        AutoTheme.SetTheme(TrueBackground, ThemeSettings.backgroundColor);
        CenterPicker.ItemsSource = ThemeManager.GetCenterList();

        XOffsetRelative.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };
        XOffsetFixed.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };
        YOffsetRelative.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };
        YOffsetFixed.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };
        WidthRelative.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };
        WidthFixed.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };
        HeightRelative.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };
        HeightFixed.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };

    }

    public void Setup(Cached_BrushV2 brush)
    {
        if (brush.brushType == BrushType.Image)
        {
            SetupImage(brush);
        } else if (brush.brushType == BrushType.Color)
        {
            SetupColor(brush);
        }

        if (brush.xOffsetAbsolute)
        {
            SwitchRelativeAbsolute(XOffsetRelative);
        }

        if (brush.yOffsetAbsolute)
        {
            SwitchRelativeAbsolute(YOffsetRelative);
        }

        if (brush.widthAbsolute)
        {
            SwitchRelativeAbsolute(WidthRelative);
        }

        if (brush.heightAbsolute)
        {
            SwitchRelativeAbsolute(HeightRelative);
        }

        CenterPicker.SelectedItem = brush.center;
        CenterOffsetX.Text = brush.xOffset.ToString();
        CenterOffsetY.Text = brush.yOffset.ToString();
        BrushWidth.Text = brush.width.ToString();
        BrushHeight.Text = brush.height.ToString();
    }

    public void SetupImage(Cached_BrushV2 brush)
    {
        if (!(brush is Cached_ImageBrushV2))
        {
            ErrorManager.ThrowError(
                "BrushMaker - Invalid brush type",
                "Brush is of type Image  yet it's not an ImageBrush, this shouldn't happen, please report this issue.");
        }

        Cached_ImageBrushV2 image = brush as Cached_ImageBrushV2 ?? new Cached_ImageBrushV2();

        StretchMode.ItemsSource = Enum.GetValues(typeof(Stretch));
        FlipMode.ItemsSource = Enum.GetValues(typeof(FlipSettings));
        TileMode.ItemsSource = Enum.GetValues(typeof(TileMode));

        this.Height = 460;
        ColorMode.IsVisible = false;
        ImageMode.IsVisible = true;
        ImageSettings.IsVisible = true;
        ImageSource.Text = image.imageSource;
        StretchMode.SelectedItem = image.stretch;
        FlipMode.SelectedItem = image.flipSetting;
        TileMode.SelectedItem = image.tilemode;
        OpacitySlider.Value = image.opacity;
    }

    public void SetupColor(Cached_BrushV2 brush)
    {
        
        if (!(brush is Cached_SolidColorBrushV2))
        {
            ErrorManager.ThrowError(
                "BrushMaker - Invalid brush type",
                "Brush is of type Color yet it's not a SolidColorBrush, this shouldn't happen, please report this issue.");
        }

        Cached_SolidColorBrushV2 solid = brush as Cached_SolidColorBrushV2 ?? new Cached_SolidColorBrushV2();

        this.Height = 290;
        ColorMode.IsVisible = true;
        ImageMode.IsVisible = false;
        ImageSettings.IsVisible = false;
        ColorHolder.Background = new SolidColorBrush(solid.color);
    }

    public void SwitchRelativeAbsolute(Button source)
    {
        TextBox? toModify;
        Vector2 slotSize = WindowManager.GetSlotSize();
        bool toAbsolute = true;
        bool horizontal = true;

        switch (source.Name)
        {
            case "XOffsetRelative":
                toModify = CenterOffsetX;
                break;
            case "XOffsetFixed":
                toModify = CenterOffsetX;
                toAbsolute = false;
                break;
            case "YOffsetRelative":
                toModify = CenterOffsetY;
                horizontal = false;
                break;
            case "YOffsetFixed":
                toModify = CenterOffsetY;
                toAbsolute = false;
                horizontal = false;
                break;
            case "WidthRelative":
                toModify = BrushWidth;
                break;
            case "WidthFixed":
                toModify = BrushWidth;
                toAbsolute = false;
                break;
            case "HeightRelative":
                toModify = BrushHeight;
                horizontal = false;
                break;
            case "HeightFixed":
                toModify = BrushHeight;
                toAbsolute = false;
                horizontal = false;
                break;
            default:
                Debug.WriteLine("SwitchRelativeAbsolute called from button " + source.Name);
                return;
        }

        double originalValue = 0;
        double.TryParse(toModify.Text, out originalValue);
        double newValue;
        double baseValue;

        if (horizontal)
        {
            baseValue = slotSize.X;
        } else
        {
            baseValue = slotSize.Y;
        }

        if (toAbsolute)
        {
            // originalValue is a percentage
            newValue = originalValue * baseValue / 100;
        }
        else
        {
            // originalValue is a number of pixels
            newValue = originalValue / baseValue * 100;
        }

        toModify.Text = Math.Round(newValue, 2).ToString();
    }
}