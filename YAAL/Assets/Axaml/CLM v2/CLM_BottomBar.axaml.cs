using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using YAAL.Assets.Scripts;
using static YAAL.ApworldSettings;

namespace YAAL;

public partial class CLM_BottomBar : UserControl
{
    private CLM clm;
    private CLM_Selector selector;
    public CLM_BottomBar()
    {
        InitializeComponent();
        ModeSelector.ItemsSource = new List<string>() { "Game", "Tool" };
        ModeSelector.SelectedIndex = 0;
    }

    public CLM_BottomBar(CLM newCLM, CLM_Selector selector) : this()
    {
        this.clm = newCLM;
        this.selector = selector;
        OpenTestWindow.Click += (_, _) =>
        {
            if(selector.GetCache() is Cache_DisplayLauncher display)
            {
                TestWindow testWindow = TestWindow.GetTestWindow(clm, (string)ModeSelector.SelectedItem! == "Game");
                bool requiresPatch = display.cache.settings[LauncherSettings.requiresPatch] == true.ToString();
                bool requiresVersion = display.cache.settings[LauncherSettings.requiresVersion] == true.ToString();
                testWindow.Setup(display.name, requiresPatch, requiresVersion);
                testWindow.Show();
            }
        };
    }

    public void SwitchGameMode(bool isGame)
    {
        if (isGame)
        {
            ModeSelector.SelectedItem = "Game";
        } else
        {
            ModeSelector.SelectedItem = "Tool";
        }
    }
}