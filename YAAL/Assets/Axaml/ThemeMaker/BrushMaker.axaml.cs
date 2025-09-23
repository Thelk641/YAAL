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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using YAAL.Assets.Script.Cache;
using YAAL.Assets.Scripts;
using static System.Net.Mime.MediaTypeNames;
using static YAAL.BrushEvents;
using static YAAL.LauncherSettings;
using static YAAL.SlotSettings;

namespace YAAL;

public partial class BrushMaker : Window
{
    public Cached_Layer brush;
    public event PropertyChangedEventHandler? SettingChanged;
    public BrushMaker()
    {
        InitializeComponent();
        AutoTheme.SetTheme(TrueBackground, ThemeSettings.backgroundColor);
        CenterPicker.ItemsSource = ThemeManager.GetCenterList();
    }

    public void SetEvents()
    {
        XOffsetRelative.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };
        XOffsetFixed.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };
        YOffsetRelative.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };
        YOffsetFixed.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };
        WidthRelative.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };
        WidthFixed.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };
        HeightRelative.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };
        HeightFixed.Click += (source, _) => { SwitchRelativeAbsolute((Button)source); };

        CenterPicker.SelectionChanged += (_, _) =>
        {
            brush.center = CenterPicker.SelectedItem!.ToString()!;
            RaiseEvent(Center);
        };

        CenterOffsetX.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(
                () =>
                {
                    if(double.TryParse(CenterOffsetX.Text, out double newDouble) && newDouble != brush.xOffset)
                    {
                        brush.xOffset = newDouble;
                        RaiseEvent(XOffset);
                    }
                },
                0.5f);
        };

        CenterOffsetY.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(
                () =>
                {
                    if (double.TryParse(CenterOffsetY.Text, out double newDouble) && newDouble != brush.yOffset)
                    {
                        brush.yOffset = newDouble;
                        RaiseEvent(YOffset);
                    }
                },
                0.5f);
        };

        BrushWidth.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(
                () =>
                {
                    if (double.TryParse(BrushWidth.Text, out double newDouble) && newDouble != brush.width)
                    {
                        brush.width = newDouble;
                        RaiseEvent(BrushEvents.Width);
                    }
                },
                0.5f);
        };

        BrushHeight.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(
                () =>
                {
                    if (double.TryParse(BrushHeight.Text, out double newDouble) && newDouble != brush.height)
                    {
                        brush.height = newDouble;
                        RaiseEvent(BrushEvents.Height);
                    }
                },
                0.5f);
        };
    }

    public void Setup(Cached_Layer newBrush)
    {
        brush = newBrush;
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
        SetEvents();
    }

    public void SetupImage(Cached_Layer brush)
    {
        if (!(brush is Cached_ImageLayer))
        {
            ErrorManager.ThrowError(
                "BrushMaker - Invalid brush type",
                "Brush is of type Image  yet it's not an ImageBrush, this shouldn't happen, please report this issue.");
        }

        Cached_ImageLayer image = brush as Cached_ImageLayer ?? new Cached_ImageLayer();

        StretchMode.ItemsSource = Enum.GetValues(typeof(Stretch));
        FlipMode.ItemsSource = Enum.GetValues(typeof(FlipSettings));
        TileMode.ItemsSource = Enum.GetValues(typeof(TileMode));

        WindowManager.ChangeHeight(this, 460);
        ColorMode.IsVisible = false;
        ImageMode.IsVisible = true;
        ImageSettings.IsVisible = true;
        ImageSource.Text = image.imageSource;
        StretchMode.SelectedItem = image.stretch;
        FlipMode.SelectedItem = image.flipSetting;
        TileMode.SelectedItem = image.tilemode;
        OpacitySlider.Value = image.opacity;

        StretchMode.SelectionChanged += (_, _) =>
        {
            if(Enum.TryParse<Stretch>(StretchMode.SelectedItem.ToString(), out Stretch newStretch))
            {
                image.stretch = newStretch;
                RaiseEvent(BrushEvents.StretchMode);
            }
        };

        FlipMode.SelectionChanged += (_, _) =>
        {
            if (Enum.TryParse<FlipSettings>(StretchMode.SelectedItem.ToString(), out FlipSettings newFlip))
            {
                image.flipSetting = newFlip;
                RaiseEvent(BrushEvents.FlipMode);
            }
        };

        TileMode.SelectionChanged += (_, _) =>
        {
            if (Enum.TryParse<TileMode>(StretchMode.SelectedItem.ToString(), out TileMode newTile))
            {
                image.tilemode = newTile;
                RaiseEvent(BrushEvents.TileMode);
            }
        };

        OpacitySlider.ValueChanged += (_, _) =>
        {
            image.opacity = OpacitySlider.Value;
            RaiseEvent(BrushEvents.Opacity);
        };

        ImageSource.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(
                () =>
                    {
                        string newPath = ThemeManager.AddNewImage(ImageSource.Text);
                        if(newPath != image.imageSource)
                        {
                            ImageSource.Text = newPath;
                            image.imageSource = newPath;
                            RaiseEvent(BrushEvents.ImageSource);
                        }
                    },
                0.5f);
        };
    }

    public void SetupColor(Cached_Layer brush)
    {
        
        if (!(brush is Cache_ColorLayer))
        {
            ErrorManager.ThrowError(
                "BrushMaker - Invalid brush type",
                "Brush is of type Color yet it's not a SolidColorBrush, this shouldn't happen, please report this issue.");
        }

        Cache_ColorLayer solid = brush as Cache_ColorLayer ?? new Cache_ColorLayer();

        WindowManager.ChangeHeight(this, 290);
        ColorMode.IsVisible = true;
        ImageMode.IsVisible = false;
        ImageSettings.IsVisible = false;
        ColorHolder.Background = new SolidColorBrush(solid.color);

        ColorHolder.Click += async (_, _) =>
        {
            var output = await ColorSelector.PickColor(this.FindAncestorOfType<Window>()!, AutoColor.ColorToHex((ColorHolder.Background as SolidColorBrush)!.Color));

            if (output != null)
            {
                var newColor = AutoColor.HexToColor(output);
                ColorHolder.Background = new SolidColorBrush(newColor);
                solid.color = newColor;
                RaiseEvent(BrushColor);
            }
        };
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

    public void RaiseEvent(BrushEvents eventName)
    {
        SettingChanged?.Invoke(this, new PropertyChangedEventArgs(eventName.ToString()));
    }
}