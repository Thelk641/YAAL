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
    public event Action BrushUpdated;

    public IBrush temporaryBrush { get { return Holder.Background!; } }
    public string type;
    public Cached_ImageLayer imageBrush;
    public BrushHolder()
    {
        InitializeComponent();

        Up.Click += (_, _) => MoveUp?.Invoke();
        Down.Click += (_, _) => MoveDown?.Invoke();
        Remove.Click += (_, _) => AskForRemoval?.Invoke();

        //Todo : 
        BrushOptions.Click += (_, _) => OpenEditWindow();
        //Update Preview
    }

    public void Setup(string type, Cached_Layer newBrush)
    {
        this.type = type;
        BrushType.Text = type;
        BrushOptions.Background = new SolidColorBrush(Colors.Transparent);
        Holder.Background = newBrush.GetLayer().Background;

        //brush = newBrush;
    }

    public void OpenEditWindow()
    {
        switch (type)
        {
            case ("Color"):
                IBrush previousBackground = Holder.Background!;
                ColorSelector selector = new ColorSelector();
                selector.IsVisible = true;
                selector.ChangedColor += () =>
                {
                    if(Holder.Background is SolidColorBrush solid)
                    {
                        solid.Color = selector.GetColor();
                        BrushUpdated?.Invoke();
                    }
                };

                selector.CancelSelection += () =>
                {
                    Holder.Background = previousBackground;
                    BrushUpdated?.Invoke();
                };
                break;
            case ("Image"):
                break;
        }
    }
}