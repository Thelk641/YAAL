using YAAL.Assets.Script.Cache;
using YAAL.Assets.Scripts;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static YAAL.LauncherSettings;
using static YAAL.SlotSettings;

namespace YAAL;

public partial class UpdateWindow : ScalableWindow
{
    private static UpdateWindow? _updateWindow;
    private List<UpdateChecker> waitingForUpdate = new List<UpdateChecker>();
    private List<UpdateChecker> waitingForDownload = new List<UpdateChecker>();
    private List<UpdateChecker> checkers = new List<UpdateChecker>();
    private bool mustThrow = false;
    private bool hasDownloads = false;
    public UpdateWindow()
    {
        InitializeComponent();
        SetBackground();
        DownloadAll.Click += DownloadAllUpdates;


        foreach (var item in LauncherManager.GetLauncherList())
        {
            UpdateChecker checker = new UpdateChecker(item, this);
            checker.DoneDownloading += NoteDoneDownloading;
            checker.FoundError += () => { mustThrow = true; };

            Container.Children.Add(checker);

            Separator separator = new Separator();
            Container.Children.Add(separator);

            waitingForUpdate.Add(checker);
            checkers.Add(checker);
        }

        foreach (var item in checkers)
        {
            item.CheckUpdate();
        }
    }

    private void DownloadAllUpdates(object? sender, RoutedEventArgs e)
    {

        foreach (var item in checkers)
        {
            if (item.HasUpdatetoDownload)
            {
                item.DownloadUpdate();
            }
        }
    }

    public void NoteDoneChecking(UpdateChecker checker)
    {
        waitingForUpdate.Remove(checker);
        if (checker.HasUpdatetoDownload)
        {
            hasDownloads = true;
        }
        if(waitingForUpdate.Count == 0 && hasDownloads)
        {
            DownloadAll.IsEnabled = true;
        }
    }

    private void NoteDoneDownloading(object? source, EventArgs e)
    {
        if(source is UpdateChecker checker)
        {
            waitingForDownload.Remove(checker);
            if (waitingForDownload.Count == 0 && mustThrow)
            {
                ErrorManager.ThrowError(
                    "UpdateWindow - Failed to update some files",
                    "Something went wrong while downloading update, please check other error for more information."
                    );
            }
        }
    }

    public static UpdateWindow GetUpdateWindow()
    {
        if (_updateWindow == null)
        {
            return new UpdateWindow();
        }
        else
        {
            _updateWindow.Activate();
            _updateWindow.Topmost = true;
            _updateWindow.Topmost = false;
            _updateWindow.Closing += (object? sender, WindowClosingEventArgs e) => { _updateWindow = null; };
            return _updateWindow;
        }
    }
    public void SetBackground()
    {
        var theme = Application.Current.ActualThemeVariant;
        if (theme == ThemeVariant.Dark)
        {
            Background_0.Background = new SolidColorBrush(Color.Parse("#454545"));
            Background_2.Background = new SolidColorBrush(Color.Parse("#454545"));
        }
        else
        {
            Background_0.Background = new SolidColorBrush(Color.Parse("#AAA"));
            Background_2.Background = new SolidColorBrush(Color.Parse("#AAA"));
        }
    }

    

    // -- events
}