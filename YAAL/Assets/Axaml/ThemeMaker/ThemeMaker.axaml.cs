using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using YAAL.Assets.Scripts;
using static YAAL.ThemeSettings;

namespace YAAL;

public partial class ThemeMaker : Window
{
    public ThemeMaker()
    {
        InitializeComponent();
        Selector.ItemsSource = new List<string> { "General", "LoZ", "Factorio" };
        Selector.SelectedIndex = 0;
        Collumn_Background.Setup("Background", backgroundColor);
        Collumn_Foreground.Setup("Foreground", foregroundColor);
        Collumn_ComboBox.Setup("Drop down menus", dropdownColor);
        Collumn_Button.Setup("Buttons", buttonColor);

        EditMode.SwitchMode();

        // If General is not selected, Collumn_Background's IsEnable needs to be false, as slots don't have a background

    }
}