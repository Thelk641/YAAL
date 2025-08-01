using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using YAAL.Assets.Scripts;

namespace YAAL;

public partial class Setting_Tool : UserControl
{
    public Setting_Tool()
    {
        InitializeComponent();
        BackgroundSetter.SetBackground(BackgroundColor);
    }
}