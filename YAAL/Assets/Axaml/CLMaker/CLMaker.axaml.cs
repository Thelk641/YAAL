using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using YAAL.Assets.Scripts;
using static System.Diagnostics.Debug;
using System.Threading;
using System.Threading.Tasks;

namespace YAAL;

public partial class CLMakerWindow : Window
{
    private static CLMakerWindow? _clMakerWindow;
    public CLMakerWindow()
    {
        InitializeComponent();
        this.Closing += (_, e) =>
        {
            if (Debouncer.isDone) {
                return;
            } 

            e.Cancel = true;
            this.Hide(); 

            Debouncer.DebounceCompleted += ActualClose;
        };
    }

    public CLMakerWindow(string launcherName) : this()
    {
        SetEvents(false);
        LauncherSelector.SelectedItem = launcherName;
        LoadLauncher(LauncherSelector.SelectedItem.ToString());
    }

    public CLMakerWindow(bool autoLoad) : this()
    {
        SetEvents(autoLoad);
    }

    private void ActualClose()
    {
        Debouncer.DebounceCompleted -= ActualClose;
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            this.Close();
        });
    }

    public static CLMakerWindow GetCLMakerWindow()
    {
        if(_clMakerWindow != null)
        {
            _clMakerWindow.Activate();
            _clMakerWindow.Topmost = true;
            _clMakerWindow.Topmost = false;
            return _clMakerWindow;
        }
        _clMakerWindow = new CLMakerWindow(true);
        return _clMakerWindow;
    }
}