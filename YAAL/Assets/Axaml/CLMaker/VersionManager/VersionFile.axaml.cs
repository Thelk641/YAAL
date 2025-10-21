using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YAAL.Assets.Scripts;

namespace YAAL;

public partial class VersionFile : UserControl
{
    public VersionFile()
    {
        InitializeComponent();
        this.IsVisible = true;
    }

    public VersionFile(Window window) : this()
    {
        FileTarget.Click += async (_, _) =>
        {
            Target.Text = await IOManager.PickFile(window);
        };

        FolderTarget.Click += async (_, _) =>
        {
            Target.Text = await IOManager.PickFolder(window);
        };
    }

    public string GetTarget()
    {
        if(Target.Text is string output)
        {
            return output;
        }
        return "";
    }
}