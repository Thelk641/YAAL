using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static YAAL.GeneralSettings;
using static YAAL.LauncherSettings;

namespace YAAL;

public partial class Test : Window
{
    public Test()
    {
        InitializeComponent();
    }
}