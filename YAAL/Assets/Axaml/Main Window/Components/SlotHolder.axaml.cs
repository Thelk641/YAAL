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

namespace YAAL;

public partial class SlotHolder : UserControl
{
    private string asyncName;
    private Cache_Slot thisSlot;
    public event Action RequestRemoval;
    public event Action SwitchedToBigger;
    public event Action SwitchedToSmaller;
    public int baseHeight = 52;
    public int heightDifference = 38;
    public SlotHolder()
    {
        InitializeComponent();
        BackgroundSetter.SetBackground(BackgroundColor);
    }

    public SlotHolder (Cache_Async async, Cache_Slot slot) : this()
    {
        asyncName = async.settings[AsyncSettings.asyncName];
        thisSlot = slot;
        SetupPlayMode();
        SetupEditMode();
        
        _ = TurnEventsOn();
    }

    public void SetupPlayMode()
    {
        _SlotName.Text = thisSlot.settings[slotName];
        ToolSelect.ItemsSource = IOManager.GetToolList();
        ToolSelect.SelectedIndex = 0;

        RealPlay.Click += (_, _) =>
        {
            if (_SlotName.Text == "")
            {
                return;
            }
            ProcessManager.StartProcess(
                Environment.ProcessPath,
                ("--async " + "\"" + asyncName + "\"" + " --slot" + "\"" + _SlotName.Text + "\""),
                true);
        };

        StartTool.Click += (_, _) =>
        {
            if (ToolSelect.SelectedItem == null || ToolSelect.SelectedItem.ToString() == "")
            {
                return;
            }
            ProcessManager.StartProcess(
                Environment.ProcessPath,
                ("--async " + "\"" + asyncName + "\"" + " --slot" + "\"" + _SlotName.Text + "\"" + " --launcher" + "\"" + ToolSelect.SelectedItem.ToString() + "\""),
                true);
        };

        Edit.Click += (source, args) =>
        {
            SwitchMode();
        };
    }

    public void SetupEditMode()
    {
        SlotName.Text = thisSlot.settings[slotName];
        Patch.Text = thisSlot.settings[patch];

        List<string> launcherList = IOManager.GetLauncherList();
        SelectedLauncher.ItemsSource = launcherList;

        if (launcherList.Contains(thisSlot.settings[baseLauncher]))
        {
            SelectedLauncher.SelectedItem = thisSlot.settings[baseLauncher];
        }
        else
        {
            SelectedLauncher.SelectedIndex = 0;
        }

        _ChangedLauncher(null, null);

        PatchSelect.Click += async (_, _) =>
        {
            Patch.Text = await IOManager.PickFile(this.VisualRoot as Window);
        };

        DoneEditing.Click += (_, _) =>
        {
            Save();
            SwitchMode();
        };

        DeleteSlot.Click += (_, _) =>
        {
            IOManager.DeleteSlot(asyncName, thisSlot.settings[slotName]);
            RequestRemoval?.Invoke();
        };
    }


    public async Task TurnEventsOn()
    {
        await Dispatcher.UIThread.InvokeAsync(() => {
            SelectedLauncher.SelectionChanged += _ChangedLauncher;
            SlotName.TextChanged += (_, _) =>
            {
                _SlotName.Text = SlotName.Text;
            };
        }, DispatcherPriority.Background);
    }

   
    public void SwitchMode()
    {
        if (PlayMode.IsVisible)
        {
            PlayMode.IsVisible = false;
            EditMode.IsVisible = true;
            this.Height = baseHeight + heightDifference;
            SwitchedToBigger?.Invoke();
        } else
        {
            PlayMode.IsVisible = true;
            EditMode.IsVisible = false;
            this.Height = baseHeight;
            SwitchedToSmaller?.Invoke();
        }
    }

    public void Save()
    {
        Cache_Slot newSlot = new Cache_Slot();
        newSlot.settings[baseLauncher] = SelectedLauncher.SelectedItem.ToString();
        newSlot.settings[version] = SelectedVersion.SelectedItem.ToString();
        newSlot.settings[patch] = Patch.Text;
        newSlot.settings[slotName] = SlotName.Text;
        IOManager.SaveSlot(asyncName, newSlot, thisSlot);

        thisSlot = newSlot;
    }

    private void _ChangedLauncher(object? sender, SelectionChangedEventArgs e)
    {
        List<string> versions = IOManager.GetDownloadedVersions(SelectedLauncher.SelectedItem.ToString());

        if (versions.Contains(thisSlot.settings[version]))
        {
            SelectedVersion.SelectedItem = thisSlot.settings[version];
        }
        else
        {
            SelectedVersion.SelectedIndex = 0;
        }
    }

    public Cache_Slot GetCache()
    {
        return thisSlot;
    }
}