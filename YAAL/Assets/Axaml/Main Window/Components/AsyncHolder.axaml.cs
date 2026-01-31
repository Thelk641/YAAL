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
using System.Runtime.CompilerServices;
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
    public event Action? DoneSaving;
    public bool isSaving = false;
    public bool waitingToSave = false;
    private Dictionary<SlotHolder, double> previousHeight = new Dictionary<SlotHolder, double>();
    private bool starting = true;
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
        string asyncName = thisAsync.settings[AsyncSettings.asyncName];
        string slotName = "";

        foreach (var item in thisAsync.slots)
        {
            slotName = item.settings[SlotSettings.slotLabel];
            if (IOManager.CheckExistance(asyncName, slotName))
            {
                AddNewSlot(item);
            } else
            {
                // Sometime for some reason we get slots only half-deleted,
                // can't seem to find what triggers this, so let's just 
                // make sure we actually nuke them as they're not meant to exists
                IOManager.DeleteSlot(asyncName, slotName);
            }
        }
        starting = false;

        UpdatePort();
        Dispatcher.UIThread.Post(TurnEventsOn);
    }

    private void TurnEventsOn()
    {
        AsyncNameBox.TextChanged += RenameAsync;
    }

    private void TurnEventsOff()
    {
        AsyncNameBox.TextChanged -= RenameAsync;
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
            SettingWindow settingManager = SettingWindow.GetSettingsWindow(this.FindAncestorOfType<Window>(), thisAsync.toolVersions);
            settingManager.OnClosing += async () =>
            {
                thisAsync.toolVersions = settingManager.OutputSettings("Tools");
                Debouncer.Debounce(Save, 1f);
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
            Debouncer.ForceDebounce(Save);
            SwitchMode();
        };

        DeleteButton.Click += (_, _) =>
        {
            if(WindowManager.OpenWindow(WindowType.ConfirmationWindow, this.GetVisualRoot() as Window) is ConfirmationWindow confirm)
            {
                confirm.Setup(thisAsync.settings[asyncName]);
                confirm.Closed += (_, _) =>
                {
                    if (confirm.confirmed)
                    {
                        IOManager.DeleteAsync(thisAsync.settings[asyncName]);
                        RequestRemoval?.Invoke();
                    }
                };
            }
        };

        AsyncNameBox.TextChanged += RenameAsync;

        RoomBox.TextChanged += (_, _) =>
        {
            Debouncer.Debounce(UpdateSlotsRoom, 0.5f);
        };
    }

    private void RenameAsync(object? sender, TextChangedEventArgs e)
    {
        waitingToSave = true;
        Debouncer.Debounce(Save, 1f);
    }

    public async Task UpdatePort(bool save = true)
    {
        string URLToParse = thisAsync.settings[roomURL];
        if (WebManager.IsValidURL(URLToParse))
        {
            thisAsync.room = await WebManager.ParseRoomURL(RoomBox.Text);
            thisAsync.settings[roomAddress] = thisAsync.room.address;
            thisAsync.settings[roomPort] = thisAsync.room.port;
            thisAsync.settings[room] = thisAsync.room.address + ":" + thisAsync.room.port;
            thisAsync.settings[cheeseURL] = thisAsync.room.cheeseTrackerURL;
        }
        else if (URLToParse.Contains(":"))
        {
            thisAsync.room = new Cache_Room();

            string[] split = URLToParse.Split(':');
            thisAsync.settings[roomAddress] = split[0];
            thisAsync.settings[roomPort] = split[1];
            thisAsync.settings[room] = URLToParse;
        }

        if (save)
        {
            Debouncer.ForceDebounce(Save);
        }
    }
    
    public async void UpdateSlotsRoom()
    {
        if (RoomBox.Text == "" || RoomBox.Text == thisAsync.room.URL)
        {
            return;
        }

        if (waitingToSave)
        {
            Debouncer.ForceDebounce(Save);
        }

        ReadingRoom.IsVisible = true;
        thisAsync.settings[roomURL] = RoomBox.Text ?? "";
        await UpdatePort(false);  

        foreach (var item in SlotsContainer.Children)
        {
            if (item is SlotHolder slotHolder)
            {
                slotHolder.SetRoom(thisAsync.room);
            }
        }

        Debouncer.ForceDebounce(Save);
        ReadingRoom.IsVisible = false;
    }

    public SlotHolder AddNewSlot(Cache_Slot newSlot)
    {
        Debouncer.ForceDebounce(Save);
        SlotHolder toAdd = new SlotHolder(thisAsync, newSlot, this);
        Rectangle rect = null;
        
        this.Height += 8; // they're going to switch to edit mode immediately, triggering ChangedHeight with previousHeight=0

        if (SlotsContainer.Children.Count > 0)
        {
            rect = new Rectangle();
            rect.Height = 8;
            SlotsContainer.Children.Add(rect);
        }

        SlotsContainer.Children.Add(toAdd);
        

        toAdd.RequestRemoval += () => 
        { 
            SlotsContainer.Children.Remove(toAdd);
            this.Height -= toAdd.Height;
            if(rect != null)
            {
                SlotsContainer.Children.Remove(rect);
                this.Height -= rect.Height;
            }

            if (SlotsContainer.Children.Count > 0 && SlotsContainer.Children[0] is Rectangle obsoleteRectangle)
            {
                SlotsContainer.Children.Remove(obsoleteRectangle);
                this.Height -= obsoleteRectangle.Height;
            }
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

    public void Save()
    {
        if (starting || AsyncNameBox.Text == "")
        {
            return;
        }

        TurnEventsOff();

        try
        {
            // TODO : implement "equal" so we can check if toSave is identical to thisAsync and not waste time saving file
            waitingToSave = false;
            isSaving = true;
            Edit.IsEnabled = false;
            Cache_Async toSave = new Cache_Async();
            toSave.settings[asyncName] = AsyncNameBox.Text!;
            toSave.settings[roomURL] = RoomBox.Text!;
            toSave.settings[password] = PasswordBox.Text!;
            toSave.settings[roomAddress] = thisAsync.settings[roomAddress];
            toSave.settings[roomPort] = thisAsync.settings[roomPort];
            toSave.settings[cheeseURL] = thisAsync.settings[cheeseURL];
            toSave.settings[room] = thisAsync.settings[room];
            toSave.room = thisAsync.room;


            foreach (var item in SlotsContainer.Children)
            {
                if (item is SlotHolder slotHolder)
                {
                    toSave.slots.Add(slotHolder.Slot);
                }
            }

            thisAsync = IOManager.SaveAsync(thisAsync, toSave);

            _AsyncNameBox.Text = thisAsync.settings[asyncName];
            AsyncNameBox.Text = thisAsync.settings[asyncName];
            thisAsync.room = toSave.room;
            thisAsync.settings[roomURL] = toSave.room.URL;

            foreach (var item in SlotsContainer.Children)
            {
                if (item is SlotHolder slotHolder)
                {
                    slotHolder.SetAsyncName(toSave.settings[asyncName]);
                    slotHolder.SetRoom(toSave.room);
                }
            }

            Edit.IsEnabled = true;
            isSaving = false;
        }
        catch (Exception e)
        {
            ErrorManager.ThrowError(
                "AsyncHolder - Failed to save async",
                "Trying to save this async raised the following exception : " + e.Message
                );
        }
        
        DoneSaving?.Invoke();

        Dispatcher.UIThread.Post(TurnEventsOn);
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
            Debouncer.ForceDebounce(Save);
        }
        DoneClosing?.Invoke();
    }

    public void UpdateToolList()
    {
        foreach (var item in SlotsContainer.Children)
        {
            if (item is SlotHolder slot)
            {
                slot.UpdateToolList();
            }
        }
    }

    public void UpdateSlotSelection(SlotHolder source)
    {
        foreach (var item in SlotsContainer.Children)
        {
            if(item is SlotHolder holder && holder != source)
            {
                holder.UpdateAvailableSlot();
            }
        }
        Debouncer.Debounce(Save, 1f);
    }

    public void UpdateSlotSelection()
    {
        if(SlotsContainer.Children.Count > 0 && SlotsContainer.Children[0] is SlotHolder holder)
        {
            UpdateSlotSelection(holder);
        }
    }
}