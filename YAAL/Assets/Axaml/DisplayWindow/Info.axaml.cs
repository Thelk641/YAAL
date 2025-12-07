using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using YAAL.Assets.Scripts;

namespace YAAL;

public partial class Info : UserControl
{
    public Info()
    {
        InitializeComponent();
    }

    public Info(string newTag, string newValue)
    {
        InitializeComponent();
        Tag.Text = newTag;
        Copy.Content = newValue;
        Copy.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(this)?.Clipboard is IClipboard clipboard)
            {
                await clipboard.SetTextAsync(newValue);
            }
        };
    }
}