using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YAAL.Assets.Scripts;

namespace YAAL;

public partial class BrushHolder : UserControl
{
    public Cached_Layer brush 
    { get {
            switch (Holder.Background)
            {
                case ImageBrush image:
                    return imageBrush;
                case SolidColorBrush solid:
                    return colorBrush;
            }

            return new Cached_ColorLayer();
        }
    }
    public event Action MoveUp;
    public event Action MoveDown;
    public event Action AskForRemoval;
    public event PropertyChangedEventHandler? BrushUpdated;

    public bool isForeground = false;
    public string type;
    public Cached_ImageLayer imageBrush;
    public Cached_ColorLayer colorBrush;
    public BrushHolder()
    {
        InitializeComponent();

        BrushOptions.SetValue(AutoTheme.AutoThemeProperty, null);

        Up.Click += (_, _) => MoveUp?.Invoke();
        Down.Click += (_, _) => MoveDown?.Invoke();
        Remove.Click += (_, _) => AskForRemoval?.Invoke();

        BrushOptions.Click += (_, _) =>
        {
            BrushMaker maker = new BrushMaker(isForeground, brush);
            maker.IsVisible = true;
            maker.SettingChanged += (_, property) =>
            {
                Setup(type, maker.brush);
                BrushUpdated?.Invoke(this, property);
                // Preview info : width = height * slotSize.Height / slotSize.Width
            };
        };

        //TODO : Update Preview
    }

    public void Setup(string type, Cached_Layer newBrush)
    {
        this.type = type;
        BrushType.Text = type;
        BrushOptions.Background = new SolidColorBrush(Colors.Transparent);
        Holder.Background = newBrush.GetLayer().Background;
        if(newBrush is Cached_ImageLayer image)
        {
            imageBrush = image;
        } else if (newBrush is Cached_ColorLayer color)
        {
            colorBrush = color;
        }
    }

    public void Setup(string type)
    {
        Cached_Layer layer;
        if(type == "Color")
        {
            layer = new Cached_ColorLayer();
        } else
        {
            layer = new Cached_ImageLayer();
        }

        Setup(type, layer);
    }

}