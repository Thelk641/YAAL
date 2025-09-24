using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using YAAL.Assets.Scripts;
using static YAAL.ApworldSettings;
using static YAAL.AsyncSettings;
using static YAAL.SlotSettings;

namespace YAAL;

public partial class AsyncHolder : UserControl
{
    public bool isParsingUrl = false;
    private Cache_Async thisAsync = new Cache_Async();
    public event Action? RequestRemoval;
    public event Action? DoneClosing;
    private Dictionary<SlotHolder, double> previousHeight = new Dictionary<SlotHolder, double>();
    public AsyncHolder()
    {
        InitializeComponent();
        AutoTheme.SetTheme(SlotBackground, ThemeSettings.backgroundColor);
    }

    public AsyncHolder(Cache_Async async) : this()
    {
        thisAsync = async;

        SetupPlayMode();
        SetupEditMode();

        foreach (var item in thisAsync.slots)
        {
            AddNewSlot(item);
        }

        UpdatePort();

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            foreach (var item in SlotsContainer.Children)
            {
                Debug.WriteLine(item.Bounds.Width);
            }
        }, Avalonia.Threading.DispatcherPriority.Render);
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
            SettingManager settingManager = SettingManager.GetSettingsWindow(this.FindAncestorOfType<Window>(), thisAsync.toolVersions);
            settingManager.OnClosing += async () =>
            {
                thisAsync.toolVersions = settingManager.OutputSettings("Tools");
                await Save();
            };
            settingManager.IsVisible = true;
        };

        Edit.Click += (_, _) =>
        {
            SwitchMode();
        };
    }

    public void SetupEditMode()
    {
        AsyncNameBox.Text = thisAsync.settings[asyncName];
        RoomBox.Text = thisAsync.settings[roomURL];
        PasswordBox.Text = thisAsync.settings[password];
        SaveButton.Click += (_, _) =>
        {
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
                    IOManager.DeleteAsync(thisAsync.settings[asyncName]);
                    RequestRemoval?.Invoke();
                }
            };
        };

        RoomBox.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(UpdateSlotsRoom, 1);
            
        };
    }

    public async void UpdatePort()
    {
        thisAsync.room = await WebManager.GetRoomPort(thisAsync.room);
        thisAsync.settings[roomIP] = thisAsync.room.IP;
        thisAsync.settings[roomPort] = thisAsync.room.port;
        Save();
    }
    
    public async void UpdateSlotsRoom()
    {
        if(RoomBox.Text == "" || RoomBox.Text == thisAsync.room.URL)
        {
            return;
        }

        if (RoomBox.Text.Contains("archipelago.gg/room/"))
        {
            thisAsync.room = await WebManager.ParseRoomURL(RoomBox.Text);
        }

        //UpdatePort();

        foreach (var item in SlotsContainer.Children)
        {
            if (item is SlotHolder slotHolder)
            {
                slotHolder.SetRoom(thisAsync.room);
            }
        }

        Save();
    }

    public SlotHolder AddNewSlot(Cache_Slot newSlot)
    {
        SlotHolder toAdd = new SlotHolder(thisAsync, newSlot);
        
        this.Height += 8; // they're going to switch to edit mode immediately, triggering ChangedHeight with previousHeight=0

        if (SlotsContainer.Children.Count > 0)
        {
            Rectangle rect = new Rectangle();
            rect.Height = 8;
            SlotsContainer.Children.Add(rect);
        }

        SlotsContainer.Children.Add(toAdd);
        

        toAdd.RequestRemoval += () => 
        { 
            SlotsContainer.Children.Remove(toAdd);
            this.Height -= toAdd.Height;
        };

        toAdd.ChangedHeight += (double previousHeight, double newHeight) =>
        {
            this.Height -= previousHeight;
            this.Height += newHeight;
        };

        if(thisAsync.room.slots.Count > 0)
        {
            toAdd.SetRoom(thisAsync.room);
        }

        
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

    public async Task Save()
    {
        Edit.IsEnabled = false;
        Cache_Async toSave = new Cache_Async();
        toSave.settings[asyncName] = AsyncNameBox.Text;
        toSave.settings[roomURL] = RoomBox.Text;
        toSave.settings[password] = PasswordBox.Text;
        toSave.settings[roomIP] = thisAsync.settings[roomIP];
        toSave.settings[roomPort] = thisAsync.settings[roomPort];
        toSave.room = thisAsync.room;

        foreach (var item in SlotsContainer.Children)
        {
            if(item is SlotHolder slotHolder)
            {
                toSave.slots.Add(slotHolder.Slot);
            }
        }

        foreach (var item in SlotsContainer.Children)
        {
            if (item is SlotHolder slotHolder)
            {
                slotHolder.SetAsyncName(toSave.settings[asyncName]);
                slotHolder.SetRoom(toSave.room);
            }
        }

        thisAsync = IOManager.SaveAsync(thisAsync, toSave);
        _AsyncNameBox.Text = thisAsync.settings[asyncName];
        AsyncNameBox.Text = thisAsync.settings[asyncName];
        thisAsync.room = toSave.room;
        thisAsync.settings[roomURL] = toSave.room.URL;

        Edit.IsEnabled = true;
    }

    public async void ClosingSave()
    {
        foreach (var item in SlotsContainer.Children)
        {
            if(item is SlotHolder slot)
            {
                slot.ClosingSave();
            }
        }
        if (!PlayMode.IsVisible)
        {
            await Save();
        }
        DoneClosing?.Invoke();
    }
}