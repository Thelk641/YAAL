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

public partial class Error : UserControl
{
    private string stackTrace;
    public Error()
    {
        InitializeComponent();
        AutoTheme.SetTheme(TrueBackground, ThemeSettings.transparent);
    }

    public Error(string name, string content, string stackTrace)
    {
        InitializeComponent();
        AutoTheme.SetTheme(TrueBackground, ThemeSettings.transparent);
        ErrorName.Text = name;
        ErrorContent.Text = content;
        this.stackTrace = stackTrace;
    }
}