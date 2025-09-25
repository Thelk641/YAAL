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
                    Cached_ColorLayer solidBrush = new Cached_ColorLayer();
                    solidBrush.color = solid.Color;
                    return solidBrush;
            }

            return new Cached_ColorLayer();
        }
    }
    public event Action MoveUp;
    public event Action MoveDown;
    public event Action AskForRemoval;
    public event PropertyChangedEventHandler? BrushUpdated;

    public IBrush temporaryBrush { get { return Holder.Background!; } }
    public string type;
    public Cached_ImageLayer imageBrush;
    public BrushHolder()
    {
        InitializeComponent();

        Up.Click += (_, _) => MoveUp?.Invoke();
        Down.Click += (_, _) => MoveDown?.Invoke();
        Remove.Click += (_, _) => AskForRemoval?.Invoke();

        BrushOptions.Click += (_, _) =>
        {
            BrushMaker maker = new BrushMaker(brush);
            maker.SettingChanged += (_, property) =>
            {
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

        //brush = newBrush;
    }

}