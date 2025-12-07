using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using YAAL.Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace YAAL;

public partial class DisplayInfo : UserControl
{
    public DisplayInfo()
    {
        InitializeComponent();
    }

    public DisplayInfo(Command_Display source, string newTag, string newValue)
    {
        InitializeComponent();
        Remove.Click += (_, _) => { source.RequestRemoval(this); };
        Tag.Text = newTag;
        Value.Text = newValue;
    }

    public string GetTag()
    {
        return Tag.Text ?? "";
    }

    public string GetValue()
    {
        return Value.Text ?? "";
    }
}