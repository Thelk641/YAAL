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
        SetBackground();
    }

    public Error(string name, string content, string stackTrace)
    {
        InitializeComponent();
        SetBackground();
        ErrorName.Text = name;
        ErrorContent.Text = content;
        this.stackTrace = stackTrace;
    }

    public void SetBackground()
    {
        var theme = Application.Current.ActualThemeVariant;
        if (theme == ThemeVariant.Dark)
        {
            Background_0.Background = new SolidColorBrush(Color.Parse("#454545"));
            Background_1.Background = new SolidColorBrush(Color.Parse("#454545"));
        }
        else
        {
            Background_0.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_1.Background = new SolidColorBrush(Color.Parse("#AAA"));
        }
    }
}