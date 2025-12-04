using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
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
using System.Collections.ObjectModel;
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

public partial class CustomThemeMaker : ScalableWindow
{
    private string previousSelection = "General Theme";
    private Cache_CustomTheme currentTheme;
    private Window backgroundWindow;
    private ThemeSlot backgroundSlot;
    private Window foregroundWindow;
    private ThemeSlot foregroundSlot;
    private int baseHeight = 742;
    private int baseWidth = 666;
    public Dictionary<BrushHolder, Border> layers = new Dictionary<BrushHolder, Border>();
    private bool loading = false;
    public CustomThemeMaker()
    {
        InitializeComponent();
        AutoTheme.SetTheme(BackgroundExample, ThemeSettings.transparent);
        AutoTheme.SetTheme(ForegroundExample, ThemeSettings.transparent);
        EditMode.SwitchMode();

        Collumn_Background.SetCategory(ThemeSettings.backgroundColor);
        Collumn_Background.SetWindow(this);
        Collumn_Foreground.SetCategory(ThemeSettings.foregroundColor);
        Collumn_Foreground.SetWindow(this);

        this.Closing += (_, _) =>
        {
            SaveTheme();
        };

        SaveButton.Click += (_, _) =>
        {
            SaveTheme();
        };

        OffsetTop.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(Resize, 1);
        };

        OffsetBottom.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(Resize, 1);
        };

        NewTheme.Click += (_, _) =>
        {
            UpdateAndSelect(ThemeManager.CreateNewTheme().name);
        };

        RemoveTheme.Click += (_, _) =>
        {
            ConfirmationWindow confirm = new ConfirmationWindow(currentTheme.name);

            confirm.Closing += (source, args) =>
            {
                if (confirm.confirmed)
                {
                    DisableEvents();
                    ThemeManager.DeleteTheme(currentTheme.name);
                    currentTheme = null;
                    GenerateThemeList();
                    LoadTheme();
                    EnableEvents();
                }
            };
        };

        DuplicateTheme.Click += (_, _) =>
        {
            if (currentTheme == null)
            {
                return;
            }
            SaveTheme();
            UpdateAndSelect(ThemeManager.DuplicateTheme(currentTheme));
        };

        RenameTheme.Click += (_, _) =>
        {
            if (NamingBox.IsVisible) 
            {
                if(NamingBox.Text != null && NamingBox.Text != "" && currentTheme != null && currentTheme.name != NamingBox.Text)
                {
                    UpdateAndSelect(ThemeManager.RenameTheme(currentTheme, NamingBox.Text));
                }
                
                NamingBox.IsVisible = false;
                Selector.IsVisible = true;
            } else
            {
                NamingBox.IsVisible = true;
                Selector.IsVisible = false;
            }
        };

        NamingBox.AddHandler(InputElement.KeyDownEvent, (sender, e) => 
        {
            if(e.Key == Key.Enter)
            {
                if (NamingBox.Text != null && NamingBox.Text != "" && currentTheme != null && currentTheme.name != NamingBox.Text)
                {
                    UpdateAndSelect(ThemeManager.RenameTheme(currentTheme, NamingBox.Text));
                }

                NamingBox.IsVisible = false;
                Selector.IsVisible = true;
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        ButtonColor.Click += async (_, _) =>
        {
            var output = await ColorSelector.PickColor(this, AutoColor.ColorToHex((ButtonColor.Background as SolidColorBrush)!.Color));

            if (output != null)
            {
                var newColor = AutoColor.HexToColor(output);
                ButtonColor.Background = new SolidColorBrush(newColor);
                currentTheme.buttonColor = newColor;
                ComputeButton();
            }
        };

        EnableEvents();
        GenerateThemeList();
        LoadTheme();
    }

    private void GenerateThemeList()
    {
        ObservableCollection<Cache_CustomTheme> list = new ObservableCollection<Cache_CustomTheme>();

        List<string> themeList = IOManager.GetThemeList();

        if (themeList.Count == 0)
        {
            list.Add(DefaultManager.launcherTheme);
        }
        else
        {
            foreach (var item in themeList)
            {
                list.Add(ThemeManager.LoadTheme(item));
            }
        }

        Selector.ItemsSource = list;
        Selector.SelectedIndex = 0;
        WindowManager.UpdateComboBox(Selector);
    }

    private void UpdateAndSelect(string target)
    {
        DisableEvents();
        GenerateThemeList();
        if (Selector.ItemsSource is ObservableCollection<Cache_CustomTheme> list)
        {
            foreach (var item in list)
            {
                if (item.name == target)
                {
                    currentTheme = null;
                    Selector.SelectedItem = item;
                    LoadTheme();
                    break;
                }
            }
        }
        EnableEvents();
    }

    private void DisableEvents()
    {
        Selector.SelectionChanged -= SelectionChanged;
    }

    private void EnableEvents()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Selector.SelectionChanged += SelectionChanged;
        });
    }

    private void SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        LoadTheme();
    }

    public void AddLayer(ThemeSettings target, BrushType layerType, BrushHolder brush, Cached_Layer? layer)
    {
        if (layerType == BrushType.Color)
        {
            if(layer == null)
            {
                layer = new Cached_ColorLayer();
            }
            brush.Setup(BrushType.Color);
        }
        else
        {
            if (layer == null)
            {
                layer = new Cached_ImageLayer();
            }
            brush.Setup(BrushType.Image);
        }


        Cache_LayeredBrush layeredBrush;

        switch (target)
        {
            case ThemeSettings.backgroundColor:
                layeredBrush = currentTheme.background;
                break;
            case ThemeSettings.foregroundColor:
                layeredBrush = currentTheme.foreground;
                break;
            default:
                return;
        }

        if (!loading)
        {
            layeredBrush.AddNewBrush(layer);
        }
        

        brush.BrushUpdated += (_, property) =>
        {
            layeredBrush.UpdateBrush(layer, brush.brush);
            layer = brush.brush;
            UpdateDisplay(layeredBrush, currentTheme.name, target);
        };

        brush.MoveUp += () =>
        {
            if (layeredBrush.MoveBrushUp(layer))
            {
                UpdateDisplay(layeredBrush, currentTheme.name, target);
            }
        };

        brush.MoveDown += () =>
        {
            if (layeredBrush.MoveBrushDown(layer))
            {
                UpdateDisplay(layeredBrush, currentTheme.name, target);
            }
        };

        brush.AskForRemoval += () =>
        {
            layeredBrush.RemoveBrush(layer);
            UpdateDisplay(layeredBrush, currentTheme.name, target);
        };
    }

    public void SaveTheme()
    {
        if (loading)
        {
            return;
        }
        Cache_CustomTheme toSave = new Cache_CustomTheme();
        toSave.name = currentTheme.name;
        toSave.background = Collumn_Background.GetBrush();
        toSave.foreground = Collumn_Foreground.GetBrush();
        if(int.TryParse(OffsetTop.Text, out int top))
        {
            toSave.topOffset = top;
        } else
        {
            toSave.topOffset = 0;
        }
        if (int.TryParse(OffsetBottom.Text, out int bottom))
        {
            toSave.bottomOffset = bottom;
        }
        else
        {
            toSave.bottomOffset = 0;
        }
        if(ButtonColor.Background is SolidColorBrush solid)
        {
            toSave.buttonBackground = solid;
        }

        ThemeManager.SaveTheme(toSave);

        DisableEvents();
        var list = Selector.ItemsSource as ObservableCollection<Cache_CustomTheme>;
        var display = list.FirstOrDefault(t => t.name == toSave.name);
        if(display != null && Selector.SelectedItem != null)
        {
            var currentSelection = Selector.SelectedItem;
            Selector.SelectedItem = null;
            loading = true;
            int index = list.IndexOf(display);
            list.RemoveAt(index);
            list.Insert(index, toSave);
            if (currentSelection == display)
            {
                Selector.SelectedItem = toSave;
            } else
            {
                Selector.SelectedItem = currentSelection;
            }
            loading = false;
        }
        EnableEvents();
    }

    public void LoadTheme()
    {
        if (loading)
        {
            return;
        }
        if(currentTheme != null)
        {
            SaveTheme();
        }
        if(Selector.SelectedItem is Cache_CustomTheme newSelection)
        {
            if(newSelection == currentTheme)
            {
                return;
            }
            currentTheme = newSelection;
        } else
        {
            return;
        }

        string themeName = currentTheme.name;

        loading = true;

        Collumn_Background.EmptyCollumn();
        Collumn_Background.LoadBrush(currentTheme.background, themeName);

        Collumn_Foreground.EmptyCollumn();
        Collumn_Foreground.LoadBrush(currentTheme.foreground, themeName);


        OffsetTop.Text = currentTheme.topOffset.ToString();
        OffsetBottom.Text = currentTheme.bottomOffset.ToString();

        ButtonColor.Background = currentTheme.buttonBackground;
        NamingBox.Text = currentTheme.name;

        loading = false;

        Resize();
        UpdateDisplay(currentTheme.background, themeName, ThemeSettings.backgroundColor);
        UpdateDisplay(currentTheme.foreground, themeName, ThemeSettings.foregroundColor);
        
    }

    public void UpdateDisplay(Bitmap newBitmap, ThemeSettings setting)
    {
        Border exampleBorder;
        Border playBorder;
        Border editBorder1;
        Border? editBorder2 = null;

        switch (setting)
        {
            case ThemeSettings.backgroundColor:
                exampleBorder = BackgroundExample;
                playBorder = PlayMode.PlayMode;
                editBorder1 = EditMode.EditMode;
                break;
            case ThemeSettings.foregroundColor:
                exampleBorder = ForegroundExample;
                playBorder = PlayMode.Foreground;
                editBorder1 = EditMode.EditRow1;
                editBorder2 = EditMode.EditRow2;
                break;
            default:
                return;
        }

        ImageBrush newBrush = new ImageBrush(newBitmap);
        newBrush.AlignmentX = AlignmentX.Center;
        newBrush.AlignmentY = AlignmentY.Center;
        newBrush.Stretch = Stretch.Fill;
        

        exampleBorder.Background = newBrush;
        playBorder.Background = newBrush;
        editBorder1.Background = newBrush;


        if(editBorder2 != null)
        {
            editBorder2.Background = newBrush;
        }
    }

    public async void UpdateDisplay(Cache_LayeredBrush layeredBrush, string themeName, ThemeSettings target)
    {
        if (loading)
        {
            return;
        }
        Bitmap? bitmap = await ThemeManager.UpdateTheme(layeredBrush, currentTheme.name, target, false);
        if (bitmap != null)
        {
            UpdateDisplay(bitmap, target);
        }
    }

    public void Resize()
    {
        if (loading)
        {
            return;
        }
        int top = 0;
        int bottom = 0;

        if(int.TryParse(OffsetTop.Text, out int newTop)){
            top = newTop;
        }

        if (int.TryParse(OffsetBottom.Text, out int newBottom))
        {
            bottom = newBottom;
        }

        double newHeight = (baseHeight + top * 3 + bottom * 3) * App.Settings.Zoom;
        WindowManager.ChangeHeight(this, (int)Math.Round(newHeight));

        PlayMode.Resize(top, bottom);
        EditMode.Resize(top, bottom);
        BackgroundExample.Height = PlayMode.Height;
        ExampleEmptySpace.RowDefinitions = new RowDefinitions(newTop + ",*," + newBottom);
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
        if (Selector.SelectedItem == null || Selector.SelectedItem.ToString() == "")
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

        if (Selector.SelectedItem == null || Selector.SelectedItem.ToString() == "")
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