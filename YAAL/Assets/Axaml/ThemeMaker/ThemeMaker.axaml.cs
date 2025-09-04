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

public partial class ThemeMaker : Window
{
    private string previousSelection = "General Theme";
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

        foreach (var item in CollumnContainer.Children)
        {
            if(item is ThemeCollumn collumn)
            {
                Debug.WriteLine(collumn.id);
                collumn.UpdatedBrush += () =>
                {
                    UpdateDisplay(collumn.id, collumn.GetBrush());
                };
            }
        }

        EditMode.SwitchMode();

        Cache_DisplayTheme general = new Cache_DisplayTheme();
        general.launcherName = "General Theme";
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

        SaveButton.Click += (_, _) =>
        {
            SaveTheme((Selector.SelectedItem as Cache_DisplayTheme).launcherName);
        };

        LoadTheme();
    }

    public void SaveTheme(string savedName = "")
    {
        if(savedName == "")
        {
            savedName = previousSelection;
        }
        Cache_DisplayTheme selectedtheme = Selector.SelectedItem as Cache_DisplayTheme;
        Cache_Theme output = new Cache_Theme();
        output.name = savedName;
        foreach (var item in CollumnContainer.Children)
        {
            if(item is ThemeCollumn collumn)
            {
                output.categories[collumn.id] = collumn.GetBrush();
            }
        }


        if (savedName == "General Theme")
        {
            IOManager.SetGeneralTheme(output);
        } else
        {
            Cache_CustomLauncher cache = IOManager.LoadCacheLauncher(savedName);
            cache.customTheme = output;
            IOManager.SaveCacheLauncher(cache);
        }

        App.Settings.SetTheme(savedName, output);

        previousSelection = selectedtheme.launcherName;
    }

    public void LoadTheme()
    {
        Cache_Theme toLoad;
        Cache_DisplayTheme selectedtheme = Selector.SelectedItem as Cache_DisplayTheme;

        if (selectedtheme.launcherName == "General Theme")
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
                UpdateDisplay(collumn.id, toLoad.categories[collumn.id]);
            }
        }

    }

    public void UpdateDisplay(ThemeSettings id, Cache_Brush newBrush)
    {
        IBrush brush = newBrush.GetBrush();

        switch (id)
        {
            case ThemeSettings.backgroundColor:
                ExampleBorder.Background = brush;
                break;
            case ThemeSettings.foregroundColor:
                PlayMode.BackgroundColor.Background = brush;
                EditMode.BackgroundColor.Background = brush;
                break;
            case ThemeSettings.dropdownColor:
                PlayMode.ToolSelect.Background = brush;
                EditMode.SlotSelector.Background = brush;
                EditMode.SelectedLauncher.Background = brush;
                EditMode.SelectedVersion.Background = brush;
                break;
            case ThemeSettings.buttonColor:
                PlayMode.RealPlay.Background = brush;
                PlayMode.StartTool.Background = brush;
                PlayMode.Edit.Background = brush;
                EditMode.FakePlay.Background = brush;
                EditMode.PatchSelect.Background = brush;
                EditMode.DownloadPatch.Background = brush;
                EditMode.ReDownloadPatch.Background = brush;
                EditMode.DoneEditing.Background = brush;
                EditMode.ManualPatchButton.Background = brush;
                EditMode.AutomaticPatchButton.Background = brush;
                EditMode.DeleteSlot.Background = brush;
                break;
        }
    }
}