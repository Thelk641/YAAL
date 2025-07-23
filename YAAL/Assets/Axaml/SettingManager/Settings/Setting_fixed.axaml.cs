using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;

namespace YAAL;

public partial class Setting_Fixed : Setting
{
    public Setting_Fixed()
    {
        InitializeComponent();
        background = BackgroundColor;
        SetBackground();
        settingName = this.FindControl<TextBlock>("SettingName");
        settingValue = this.FindControl<TextBox>("Value");
    }
}