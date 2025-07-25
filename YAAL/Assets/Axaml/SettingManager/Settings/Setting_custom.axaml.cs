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

    public override void SetBinary()
    {
        Binary.ItemsSource = new List<string>()
        {
            true.ToString(),
            false.ToString()
        };
        TextBox storage = settingValue as TextBox;
        if (storage.Text == true.ToString() || storage.Text == false.ToString())
        {
            storage.IsVisible = false;
            Binary.IsVisible = true;
            if (storage.Text == true.ToString())
            {
                Binary.SelectedItem = true.ToString();
            }
            else
            {
                Binary.SelectedItem = false.ToString();
            }
            Binary.SelectionChanged += (sender, args) => { storage.Text = Binary.SelectedItem.ToString(); };
        }
    }
}