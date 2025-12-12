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
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YAAL.Assets.Scripts;

namespace YAAL;

public partial class Setting : UserControl
{
    public event Action RequestRemoval;
    private Control displayedValue;
    string realValue = "";
    bool firstTimeColor = true;
    bool firstTimeSlider = true;
    bool firstTimeBoolean = true;

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
        SwitchToSpecialMode(value);
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
    }

    private void Setup(string name, string value)
    {
        InitializeComponent();
        CustomValue.TextChanged += (_, _) =>
        {
            SetValue.Text = CustomValue.Text;
        };
        SetName.Text = name;

        SetValue.Text = value;

        displayedValue = SetValue;

        //SwitchToSpecialMode(value);

        File.Click += async (_, _) =>
        {
            CustomValue.Text = await IOManager.PickFile(this.FindAncestorOfType<Window>());
        };

        SpecialMode.Click += (_, _) =>
        {
            SwitchToSpecialMode(CustomValue.Text);
        };
    }

    public void IsZoom()
    {
        CustomValue.TextChanged += (_, _) =>
        {
            if (float.TryParse(CustomValue.Text, out float newZoom))
            {
                Debouncer.Debounce(
                    () => { App.Settings.Zoom = newZoom; },
                    0.5f
                    );
            }
        };
    }

    public void SwitchToSpecialMode(string value)
    {
        if (SetValue.Text == true.ToString() || SetValue.Text == false.ToString())
        {
            SwitchToBinary();
        }
        else if (Color.TryParse(value, out Color color))
        {
            SwitchToColor(color);
        }
        else
        {
            string pattern = @"^\[(.*?)\]\[(.*?)\]\[(.*?)\]$";
            var match = Regex.Match(value, pattern);

            if (match.Success)
            {
                SwitchToSlider(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
            }
        }
    }

    public string GetValue(out string value)
    {
        if(Enum.TryParse<HardcodedSettings>(SetName.Text, out HardcodedSettings name))
        {
            value = realValue;
        } else
        {
            value = SetValue.Text!;
        }
        return SetName.Text!;
    }

    private void SetupSoftCodedSetting()
    {
        if(displayedValue.Name != SetValue.Name)
        {
            return;
        }

        CustomValue.Text = SetValue.Text;
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

        if (firstTimeBoolean)
        {
            BinaryValue.SelectionChanged += (_, _) =>
            {
                CustomValue.Text = BinaryValue.SelectedItem.ToString();
                if (BinaryValue.SelectedItem.ToString() == "Manual")
                {
                    BinaryValue.IsVisible = false;
                    CustomValueContainer.IsVisible = true;
                    displayedValue = CustomValueContainer;
                }
            };
            firstTimeBoolean = false;
        }

        
    }

    private void SwitchToColor(Color color)
    {
        var oldDisplay = displayedValue;
        displayedValue.IsVisible = false;
        ColorContainer.IsVisible = true;
        displayedValue = ColorContainer;

        AutoTheme.SetTheme(ColorValue, ThemeSettings.off);
        ColorValue.Background = new SolidColorBrush(color);

        if (firstTimeColor)
        {
            ColorValue.Click += async (_, _) =>
            {
                var output = await ColorSelector.PickColor(this.FindAncestorOfType<Window>(), SetValue.Text.ToString());

                if (output != null)
                {
                    CustomValue.Text = output;
                    ColorValue.Background = new SolidColorBrush(AutoColor.HexToColor(CustomValue.Text));
                    if(SetName.Text is string name && Enum.TryParse<GeneralSettings>(name, out GeneralSettings setting))
                    {
                        // The only color in GeneralSettings are the general theme
                        // this lets us update it in real time
                        IOManager.SetSetting(setting, output);
                    }
                }

            };

            ColorModeOff.Click += (_, _) =>
            {
                ColorContainer.IsVisible = false;
                oldDisplay.IsVisible = true;
                displayedValue = CustomValueContainer;
            };

            firstTimeColor = false;
        }
    }

    private void SwitchToSlider(string minimum, string value, string maximum)
    {
        if(
            !double.TryParse(Clean(minimum), CultureInfo.InvariantCulture, out double trueMinimum) ||
            !double.TryParse(Clean(value), CultureInfo.InvariantCulture, out double trueValue) ||
            !double.TryParse(Clean(maximum), CultureInfo.InvariantCulture, out double trueMaximum)){
            return;
        }

        if(trueValue > trueMaximum)
        {
            trueMaximum = trueValue;
        }

        if(trueValue < trueMinimum)
        {
            trueMinimum = trueValue;
        }

        slider.Minimum = trueMinimum;
        slider.Value = trueValue;
        slider.Maximum = trueMaximum;

        displayedValue.IsVisible = false;
        SliderContainer.IsVisible = true;
        displayedValue = SliderContainer;

        CustomValue.Text = "[" + trueMinimum + "]" + "[" + trueValue + "]" + "[" + trueMaximum + "]";

        if (!firstTimeSlider)
        {
            return;
        }
        firstTimeSlider = false;

        slider.ValueChanged += (_, _) =>
        {
            slider.Value = Math.Round(slider.Value, 1);
            Debouncer.Debounce(
                () => {
                    string updatedValue = Math.Round(slider.Value, 1).ToString();
                    string fullText = "[" + trueMinimum + "]" + "[" + updatedValue + "]" + "[" + trueMaximum + "]";

                    CustomValue.Text = Clean(fullText);

                    Trace.WriteLine("Slider changed, new value : " + CustomValue.Text);
                },
                0.5f);
        };

        SliderModeOff.Click += (_, _) =>
        {
            SliderContainer.IsVisible = false;
            CustomValueContainer.IsVisible = true;
            displayedValue = CustomValueContainer;
        };

        if (SetName.Text == GeneralSettings.zoom.ToString())
        {
            CustomValue.TextChanged += (_, _) =>
            {
                string value = Clean(ParseSlider(CustomValue.Text));

                if (double.TryParse(value, CultureInfo.InvariantCulture, out double newZoom))
                {
                    App.Settings.Zoom = newZoom;
                }

            };
        }
    }

    private string ParseSlider(string rawText)
    {
        string pattern = @"^\[(.*?)\]\[(.*?)\]\[(.*?)\]$";
        var match = Regex.Match(rawText, pattern);

        if (match.Success)
        {
            return match.Groups[2].Value;
        }

        return "";
    }

    private string Clean(string input)
    {
        if (input.Contains(","))
        {
            return input.Replace(",", ".");
        }
        return input;
    }
}