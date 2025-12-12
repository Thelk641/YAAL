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
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using YAAL.Assets.Script.Cache;
using YAAL.Assets.Scripts;
using static System.Net.Mime.MediaTypeNames;
using static YAAL.BrushEvents;
using static YAAL.LauncherSettings;
using static YAAL.SlotSettings;

namespace YAAL;

public partial class BrushMaker : ScalableWindow
{
    public Cached_Layer brush;
    public event PropertyChangedEventHandler? SettingChanged;
    public bool isForeground;
    public string themeName;
    public BrushMaker()
    {
        InitializeComponent();
        AutoTheme.SetTheme(TrueBackground, ThemeSettings.backgroundColor);
        CenterPicker.ItemsSource = ThemeManager.GetCenterList();
    }

    public BrushMaker(bool shouldBeForeground, Cached_Layer newBrush)
    {
        InitializeComponent();
        isForeground = shouldBeForeground;
        AutoTheme.SetTheme(TrueBackground, ThemeSettings.backgroundColor);
        CenterPicker.ItemsSource = ThemeManager.GetCenterList();

        Setup(newBrush);
    }

    public void SetEvents()
    {
        XOffsetRelative.Click += (source, _) => { SwitchRelativeAbsolute((Button)source!); };
        XOffsetFixed.Click += (source, _) => { SwitchRelativeAbsolute((Button)source!); };
        YOffsetRelative.Click += (source, _) => { SwitchRelativeAbsolute((Button)source!); };
        YOffsetFixed.Click += (source, _) => { SwitchRelativeAbsolute((Button)source!); };
        WidthRelative.Click += (source, _) => { SwitchRelativeAbsolute((Button)source!); };
        WidthFixed.Click += (source, _) => { SwitchRelativeAbsolute((Button)source!); };
        HeightRelative.Click += (source, _) => { SwitchRelativeAbsolute((Button)source!); };
        HeightFixed.Click += (source, _) => { SwitchRelativeAbsolute((Button)source!); };

        CenterPicker.SelectionChanged += (_, _) =>
        {
            if(CenterPicker.SelectedItem is Combo_Centers newCenter)
            {
                brush.center = newCenter.centerName;
                RaiseEvent(Center);
            }
        };

        CenterOffsetX.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(
                () =>
                {
                    if(CenterOffsetX.Text != null && double.TryParse(CenterOffsetX.Text.Replace(",", "."), out double newDouble) && newDouble != brush.xOffset)
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
                    if (CenterOffsetY.Text != null && double.TryParse(CenterOffsetY.Text.Replace(",", "."), out double newDouble) && newDouble != brush.yOffset)
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
                    string toParse = BrushWidth.Text.Replace(",", ".");
                    double parsed = 0;
                    double.TryParse(toParse, CultureInfo.InvariantCulture.NumberFormat, out parsed);
                    if (BrushWidth.Text != null && double.TryParse(BrushWidth.Text.Replace(",", "."), CultureInfo.InvariantCulture.NumberFormat, out double newDouble) && newDouble != brush.width)
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
                    if (BrushHeight.Text != null && double.TryParse(BrushHeight.Text.Replace(",", "."), CultureInfo.InvariantCulture.NumberFormat, out double newDouble) && newDouble != brush.height)
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

        for (int i = 0; i < CenterPicker.ItemCount; i++)
        {
            if (CenterPicker.Items[i] is Combo_Centers center && center.centerName == brush.center)
            {
                CenterPicker.SelectedIndex = i;
                break;
            }
        }

        
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

        if (image.absoluteImageHeight)
        {
            SwitchRelativeAbsolute(ImageHeightRelative);
        }

        if (image.absoluteImageWidth)
        {
            SwitchRelativeAbsolute(ImageWidthRelative);
        }

        StretchMode.ItemsSource = Enum.GetValues(typeof(Stretch));
        FlipMode.ItemsSource = Enum.GetValues(typeof(FlipSettings));
        TileMode.ItemsSource = Enum.GetValues(typeof(TileMode));

        WindowManager.ChangeHeight(this, 536);
        ColorMode.IsVisible = false;
        ImageMode.IsVisible = true;
        ImageSettings.IsVisible = true;
        ImageSource.Text = image.imageSource;
        ImageWidth.Text = image.imageWidth.ToString();
        ImageHeight.Text = image.imageHeight.ToString();
        StretchMode.SelectedItem = image.stretch;
        FlipMode.SelectedItem = image.flipSetting;
        TileMode.SelectedItem = image.tilemode;
        OpacitySlider.Value = image.opacity * 100;

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
            if (Enum.TryParse<FlipSettings>(FlipMode.SelectedItem.ToString(), out FlipSettings newFlip))
            {
                image.flipSetting = newFlip;
                RaiseEvent(BrushEvents.FlipMode);
            }
        };

        TileMode.SelectionChanged += (_, _) =>
        {
            if (Enum.TryParse<TileMode>(TileMode.SelectedItem.ToString(), out TileMode newTile))
            {
                image.tilemode = newTile;
                RaiseEvent(BrushEvents.TileMode);
            }
        };

        OpacitySlider.ValueChanged += (_, _) =>
        {
            image.opacity = OpacitySlider.Value / 100;
            RaiseEvent(BrushEvents.Opacity);
        };

        ImageSource.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(
                () =>
                    {
                        if(ImageSource.Text == null)
                        {
                            return;
                        }
                        string newPath = ThemeManager.AddNewImage(ImageSource.Text, themeName);
                        if(newPath != image.imageSource)
                        {
                            ImageSource.Text = newPath;
                            image.imageSource = IOManager.ProcessLocalPath(newPath);
                            RaiseEvent(BrushEvents.ImageSource);
                        }
                    },
                0.5f);
        };

        ImageSelect.Click += async (_, _) =>
        {
            ImageSource.Text = await IOManager.PickFile(this);
        };

        ImageHeight.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(
                () =>
                {
                if (ImageHeight.Text != null && float.TryParse(ImageHeight.Text.Replace(",","."), CultureInfo.InvariantCulture.NumberFormat, out float rawImageHeight) && rawImageHeight != image.imageHeight)
                    {
                        image.imageHeight = rawImageHeight;
                        RaiseEvent(BrushEvents.ImageHeight);
                    }
                },
                0.5f
                );
        };

        ImageHeightFixed.Click += (source, _) => { SwitchRelativeAbsolute((source as Button)!); };
        ImageHeightRelative.Click += (source, _) => { SwitchRelativeAbsolute((source as Button)!); };

        ImageWidth.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(
                () =>
                {
                    if(ImageWidth.Text != null &&float.TryParse(ImageWidth.Text.Replace(",", "."), CultureInfo.InvariantCulture.NumberFormat, out float rawImageWidth) && rawImageWidth != image.imageWidth)
                    {
                        image.imageWidth = rawImageWidth;
                        RaiseEvent(BrushEvents.ImageWidth);
                    }
                },
                0.5f
                );
        };

        ImageWidthFixed.Click += (source, _) => { SwitchRelativeAbsolute((source as Button)!); };
        ImageWidthRelative.Click += (source, _) => { SwitchRelativeAbsolute((source as Button)!); };
    }

    public void SetupColor(Cached_Layer brush)
    {
        
        if (!(brush is Cached_ColorLayer))
        {
            ErrorManager.ThrowError(
                "BrushMaker - Invalid brush type",
                "Brush is of type Color yet it's not a SolidColorBrush, this shouldn't happen, please report this issue.");
        }

        Cached_ColorLayer solid = brush as Cached_ColorLayer ?? new Cached_ColorLayer();

        WindowManager.ChangeHeight(this, 290);
        ColorMode.IsVisible = true;
        ImageMode.IsVisible = false;
        ImageSettings.IsVisible = false;
        ColorHolder.Background = new SolidColorBrush(solid.color);

        ColorHolder.Click += async (_, _) =>
        {
            var output = await ColorSelector.PickColor(this, AutoColor.ColorToHex((ColorHolder.Background as SolidColorBrush)!.Color));

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
        Vector2 slotSize;
        if(isForeground)
        {
            slotSize = WindowManager.GetSlotForegroundSize();
        } else
        {
            slotSize = WindowManager.GetSlotSize();
        }

        bool toAbsolute = true;
        bool horizontal = true;

        switch (source.Name)
        {
            case "XOffsetRelative":
                XOffsetRelative.IsVisible = false;
                XOffsetFixed.IsVisible = true;
                toModify = CenterOffsetX;
                brush.xOffsetAbsolute = true;
                break;
            case "XOffsetFixed":
                XOffsetRelative.IsVisible = true;
                XOffsetFixed.IsVisible = false;
                toModify = CenterOffsetX;
                toAbsolute = false;
                brush.xOffsetAbsolute = false;
                break;
            case "YOffsetRelative":
                YOffsetRelative.IsVisible = false;
                YOffsetFixed.IsVisible = true;
                toModify = CenterOffsetY;
                horizontal = false;
                brush.yOffsetAbsolute = true;
                break;
            case "YOffsetFixed":
                YOffsetRelative.IsVisible = true;
                YOffsetFixed.IsVisible = false;
                toModify = CenterOffsetY;
                toAbsolute = false;
                horizontal = false;
                brush.yOffsetAbsolute = false;
                break;
            case "WidthRelative":
                WidthRelative.IsVisible = false;
                WidthFixed.IsVisible = true;
                toModify = BrushWidth;
                brush.widthAbsolute = true;
                break;
            case "WidthFixed":
                WidthRelative.IsVisible = true;
                WidthFixed.IsVisible = false;
                toModify = BrushWidth;
                toAbsolute = false;
                brush.widthAbsolute = false;
                break;
            case "HeightRelative":
                HeightRelative.IsVisible = false;
                HeightFixed.IsVisible = true;
                toModify = BrushHeight;
                horizontal = false;
                brush.heightAbsolute = true;
                break;
            case "HeightFixed":
                HeightRelative.IsVisible = true;
                HeightFixed.IsVisible = false;
                toModify = BrushHeight;
                toAbsolute = false;
                horizontal = false;
                brush.heightAbsolute = false;
                break;
            case "ImageHeightFixed":
                ImageHeightFixed.IsVisible = false;
                ImageHeightRelative.IsVisible = true;
                toModify = ImageHeight;
                toAbsolute = false;
                horizontal = false;
                (brush as Cached_ImageLayer)!.absoluteImageHeight = false;
                break;
            case "ImageHeightRelative":
                ImageHeightFixed.IsVisible = true;
                ImageHeightRelative.IsVisible = false;
                toModify = ImageHeight;
                toAbsolute = true;
                horizontal = false;
                (brush as Cached_ImageLayer)!.absoluteImageHeight = true;
                break;
            case "ImageWidthFixed":
                ImageWidthFixed.IsVisible = false;
                ImageWidthRelative.IsVisible = true;
                toModify = ImageWidth;
                toAbsolute = false;
                horizontal = true;
                (brush as Cached_ImageLayer)!.absoluteImageWidth = false;
                break;
            case "ImageWidthRelative":
                ImageWidthFixed.IsVisible = true;
                ImageWidthRelative.IsVisible = false;
                toModify = ImageWidth;
                toAbsolute = true;
                horizontal = true;
                (brush as Cached_ImageLayer)!.absoluteImageWidth = true;
                break;
            default:
                Trace.WriteLine("SwitchRelativeAbsolute called from button " + source.Name);
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