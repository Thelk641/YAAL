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
    private Cache_Async? thisAsync;
    private Cache_Async? oldAsync;
    public event Action? RequestRemoval;
    public AsyncHolder()
    {
        InitializeComponent();
        BackgroundSetter.SetBackground(BackgroundColor);
    }

    public AsyncHolder(Cache_Async async) : this()
    {
        oldAsync = async;
        thisAsync = async.Clone() as Cache_Async;

        SetupPlayMode();
        SetupEditMode();
    }

    public void SetupPlayMode()
    {
        _AsyncNameBox.Text = thisAsync.settings[asyncName];
        NewSlot.Click += (_, _) =>
        {
            SlotHolder newSlot = AddNewSlot(IOManager.CreateNewSlot(thisAsync, "New"));
            newSlot.SwitchMode();
        };

        ToolVersions.Click += (_, _) =>
        {
            SettingManager settingManager = SettingManager.GetSettingsWindow(thisAsync.toolVersions);
            settingManager.OnClosing += () =>
            {
                thisAsync.toolVersions = settingManager.ParseSetting();
                Save();
            };
        };
    }

    public void SetupEditMode()
    {
        AsyncNameBox.Text = thisAsync.settings[asyncName];
        RoomBox.Text = thisAsync.settings[room];
        PasswordBox.Text = thisAsync.settings[password];
        SaveButton.Click += (_, _) =>
        {
            thisAsync.settings[asyncName] = AsyncNameBox.Text;
            thisAsync.settings[room] = RoomBox.Text;
            thisAsync.settings[password] = PasswordBox.Text;
            Save();
            SwitchMode();
        };

        DeleteButton.Click += (_, _) =>
        {
            ConfirmationWindow confirm = new ConfirmationWindow(thisAsync.settings[asyncName]);
            confirm.Closed += (_, _) =>
            {
                if (confirm.confirmed)
                {
                    IOManager.DeleteAsync(oldAsync.settings[asyncName]);
                    RequestRemoval?.Invoke();
                }
            };
        };
    }

    public SlotHolder AddNewSlot(Cache_Slot newSlot)
    {
        SlotHolder toAdd = new SlotHolder(thisAsync, newSlot);
        SlotsContainer.Children.Add(toAdd);
        toAdd.RequestRemoval += () => 
        { 
            SlotsContainer.Children.Remove(toAdd);
            this.Height -= 78;
        };
        this.Height += 78;
        return toAdd;
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
        Cache_Async newAsync = IOManager.SaveAsync(oldAsync, thisAsync);
        oldAsync = newAsync;
        thisAsync = (Cache_Async)newAsync.Clone();
        _AsyncNameBox.Text = thisAsync.settings[asyncName];
        AsyncNameBox.Text = thisAsync.settings[asyncName];
    }
}