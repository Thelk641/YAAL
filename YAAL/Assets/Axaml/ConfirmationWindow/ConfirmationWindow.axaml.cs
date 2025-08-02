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

public partial class ConfirmationWindow : Window
{
    public bool confirmed = false;
    public ConfirmationWindow()
    {
        InitializeComponent();
        Yes.Click += (source, args) =>
        {
            confirmed = true;
            this.Close();
        };

        No.Click += (source, args) =>
        {
            this.Close();
        };
    }

    public ConfirmationWindow(string toDisplay) : this()
    {
        TargetName.Text = toDisplay;
        this.IsVisible = true;
    }
}