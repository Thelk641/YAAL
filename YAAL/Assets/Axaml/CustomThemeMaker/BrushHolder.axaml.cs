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
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YAAL.Assets.Scripts;

namespace YAAL;

public partial class BrushHolder : UserControl
{
    public Cached_Brush brush;
    public event Action MoveUp;
    public event Action MoveDown;
    public event Action AskForRemoval;
    public BrushHolder()
    {
        InitializeComponent();

        Up.Click += (_, _) => MoveUp?.Invoke();
        Down.Click += (_, _) => MoveDown?.Invoke();
        Remove.Click += (_, _) => AskForRemoval?.Invoke();

        //Todo : 
        //BrushOptions.Click +=
        //Update Preview
    }

    public void Setup(string type, Cached_Brush newBrush)
    {
        BrushType.Text = type;
        BrushOptions.Background = new SolidColorBrush(Colors.Transparent);
        Holder.Background = new SolidColorBrush(Colors.Transparent);
        Holder.Children.Add(newBrush.GetLayer());

        brush = newBrush;
    }
}