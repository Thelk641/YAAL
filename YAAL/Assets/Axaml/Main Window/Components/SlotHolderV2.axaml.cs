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
using System.Diagnostics;
using Avalonia;

namespace YAAL;

public partial class SlotHolderV2 : UserControl
{
    private string asyncName;
    private Cache_Slot thisSlot;
    private Cache_Room room;
    private List<Cache_RoomSlot> availableGames;
    public Cache_DisplaySlot selectedSlot;
    public Cache_CustomLauncher currentLauncher;
    public event Action RequestRemoval;
    public event Action SwitchedToBigger;
    public event Action SwitchedToSmaller;
    public event Action FinishedEditing;
    public event Action ChangedHeight;
    public int hardCodedHeight = 52;
    public bool isEditing = false;
    private ObservableCollection<string> toolList = new ObservableCollection<string>();
    private string previousTheme = "";
    
    public SlotHolderV2()
    {
        InitializeComponent();
    }



    public void Resize()
    {
        double newHeight = hardCodedHeight;

        Cache_CustomTheme customTheme;

        if (currentLauncher.settings.TryGetValue(LauncherSettings.customTheme, out string themeName))
        {
            customTheme = ThemeManager.GetTheme(themeName);
        } else
        {
            customTheme = ThemeManager.GetDefaultTheme();
        }

        newHeight += customTheme.topOffset + customTheme.bottomOffset;

        string combined = customTheme.topOffset.ToString() + ",*," + customTheme.bottomOffset.ToString();
        PlayEmptySpace.RowDefinitions = new RowDefinitions(combined);
        EditEmptySpace1.RowDefinitions = new RowDefinitions(combined);
        EditEmptySpace2.RowDefinitions = new RowDefinitions(combined);

        if (EditMode.IsVisible)
        {
            newHeight += hardCodedHeight + 8; // one more line, plus spacing
        }

        this.Height = newHeight;
        ChangedHeight?.Invoke();
    }

    public async Task AutoDownload()
    {
        Patch.Text = await WebManager.DownloadPatch(asyncName, thisSlot.settings[slotName], selectedSlot.cache.patchURL);
        DownloadPatch.IsVisible = false;
        ReDownloadPatch.IsVisible = true;
        Save();
    }

    public void Save()
    {
        //TODO
    }

    public void SwitchMode()
    {
        if (PlayMode.IsVisible)
        {
            PlayMode.IsVisible = false;
            EditMode.IsVisible = true;
            Resize();
        }
        else
        {
            PlayMode.IsVisible = true;
            EditMode.IsVisible = false;
            Resize();
        }
    }

    public void SwitchPatchMode()
    {
        if (AutomaticPatch.IsVisible)
        {
            AutomaticPatch.IsVisible = false;
            SlotSelector.IsVisible = false;
            SlotName.IsVisible = true;
            ManualPatch.IsVisible = true;
            AutomaticPatchButton.IsVisible = true;
            ManualPatchButton.IsVisible = false;
        }
        else
        {
            AutomaticPatch.IsVisible = true;
            SlotSelector.IsVisible = true;
            SlotName.IsVisible = false;
            ManualPatch.IsVisible = false;
            AutomaticPatchButton.IsVisible = false;
            ManualPatchButton.IsVisible = true;
        }
    }
}