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

namespace YAAL;

public partial class ThemeGeneral : UserControl
{
    public ThemeGeneral()
    {
        InitializeComponent();
    }
}