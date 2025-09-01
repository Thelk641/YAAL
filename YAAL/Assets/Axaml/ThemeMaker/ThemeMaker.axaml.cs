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
    Cache_DisplayTheme selectedtheme;

    public ThemeMaker()
    {
        InitializeComponent();
        Collumn_Background.Setup("Background", backgroundColor);
        Collumn_Foreground.Setup("Foreground", foregroundColor);
        Collumn_ComboBox.Setup("Drop down menus", dropdownColor);
        Collumn_Button.Setup("Buttons", buttonColor);
        AutoTheme.SetTheme(TrueBackground, ThemeSettings.backgroundColor);
        AutoTheme.SetTheme(SettingBorder, ThemeSettings.backgroundColor);
        AutoTheme.SetTheme(ExampleBorder, ThemeSettings.backgroundColor);

        EditMode.SwitchMode();

        Cache_DisplayTheme general = new Cache_DisplayTheme();
        general.launcherName = "General theme";
        general.isHeader = false;
        general.cache_theme = IOManager.GetGeneralTheme();

        Cache_DisplayTheme header = new Cache_DisplayTheme();
        header.launcherName = "-- Custom themes";

        List<Cache_DisplayTheme> list = new List<Cache_DisplayTheme>() { general, header };

        foreach (var item in IOManager.GetLauncherList())
        {
            Cache_DisplayTheme toAdd = new Cache_DisplayTheme();
            Cache_CustomLauncher cache = IOManager.LoadCacheLauncher(item);
            toAdd.SetTheme(item, cache.customTheme);
            list.Add(toAdd);
        }

        Selector.ItemsSource = list;
        Selector.SelectedIndex = 0;


        Selector.SelectionChanged += (_, _) =>
        {
            SaveTheme();
            LoadTheme();
        };
    }

    public void SaveTheme()
    {
        Cache_Theme output = new Cache_Theme();
        output.name = selectedtheme.launcherName;
        foreach (var item in CollumnContainer.Children)
        {
            if(item is ThemeCollumn collumn)
            {
                output.categories[collumn.id] = collumn.GetBrush();
            }
        }


        if (selectedtheme.launcherName == "General Theme")
        {
            IOManager.SetGeneralTheme(output);
        } else
        {
            Cache_CustomLauncher cache = IOManager.LoadCacheLauncher(selectedtheme.launcherName);
            cache.customTheme = output;
            IOManager.SaveCacheLauncher(cache);
        }
    }

    public void LoadTheme()
    {
        Cache_Theme toLoad;

        if(selectedtheme.launcherName == "General Theme")
        {
            Collumn_Background.IsEnabled = true;
            toLoad = IOManager.GetGeneralTheme();
        } else
        {
            Collumn_Background.IsEnabled = false;
            Cache_CustomLauncher cache = IOManager.LoadCacheLauncher(selectedtheme.launcherName);
            toLoad = cache.customTheme;
        }

        foreach (var item in CollumnContainer.Children)
        {
            if (item is ThemeCollumn collumn)
            {
                collumn.SetBrush(toLoad.categories[collumn.id]);
            }
        }
    }
}