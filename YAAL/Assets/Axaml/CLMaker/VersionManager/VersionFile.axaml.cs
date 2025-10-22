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

        RemoveVersion.Click += (_, _) => {
            if(window is VersionManager manager)
            {
                manager.RemoveFile(this);
            }
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

    public void SetTarget(string target)
    {
        Target.Text = target;
    }
}