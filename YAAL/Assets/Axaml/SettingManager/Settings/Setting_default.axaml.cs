using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;

namespace YAAL;

public partial class Setting_Default : Setting
{
    public Setting_Default()
    {
        InitializeComponent();
        background = BackgroundColor;
        SetBackground();
        settingName = this.FindControl<TextBlock>("SettingName");
        settingValue = this.FindControl<TextBlock>("Value");
    }
}