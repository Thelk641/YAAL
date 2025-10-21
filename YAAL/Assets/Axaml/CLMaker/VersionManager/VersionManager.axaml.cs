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
using YAAL.Assets.Scripts;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static YAAL.GeneralSettings;
using static YAAL.LauncherSettings;

namespace YAAL;

public partial class VersionManager : ScalableWindow
{
    List<VersionFile> filesToAdd = new List<VersionFile>();
    public VersionManager()
    {
        InitializeComponent();
        VersionFile firstVersion = new VersionFile(this);
        filesToAdd.Add(firstVersion);
        FileHolder.Children.Add(firstVersion);

        VersionFile secondVersion = new VersionFile(this);
        filesToAdd.Add(secondVersion);
        FileHolder.Children.Add(secondVersion);
    }
}