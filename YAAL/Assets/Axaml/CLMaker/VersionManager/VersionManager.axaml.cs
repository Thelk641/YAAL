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
    public string gameName = "";
    public string originalName = "";

    public VersionManager()
    {
        InitializeComponent();
        this.IsVisible = true;

        AddFile.Click += (_, _) =>
        {
            VersionFile file = new VersionFile(this);
            filesToAdd.Add(file);
            FileHolder.Children.Add(file);
        };

        SaveVersion.Click += (_, _) =>
        {
            Save();
        };

        this.Closing += (_, _) =>
        {
            Save();
        };
    }

    public VersionManager(string game) : this()
    {
        gameName = game;
        VersionFile firstVersion = new VersionFile(this);
        filesToAdd.Add(firstVersion);
        FileHolder.Children.Add(firstVersion);

        VersionFile secondVersion = new VersionFile(this);
        filesToAdd.Add(secondVersion);
        FileHolder.Children.Add(secondVersion);
    }

    public VersionManager(string game, string versionName) : this()
    {
        gameName = game;
        VersionName.Text = versionName;
        originalName = versionName;
        List<string> files = IOManager.GetFilesFromVersion(game, versionName);
        foreach (var item in files)
        {
            VersionFile file = new VersionFile(this);
            file.SetTarget(item);
            filesToAdd.Add(file);
            FileHolder.Children.Add(file);
        }
    }

    public void Save()
    {
        List<string> files = new List<string>();
        foreach (var item in filesToAdd)
        {
            string target = item.GetTarget();
            if(target != "")
            {
                files.Add(item.GetTarget());
            }
        }
        if(VersionName.Text is string newName)
        {
            if(originalName != "")
            {
                if(originalName == newName)
                {
                    IOManager.UpdateVersion(gameName, newName, files);
                } else
                {
                    IOManager.UpdateVersion(gameName, originalName, newName, files);
                    originalName = newName;
                }
            } else
            {
                IOManager.AddFilesToVersion(gameName, newName, files);
            }
        }
    }

    public void RemoveFile(VersionFile file)
    {
        filesToAdd.Remove(file);
        FileHolder.Children.Remove(file);
    }
}