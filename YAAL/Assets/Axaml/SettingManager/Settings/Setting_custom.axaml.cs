using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;

namespace YAAL;

public partial class Setting_Custom : Setting
{
    public Setting_Custom()
    {
        InitializeComponent();
        background = BackgroundColor;
        SetBackground();
        settingName = this.FindControl<TextBox>("SettingName");
        settingValue = this.FindControl<TextBox>("Value");
    }
}