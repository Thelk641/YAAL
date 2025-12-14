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
using Svg;

namespace YAAL;

public partial class InputWindow : ScalableWindow
{
    public InputWindow()
    {
        InitializeComponent();

        VariableContent.KeyDown += (_, keyValue) =>
        {
            if(keyValue.Key == Avalonia.Input.Key.Enter)
            {
                keyValue.Handled = true;
                this.Close();
            }
        };

        FileButton.Click += async (_, _) =>
        {
            string input = await IOManager.PickFile(this);
            input = "\"" + input + "\";";
            VariableContent.Text += input;
        };
    }

    public void Setup(string variableName)
    {
        VariableDisplay.Text = "Value to be stored in : " + variableName;
    }

    public string GetVariableContent()
    {
        return VariableContent.Text ?? "";
    }
}