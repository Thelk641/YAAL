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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using YAAL.Assets.Scripts;
using static YAAL.ThemeSettings;

namespace YAAL;

public partial class CustomThemeMaker : Window
{
    private string previousSelection = "General Theme";
    private int baseHeight = 600;
    private int baseWidth = 550;
    public CustomThemeMaker()
    {
        InitializeComponent();

        EditMode.SwitchMode();

        Collumn_Background.SetCategory(ThemeSettings.backgroundColor);
        Collumn_Foreground.SetCategory(ThemeSettings.foregroundColor);

        List<Cache_DisplayTheme> list = new List<Cache_DisplayTheme>();

        foreach (var item in IOManager.GetThemeList())
        {
            Cache_DisplayTheme toAdd = new Cache_DisplayTheme();
            toAdd.SetTheme(item, IOManager.GetTheme(item));
            list.Add(toAdd);
        }

        Selector.ItemsSource = list;
        Selector.SelectedIndex = 0;

        Selector.SelectionChanged += (_, _) =>
        {
            SaveTheme();
            LoadTheme();
        };

        SaveButton.Click += (_, _) =>
        {
            SaveTheme((Selector.SelectedItem as Cache_DisplayTheme).launcherName);
        };

        LoadTheme();

        OffsetTop.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(Resize, 1);
        };

        OffsetBottom.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(Resize, 1);
        };
    }

    public void SaveTheme(string savedName = "")
    {
        
    }

    public void LoadTheme()
    {

    }

    public void Resize()
    {
        int top = 0;
        int bottom = 0;

        if(int.TryParse(OffsetTop.Text, out int newTop)){
            top = newTop;
        }

        if (int.TryParse(OffsetBottom.Text, out int newBottom))
        {
            bottom = newBottom;
        }

        this.Height = (baseHeight + top * 3 + bottom * 3) * App.Settings.Zoom;
        this.Width = (baseWidth + top * 3 + bottom * 3) * App.Settings.Zoom;

        // Todo : update ThemeSlot
    }
}