using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using YAAL.Assets.Scripts;

namespace YAAL;

public partial class Setting : UserControl
{
    public event Action RequestRemoval;
    private Control displayedValue;
    string realValue = "";

    public Setting()
    {
        InitializeComponent();
    }

    public Setting(string name, string value, bool canBeEdited = true)
    {
        Setup(name, value);
        if (canBeEdited)
        {
            SetupCustomSetting();
        } else
        {
            SetupSoftCodedSetting();
        }
    }

    public Setting(string name, string value, string displayedText)
    {
        Setup(name, value);
        SetValue.Text = displayedText;
        realValue = value;

        File.Click += async (_, _) =>
        {
            CustomValue.Text = await IOManager.PickFile(this.FindAncestorOfType<Window>());
        };
        BackgroundSetter.Set(File);
        BackgroundSetter.Set(ColorContainer, GeneralSettings.backgroundColor);
    }

    private void Setup(string name, string value)
    {
        InitializeComponent();
        SetName.Text = name;

        SetValue.Text = value;

        displayedValue = SetValue;

        if (SetValue.Text == true.ToString() || SetValue.Text == false.ToString())
        {
            SwitchToBinary();
        }
        else if (Color.TryParse(value, out Color color))
        {
            SwitchToColor();
        }

        File.Click += async (_, _) =>
        {
            CustomValue.Text = await IOManager.PickFile(this.FindAncestorOfType<Window>());
        };
        BackgroundSetter.Set(File);
    }

    public string GetValue(out string value)
    {
        if(realValue == "")
        {
            value = SetValue.Text;
        } else
        {
            value = realValue;
        }
        return SetName.Text;
    }

    private void SetupSoftCodedSetting()
    {
        if(displayedValue.Name != SetValue.Name)
        {
            return;
        }

        CustomValue.Text = SetValue.Text;
        CustomValue.TextChanged += (_, _) =>
        {
            SetValue.Text = CustomValue.Text;
        };
        displayedValue.IsVisible = false;
        CustomValueContainer.IsVisible = true;
        displayedValue = CustomValueContainer;
    }

    private void SetupCustomSetting()
    {
        SetupSoftCodedSetting();

        CustomName.Text = SetName.Text;
        CustomName.TextChanged += (_, _) =>
        {
            SetName.Text = CustomName.Text;
        };
        SetName.IsVisible = false;
        CustomName.IsVisible = true;

        TurnOnRemovalButton();
    }

    private void TurnOnRemovalButton()
    {
        Grid.SetColumnSpan(displayedValue, 1);
        removeComponent.IsVisible = true;
        removeComponent.IsEnabled = true;
        removeComponent.Click += (_, _) =>
        {
            RequestRemoval?.Invoke();
        };
    }

    private void SwitchToBinary()
    {
        BinaryValue.ItemsSource = new[] { true.ToString(), false.ToString(), "Manual" };
        BinaryValue.SelectedItem = SetValue.Text;
        displayedValue.IsVisible = false;
        BinaryValue.IsVisible = true;
        displayedValue = BinaryValue;

        BinaryValue.SelectionChanged += (_, _) =>
        {
            SetValue.Text = BinaryValue.SelectedItem.ToString();
            if(BinaryValue.SelectedItem.ToString() == "Manual")
            {
                BinaryValue.IsVisible = false;
                CustomValueContainer.IsVisible = true;
                displayedValue = CustomValueContainer;
            }
        };
    }

    private void SwitchToColor()
    {
        displayedValue.IsVisible = false;
        ColorContainer.IsVisible = true;
        displayedValue = ColorContainer;

        ColorValue.Background = new SolidColorBrush(AutoColor.HexToColor(SetValue.Text));

        ColorValue.Click += async (_, _) =>
        {
            var output = await ColorSelector.PickColor(this.FindAncestorOfType<Window>(), SetValue.Text.ToString());
            
            if (output != null)
            {
                SetValue.Text = output;
                ColorValue.Background = new SolidColorBrush(AutoColor.HexToColor(SetValue.Text));
                this.FindAncestorOfType<SettingManager>().ChangedColor(this, SetName.Text, SetValue.Text);
            }

        };
    }
}