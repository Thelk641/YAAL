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

public partial class ErrorWindow : Window
{
    private Separator lastSeparator;
    public ErrorWindow()
    {
        InitializeComponent();
        AutoTheme.SetTheme(BackgroundColor, ThemeSettings.backgroundColor);
        this.IsVisible = false;
    }

    public void CloseCurrentError()
    {
        Separator newSeparator = new Separator();
        ErrorContainer.Children.Add(newSeparator);
        lastSeparator = newSeparator;
    }

    public void ShowError()
    {
        if(lastSeparator != null)
        {
            ErrorContainer.Children.Remove(lastSeparator);
        }
        this.IsVisible = true;
    }

    public void AddError(Error newError)
    {
        ErrorContainer.Children.Add(newError);
        CloseCurrentError();
    }

    public void AddError(Cache_ErrorList cache)
    {
        foreach (var item in cache.errors)
        {
            Error newError = new Error(item.name, item.content, item.stackTrace);
            AddError(newError);
        }
    }

    
}