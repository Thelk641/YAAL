using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using YAAL.Assets.Scripts;
using static YAAL.ThemeSettings;

namespace YAAL;

public partial class CustomThemeMaker : Window
{
    private string previousSelection = "General Theme";
    private Cache_CustomTheme currentTheme = new Cache_CustomTheme();
    private Window backgroundWindow;
    private ThemeSlot backgroundSlot;
    private Window foregroundWindow;
    private ThemeSlot foregroundSlot;
    private int baseHeight = 600;
    private int baseWidth = 550;
    public CustomThemeMaker()
    {
        InitializeComponent();

        Border test = new Border()
        {
            Width = 10,
            Height = 10,
        };
        test.SetValue(AutoTheme.AutoThemeProperty!, null);
        test.Background = new SolidColorBrush(Colors.Red);
        test.IsVisible = true;
        ExampleContainer.Children.Add(test);
        ThemeManager.SetCenter(test, "Start Tool", currentTheme.topOffset);



        EditMode.SwitchMode();

        Collumn_Background.SetCategory(ThemeSettings.backgroundColor);
        Collumn_Foreground.SetCategory(ThemeSettings.foregroundColor);

        List<Cache_DisplayTheme> list = new List<Cache_DisplayTheme>();

        List<string> themeList = IOManager.GetThemeList();

        if(themeList.Count == 0)
        {
            Cache_DisplayTheme defaultTheme = new Cache_DisplayTheme();
            defaultTheme.SetTheme("Default Theme", DefaultManager.theme);
            list.Add(defaultTheme);
        }

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
            SaveTheme((Selector.SelectedItem as Cache_DisplayTheme)!.launcherName);
        };

        OffsetTop.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(Resize, 1);
        };

        OffsetBottom.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(Resize, 1);
        };

        //LoadTheme();
    }

    public void SaveTheme(string savedName = "")
    {
        
    }

    public void LoadTheme()
    {
        // TODO : make this function
        currentTheme = new Cache_CustomTheme();
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

        this.Height = baseHeight + top * 3 + bottom * 3;
        this.Width = baseWidth + top * 3 + bottom * 3;

        PlayMode.Resize(top, bottom);
        EditMode.Resize(top, bottom);
        EmptyExample.Height = PlayMode.Height;
    }

    public void ComputeOffSets()
    {
        if(int.TryParse(OffsetTop.Text, out int top))
        {
            currentTheme.topOffset = top;
        } else
        {
            currentTheme.topOffset = 0;
        }

        if (int.TryParse(OffsetBottom.Text, out int bottom))
        {
            currentTheme.bottomOffset = bottom;
        } else
        {
            currentTheme.bottomOffset = 0;
        }

        Resize();
        ComputeSlotBackground();
        ComputeSlotForeground();
    }

    public void ComputeWindowBackground()
    {
        currentTheme.background = Collumn_Background.GetBrush();
        if (Selector.SelectedItem == null || Selector.SelectedItem.ToString() != ThemeManager.GetDefaultTheme().name)
        {
            return;
        }

        if (backgroundWindow == null)
        {
            backgroundWindow = new Window();
            Vector2 dimensions = WindowManager.GetWindowSize();
            backgroundWindow.Width = dimensions.X;
            backgroundWindow.Height = dimensions.Y;
            this.Closed += (_, _) =>
            {
                backgroundWindow.Close();
            };
        }
        backgroundWindow.Content = null;
        backgroundWindow.Content = currentTheme.background.BackgroundHolder;
        Bitmap rendered = ThemeManager.Render(backgroundWindow, currentTheme.name, ThemeSettings.backgroundColor);

        if (TrueBackground.Background is ImageBrush imageBrush && imageBrush.Source is Bitmap oldBitmap)
        {
            oldBitmap.Dispose();
        }

        ImageBrush windowBrush = new ImageBrush(rendered);
        TrueBackground.Background = windowBrush;
    }

    public void ComputeSlotBackground()
    {
        currentTheme.background = Collumn_Background.GetBrush();
        if (backgroundSlot == null)
        {
            backgroundSlot = new ThemeSlot();
            backgroundSlot.Width = EditMode.Width;
            backgroundSlot.Height = EditMode.Height;
        }

        Bitmap slotRender = ThemeManager.Render(backgroundSlot, currentTheme.name, ThemeSettings.backgroundColor);
        ImageBrush slotBrush = new ImageBrush(slotRender);
        PlayMode.SetTheme(ThemeSettings.backgroundColor, slotBrush);
        EditMode.SetTheme(ThemeSettings.backgroundColor, slotBrush);
    }
    public void ComputeWindowForeground()
    {
        currentTheme.foreground = Collumn_Foreground.GetBrush();

        if (Selector.SelectedItem == null || Selector.SelectedItem.ToString() != ThemeManager.GetDefaultTheme().name)
        {
            return;
        }

        if (foregroundWindow == null)
        {
            foregroundWindow = new Window();
            Vector2 dimensions = WindowManager.GetWindowSize();
            foregroundWindow.Width = dimensions.X;
            foregroundWindow.Height = dimensions.Y;
            this.Closed += (_, _) =>
            {
                foregroundWindow.Close();
            };
        }
        foregroundWindow.Content = null;
        foregroundWindow.Content = currentTheme.foreground.BackgroundHolder;
        Bitmap rendered = ThemeManager.Render(foregroundWindow, currentTheme.name, ThemeSettings.foregroundColor);

        if (SettingBorder.Background is ImageBrush imageBrush && imageBrush.Source is Bitmap oldBitmap)
        {
            oldBitmap.Dispose();
        }

        ImageBrush windowBrush = new ImageBrush(rendered);
        SettingBorder.Background = windowBrush;
        ExampleBorder.Background = windowBrush;
    }

    public void ComputeSlotForeground()
    {
        currentTheme.foreground = Collumn_Foreground.GetBrush();
        if (foregroundSlot == null)
        {
            foregroundSlot = new ThemeSlot();
            foregroundSlot.Width = PlayMode.Width;
            foregroundSlot.Height = PlayMode.Height;
        }

        Bitmap slotRender = ThemeManager.Render(foregroundSlot, currentTheme.name, ThemeSettings.foregroundColor);
        ImageBrush slotBrush = new ImageBrush(slotRender);
        PlayMode.SetTheme(ThemeSettings.foregroundColor, slotBrush);
        EditMode.SetTheme(ThemeSettings.foregroundColor, slotBrush);
    }

    public void ComputeButton()
    {
        if(ButtonColor.Background is SolidColorBrush solid)
        {
            currentTheme.buttonBackground = solid;
            PlayMode.SetTheme(ThemeSettings.buttonColor, solid);
            EditMode.SetTheme(ThemeSettings.buttonColor, solid);
        }
    }
}