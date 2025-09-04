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
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.IO;

namespace YAAL;

public partial class ThemeCollumn : UserControl
{
    public event Action UpdatedBrush;
    public ThemeSettings id;

    public ThemeCollumn()
    {
        InitializeComponent();

        StretchSetting.ItemsSource = new List<Stretch>() { Stretch.None, Stretch.Fill, Stretch.Uniform, Stretch.UniformToFill  };
        StretchSetting.SelectedIndex = 0;
        TileSetting.ItemsSource = new List<TileMode>() { TileMode.None, TileMode.Tile, TileMode.FlipX, TileMode.FlipXY, TileMode.FlipY };
        TileSetting.SelectedIndex = 0;
        FilePicker.Click += async (_, _) =>
        {
            ImageSource.Text = await IOManager.PickFile(this.FindAncestorOfType<Window>());
        };
        ModeSwitch.Click += (_, _) =>
        {
            SwitchMode();
        };
        ColorButton.Click += async (_, _) =>
        {
            string hex = AutoColor.ColorToHex((ColorButton.Background as SolidColorBrush).Color);
            var output = await ColorSelector.PickColor(this.FindAncestorOfType<Window>(), hex);

            if (output != null)
            {
                SetColor(new SolidColorBrush(AutoColor.HexToColor(output)));
                UpdatedBrush?.Invoke();
            }
        };

        ImageSource.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(
                () =>
                {
                    UpdatedBrush?.Invoke();
                },
                2);
        };

        StretchSetting.SelectionChanged += (_, _) =>
        {
            Debouncer.Debounce(
                () =>
                {
                    UpdatedBrush?.Invoke();
                },
                2);
        };

        TileSetting.SelectionChanged += (_, _) =>
        {
            Debouncer.Debounce(
                () =>
                {
                    UpdatedBrush?.Invoke();
                },
                2);
        };
    }

    public void Setup(string name, ThemeSettings newId)
    {
        categoryName.Text = name;
        id = newId;
    }

    public void SetImage(string source, Stretch stretchMode, TileMode tilemode)
    {
        ImageSource.Text = source;
        StretchSetting.SelectedItem = stretchMode;
        TileSetting.SelectedItem = tilemode;
    }

    public void SetColor(SolidColorBrush color)
    {
        (ColorButton.Background as SolidColorBrush).Color = color.Color;
        (ColorButton.Background as SolidColorBrush).Opacity = color.Opacity;
        if (AutoColor.NeedsWhite(color.Color))
        {
            ColorBorder.BorderBrush = new SolidColorBrush(Colors.White);
        } else
        {
            ColorBorder.BorderBrush = new SolidColorBrush(Colors.Black);
        }
    }
    public Cache_Brush GetBrush()
    {
        Cache_Brush output = new Cache_Brush();
        output.isImage = (ImageMode.IsVisible && ImageSource.Text != null && File.Exists(ImageSource.Text));
        output.imageSource = ImageSource.Text;


        if (StretchSetting.SelectedItem is Stretch stretch)
        {
            output.stretch = stretch;
        }
        else
        {
            output.stretch = Stretch.None;
        }

        if (TileSetting.SelectedItem is TileMode tilemode)
        {
            output.tilemode = tilemode;
        }
        else
        {
            output.tilemode = TileMode.None;
        }

        if(ColorButton.Background is SolidColorBrush solidColorBrush)
        {
            output.colorBrush = solidColorBrush;
        }

        return output;
    }

    public void SetBrush(Cache_Brush newBrush)
    {
        if(newBrush.colorBrush != null)
        {
            SetColor(newBrush.colorBrush);
        }
        if(newBrush.imageSource != null && newBrush.imageSource != "")
        {
            SetImage(newBrush.imageSource, newBrush.stretch, newBrush.tilemode);
        }
        if (newBrush.isImage)
        {
            SwitchMode();
        }
    }

    public void SwitchMode()
    {
        if (ImageMode.IsVisible)
        {
            ImageMode.IsVisible = false;
            ColorMode.IsVisible = true;
        } else
        {
            ImageMode.IsVisible = true;
            ColorMode.IsVisible = false;
        }
        UpdatedBrush?.Invoke();
    }
}
