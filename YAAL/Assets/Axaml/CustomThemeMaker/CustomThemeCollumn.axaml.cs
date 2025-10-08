using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YAAL.Assets.Scripts;
using static YAAL.ApworldSettings;
using static YAAL.SlotSettings;
using static YAAL.AsyncSettings;
using System.Linq;
using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.IO;
using System.Diagnostics;

namespace YAAL;

public partial class CustomThemeCollumn : UserControl
{
    public event Action? UpdatedBrush;
    public ThemeSettings id;
    public CustomThemeMaker window;

    public CustomThemeCollumn()
    {
        InitializeComponent();
    }

    public void SetWindow(CustomThemeMaker maker)
    {
        window = maker;

        AddColor.Click += (_, _) =>
        {
            AddNewBrush("Color", maker.Selector.SelectedItem.ToString());
        };

        AddImage.Click += (_, _) =>
        {
            AddNewBrush("Image", maker.Selector.SelectedItem.ToString());
        };
    }

    private void AddNewBrush(string type, string newThemeName)
    {
        
        BrushHolder brush = new BrushHolder();
        brush.themeName = newThemeName;
        brush.isForeground = (id == ThemeSettings.foregroundColor);
        CommandContainer.Children.Add(brush);
        window.AddLayer(id, type, brush);
        brush.AskForRemoval += () =>
        {
            CommandContainer.Children.Remove(brush);
        };

        brush.MoveUp += () =>
        {
            int index = CommandContainer.Children.IndexOf(brush);
            if(index > 0)
            {
                CommandContainer.Children.Remove(brush);
                CommandContainer.Children.Insert(index - 1, brush);
            }
        };

        brush.MoveDown += () =>
        {
            int index = CommandContainer.Children.IndexOf(brush);
            if (index < CommandContainer.Children.Count - 1)
            {
                CommandContainer.Children.Remove(brush);
                CommandContainer.Children.Insert(index + 1, brush);
            }
        };
    }

    public void SetCategory(ThemeSettings newId)
    {
        id = newId;
        switch (newId)
        {
            case ThemeSettings.backgroundColor:
                categoryName.Text = "Background brush";
                break;
            case ThemeSettings.foregroundColor:
                categoryName.Text = "Foreground brush";
                break;
        }
    }

    public Cache_LayeredBrush GetBrush()
    {
        Cache_LayeredBrush brush = new Cache_LayeredBrush();
        foreach (var item in CommandContainer.Children)
        {
            if(item is BrushHolder holder)
            {
                brush.AddNewBrush(holder.brush);
            }
        }
        return brush;
    }
}
