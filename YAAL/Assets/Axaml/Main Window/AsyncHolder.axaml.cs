using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YAAL.Assets.Scripts;
using static YAAL.ApworldSettings;
using static YAAL.AsyncSettings;
using static YAAL.SlotSettings;

namespace YAAL;

public partial class AsyncHolder : UserControl
{
    private Cache_Async thisAsync;
    public AsyncHolder()
    {
        InitializeComponent();
    }

    public AsyncHolder(Cache_Async async)
    {
        thisAsync = async;
        SetupPlayMode();
        SetupEditMode();
        
        _ = TurnEventsOn();
    }

    public void SetupPlayMode()
    {
        _AsyncNameBox.Text = thisAsync.settings[asyncName];
        NewSlot.Click += (source, args) =>
        {
            SlotHolder newSlot = AddNewSlot(IOManager.CreateNewSlot(thisAsync, "New"));
            newSlot.SwitchMode();
        };

        ToolVersions.Click += (source, args) =>
        {
            // Need to add it the option to just recieve dict<string,string>
            // And then on exit save its settings
            SettingManager settingManager = new SettingManager();
        };
    }

    public void SetupEditMode()
    {
        
    }

    public SlotHolder AddNewSlot(Cache_Slot newSlot)
    {
        SlotHolder toAdd = new SlotHolder(thisAsync, newSlot);
        SlotsContainer.Children.Add(toAdd);
        return toAdd;
    }

    public async Task TurnEventsOn()
    {
        await Dispatcher.UIThread.InvokeAsync(() => {
            
        }, DispatcherPriority.Background);
    }

   
    public void SwitchMode()
    {
        if (PlayMode.IsVisible)
        {
            PlayMode.IsVisible = false;
            EditMode.IsVisible = true;
        } else
        {
            PlayMode.IsVisible = true;
            EditMode.IsVisible = false;
        }
    }

    public void Save()
    {
        
    }
}