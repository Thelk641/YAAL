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
using System.Xml.Linq;
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
    private int baseHeight = 742;
    private int baseWidth = 666;
    public Dictionary<BrushHolder, Border> layers = new Dictionary<BrushHolder, Border>();
    public CustomThemeMaker()
    {
        InitializeComponent();

        // DEBUG
        //AddARedSquare("Start Tool");
        // END OF DEBUG


        BackgroundExample.SetValue(AutoTheme.AutoThemeProperty, null);
        BackgroundExample.Background = new SolidColorBrush(Colors.Transparent);
        ForegroundExample.SetValue(AutoTheme.AutoThemeProperty, null);
        ForegroundExample.Background = new SolidColorBrush(Colors.Transparent);
        EditMode.SwitchMode();

        Collumn_Background.SetCategory(ThemeSettings.backgroundColor);
        Collumn_Background.SetWindow(this);
        Collumn_Foreground.SetCategory(ThemeSettings.foregroundColor);
        Collumn_Foreground.SetWindow(this);

        List<Cache_DisplayTheme> list = new List<Cache_DisplayTheme>();

        List<string> themeList = IOManager.GetThemeList();

        if (themeList.Count == 0)
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



    private void AddARedSquare(string centerName)
    {
        Border test = new Border()
        {
            Width = 12,
            Height = 12,
        };
        test.SetValue(AutoTheme.AutoThemeProperty!, null);
        test.Background = new SolidColorBrush(Colors.Red);
        test.IsVisible = true;
        ExampleBackgroundContainer.Children.Add(test);
        ThemeManager.SetCenter(test, "Tracker", currentTheme.topOffset, BackgroundExample);

        Border secondTest = new Border()
        {
            Width = 10,
            Height = 200,
        };
        secondTest.SetValue(AutoTheme.AutoThemeProperty!, null);
        secondTest.Background = new SolidColorBrush(Colors.White);
        secondTest.IsVisible = true;
        ExampleForegroundContainer.Children.Add(secondTest);
        ThemeManager.SetCenter(secondTest, "Tracker", currentTheme.topOffset, BackgroundExample);

        Dispatcher.UIThread.Post(() =>
        {
            Point testPoint = new Point(Canvas.GetLeft(test), Canvas.GetTop(test));
            Point secondtestPoint = new Point(Canvas.GetLeft(secondTest), Canvas.GetTop(secondTest));
            Debug.WriteLine("Red : " + testPoint + " / " + test.TransformToVisual(BackgroundExample));
            Debug.WriteLine("White : " + secondtestPoint + " / " + secondTest.TransformToVisual(BackgroundExample));
        });
    }

    public void AddLayer(ThemeSettings target, string layerType, BrushHolder brush)
    {
        Canvas holder = new Canvas();
        switch (target)
        {
            case ThemeSettings.backgroundColor:
                holder = ExampleBackgroundContainer;
                break;
            case ThemeSettings.foregroundColor:
                holder = ExampleForegroundContainer;
                break;
        }

        if(layerType == "Color")
        {
            brush.Setup("Color");
        } else
        {
            brush.Setup("Image");
        }

        Border border = brush.brush.GetLayer();
        holder.Children.Add(border);
        layers[brush] = border;

        brush.BrushUpdated += (_, _) =>
        {
            Border updated = brush.brush.GetLayer();
            ThemeManager.SetCenter(updated, brush.brush.center, currentTheme.topOffset, BackgroundExample);

            int index = holder.Children.IndexOf(layers[brush]);
            holder.Children.Remove(layers[brush]);
            holder.Children.Insert(index, updated);
            layers[brush] = updated;
        };
    }

    public void MoveLayerUp(BrushHolder brush)
    {
        if (layers.ContainsKey(brush))
        {
            Border toMove = layers[brush];
            if (toMove.Parent is Canvas canvas)
            {
                int index = canvas.Children.IndexOf(toMove);
                canvas.Children.Remove(toMove);
                canvas.Children.Insert(index - 1, toMove);
            }
        }
    }

    public void MoveLayerDown(BrushHolder brush)
    {
        if (layers.ContainsKey(brush))
        {
            Border toMove = layers[brush];
            if (toMove.Parent is Canvas canvas)
            {
                int index = canvas.Children.IndexOf(toMove);
                canvas.Children.Remove(toMove);
                canvas.Children.Insert(index + 1, toMove);
            }
        }
    }

    public void RemoveLayer(BrushHolder brush)
    {
        if (layers.ContainsKey(brush))
        {
            Border toRemove = layers[brush];
            if(toRemove.Parent is Canvas canvas)
            {
                layers.Remove(brush);
                canvas.Children.Remove(toRemove);
            }
        }
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
        BackgroundExample.Height = PlayMode.Height;
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