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
        ColorMode.IsVisible = false;
        ImageMode.IsVisible = true;
    }

    public void SetColor(SolidColorBrush color)
    {
        ColorButton.Background = color;
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
        output.isImage = (ImageSource.IsVisible && ImageSource.Text != null && File.Exists(ImageSource.Text));
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
}