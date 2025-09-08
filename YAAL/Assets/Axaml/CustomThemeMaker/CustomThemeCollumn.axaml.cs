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

namespace YAAL;

public partial class CustomThemeCollumn : UserControl
{
    public event Action UpdatedBrush;
    public ThemeSettings id;

    public CustomThemeCollumn()
    {
        InitializeComponent();
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
}
