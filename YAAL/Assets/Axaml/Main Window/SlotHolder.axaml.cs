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
    public SlotHolder()
    {
        InitializeComponent();
        BackgroundSetter.SetBackground(BackgroundColor);
    }

    public SlotHolder (Cache_Async async, Cache_Slot slot)
    {
        asyncName = async.settings[AsyncSettings.asyncName];
        thisSlot = slot;
        BackgroundSetter.SetBackground(BackgroundColor);
        SetupPlayMode();
        SetupEditMode();
        
        _ = TurnEventsOn();
    }

    public void SetupPlayMode()
    {
        _SlotName.Text = thisSlot.settings[slotName];
        ToolSelect.ItemsSource = IOManager.GetToolList();

        RealPlay.Click += (source, args) =>
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

        StartTool.Click += (source, args) =>
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

        PatchSelect.Click += async (source, args) =>
        {
            Patch.Text = await IOManager.PickFile(this.VisualRoot as Window);
        };

        DoneEditing.Click += (source, args) =>
        {
            Save();
            SwitchMode();
        };
    }


    public async Task TurnEventsOn()
    {
        await Dispatcher.UIThread.InvokeAsync(() => {
            SelectedLauncher.SelectionChanged += _ChangedLauncher;
            SlotName.TextChanged += (source, args) =>
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
            this.Height = 108;
        } else
        {
            PlayMode.IsVisible = true;
            EditMode.IsVisible = false;
            this.Height = 70;
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
}