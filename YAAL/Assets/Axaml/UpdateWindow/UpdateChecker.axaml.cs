using YAAL.Assets.Scripts;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static YAAL.LauncherSettings;

namespace YAAL;

public partial class UpdateChecker : UserControl
{
    public bool HasUpdatetoDownload = false;
    private UpdateWindow mainWindow;
    public UpdateChecker()
    {
        InitializeComponent();
        if (Application.Current.ActualThemeVariant == ThemeVariant.Dark)
        {
            BackgroundColor.Background = new SolidColorBrush(Color.Parse("#454545"));
        }
        else
        {
            BackgroundColor.Background = new SolidColorBrush(Color.Parse("#AAA"));
        }
        Download.Click += DownloadUpdate;
    }

    public UpdateChecker(string launcher, UpdateWindow window) : this()
    {
        launcherName.Text = launcher;
        List<string> donwloadedVersions = IOManager.GetDownloadedVersions(launcher);
        if( donwloadedVersions.Count > 0)
        {
            Downloaded.Text = IOManager.GetDownloadedVersions(launcher)[0];
        } else
        {
            Downloaded.Text = "None";
        }
        mainWindow = window;
    }

    public event EventHandler? DoneDownloading;
    public Action FoundError;

    public void CheckUpdate()
    {
        _ = GetGitVersions(launcherName.Text);
    }

    private void UpdateStatus(string newStatus)
    {
        switch (newStatus)
        {
            case "badLink":
                CheckingForUpdates.IsVisible = false;
                BadLink.IsVisible = true;
                break;
            case "done":
                CheckingForUpdates.IsVisible = false;
                GitVersion.IsVisible = true;
                if (Available.Text != Downloaded.Text)
                {
                    Download.IsEnabled = true;
                    HasUpdatetoDownload = true;
                }
                break;
        }
        mainWindow.NoteDoneChecking(this);
    }

    public async Task GetGitVersions(string launcher)
    {
        Cache_CustomLauncher cache = IOManager.LoadCacheLauncher(launcher);
        string gitlink = cache.settings[githubURL];
        if(gitlink == "")
        {
            UpdateStatus("badLink");
            return;
        }

        if (!WebManager.IsValidGitURL(gitlink))
        {
            ErrorManager.AddNewError(
                "UpdateChecker - Invalid Git URL",
                "The following URL is not a valid github URL : " + gitlink
                );
            UpdateStatus("badLink");
            FoundError?.Invoke();
            return;
        }

        string latestVersion = await WebManager.GetLatestVersion(gitlink);
        

        if (latestVersion == null || latestVersion == "")
        {
            UpdateStatus("badLink");
            FoundError?.Invoke();
            return;
        }
        Available.Text = latestVersion;

        UpdateStatus("done");
    }

    public void DownloadUpdate()
    {
        StandardDisplay.IsVisible = false;
        WaitingForDownload.IsVisible = true;
        DownloadUpdate(null, null);
    }

    private async void DownloadUpdate(object? sender, RoutedEventArgs e)
    {
        if (!HasUpdatetoDownload)
        {
            return;
        }

        StandardDisplay.IsVisible = false;
        WaitingForDownload.IsVisible = true;
        CustomLauncher customLauncher = IOManager.LoadLauncher(launcherName.Text);
        if (await WebManager.DownloadUpdatedApworld(customLauncher, Available.Text))
        {
            Downloaded.Text = IOManager.GetDownloadedVersions(launcherName.Text)[0];
            Download.IsEnabled = false;
            HasUpdatetoDownload = false;
            DoneDownloading?.Invoke(this, EventArgs.Empty);
        }
        WaitingForDownload.IsVisible = false;
        StandardDisplay.IsVisible = true;
    }
}