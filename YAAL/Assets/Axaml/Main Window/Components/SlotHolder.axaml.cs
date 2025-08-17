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

namespace YAAL;

public partial class SlotHolder : UserControl
{
    private string asyncName;
    private Cache_Slot thisSlot;
    private Cache_Room room;
    private List<Cache_RoomSlot> availableGames;
    public Cache_DisplaySlot selectedSlot;
    public event Action RequestRemoval;
    public event Action SwitchedToBigger;
    public event Action SwitchedToSmaller;
    public event Action FinishedEditing;
    public int baseHeight = 52;
    public int heightDifference = 38;
    public bool isEditing = false;
    private ObservableCollection<string> toolList = new ObservableCollection<string>();
    
    public SlotHolder()
    {
        InitializeComponent();
    }

    public SlotHolder (Cache_Async async, Cache_Slot slot) : this()
    {
        asyncName = async.settings[AsyncSettings.asyncName];
        thisSlot = slot;
        room = async.room;

        SetupPlayMode();
        SetupEditMode();

        if (room.slots.Count > 0) {
            UpdateAvailableSlot();
        }

        _ChangedSlot(null, null);
    }

    public void SetAsyncName(string newName)
    {
        asyncName = newName;
    }

    public void SetRoom(Cache_Room newRoom)
    {
        if (!isEditing)
        {
            DebouncedSetRoom(newRoom);
            return;
        } else
        {
            Action handler = null;  
            
            handler = () =>
            {
                DebouncedSetRoom(newRoom);
                FinishedEditing -= handler;
            };
            FinishedEditing += handler;
        }

            
    }

    public void DebouncedSetRoom(Cache_Room newRoom)
    {
        room = newRoom;
        UpdateAvailableSlot();
        if (room.slots.Count > 0) 
        {
            SwitchPatchMode();
        }

        if (room.cheeseTrackerURL != "")
        {
            if (!toolList.Contains("Cheesetracker"))
            {
                ListSorter.AddSorted(toolList, "Cheesetracker");
            }
        }
        else
        {
            if (toolList.Contains("Cheesetracker"))
            {
                toolList.Remove("Cheesetracker");
            }
        }

        ToolSelect.SelectedIndex = 0;
    }

    public void UpdateAvailableSlot()
    {
        // TODO : this is going to be slow and done once per slot every time we open the software
        // Need to find a way to cache it with the time of last update of launcher list
        // Which also means caching the launcher list with a time
        List<string> patterns = IOManager.SplitPathList(IOManager.GetSetting(GeneralSettings.slotNameFilter));
        List<Cache_RoomSlot> filteredSlots = new List<Cache_RoomSlot>();
        List<Cache_RoomSlot> unfilteredSlots = new List<Cache_RoomSlot>();
        List<string> games = IOManager.GetGameList();
        List<string> slots = IOManager.GetSlotList(asyncName);

        foreach (var item in room.slots)
        {
            if (slots.Contains(item.Key) && item.Key != thisSlot.settings[slotName])
            {
                continue;
            }

            if (patterns.Any(p => item.Key.Contains(p)))
            {
                filteredSlots.Add(item.Value);
                continue;
            }
            else
            {
                if (games.Contains(item.Value.gameName))
                {
                    unfilteredSlots.Add(item.Value);
                }
            }
        }

        availableGames = new List<Cache_RoomSlot>();
        List<Cache_DisplaySlot> possibleSlots = new List<Cache_DisplaySlot>();
        Cache_DisplaySlot selected = null;

        if (filteredSlots.Count > 0) 
        {
            Cache_DisplaySlot header = new Cache_DisplaySlot();
            header.slotName = "-- Filtered";
            possibleSlots.Add(header);
        }

        foreach (var item in filteredSlots)
        {
            availableGames.Add(item);
            Cache_DisplaySlot toAdd = new Cache_DisplaySlot();
            toAdd.SetSlot(item);
            possibleSlots.Add(toAdd);
            if(selected == null && item.slotName == _SlotName.Text)
            {
                selected = toAdd;
            }
        }

        if (unfilteredSlots.Count > 0)
        {
            Cache_DisplaySlot header = new Cache_DisplaySlot();
            header.slotName = "-- Unfiltered";
            possibleSlots.Add(header);
        }

        foreach (var item in unfilteredSlots)
        {
            availableGames.Add(item);
            Cache_DisplaySlot toAdd = new Cache_DisplaySlot();
            toAdd.SetSlot(item);
            possibleSlots.Add(toAdd);
            if (selected == null && item.slotName == _SlotName.Text)
            {
                selected = toAdd;
            }
        }

        SlotSelector.ItemsSource = possibleSlots;

        if(selected != null)
        {
            SlotSelector.SelectedItem = selected;
        } else
        {
            if(possibleSlots.Count == 0)
            {
                SlotSelector.SelectedIndex = 0;
            } else
            {
                SlotSelector.SelectedIndex = 1;
            }
        }
    }

    public void SetupPlayMode()
    {
        _SlotName.Text = thisSlot.settings[slotName];

        foreach (var item in IOManager.GetToolList())
        {
            toolList.Add(item);
        }

        if(toolList.Count == 0)
        {
            toolList.Add("None");
        }

        ToolSelect.ItemsSource = toolList;
        

        RealPlay.Click += async (_, _) =>
        {
            if (_SlotName.Text == "")
            {
                return;
            }
            if (thisSlot.settings[SlotSettings.patch] == "" && selectedSlot.cache.patchURL != "" && AutomaticPatch.IsVisible)
            {
                await AutoDownload();
            }
            ProcessManager.StartProcess(
                Environment.ProcessPath,
                ("--async " + "\"" + asyncName + "\"" + " --slot " + "\"" + _SlotName.Text + "\""),
                true);
        };

        StartTool.Click += (_, _) =>
        {
            if (ToolSelect.SelectedItem == "None")
            {
                return;
            }

            switch (ToolSelect.SelectedItem.ToString())
            {
                case "None":
                    return;
                case "Tracker":
                    if (selectedSlot.cache.trackerURL != "")
                    {
                        ProcessManager.StartProcess(selectedSlot.cache.trackerURL, "", true);
                    }
                    break;
                case "Cheesetracker":
                    if(room.cheeseTrackerURL != "")
                    {
                        ProcessManager.StartProcess(room.cheeseTrackerURL, "", true);
                    }
                    break;
                default:
                    ProcessManager.StartProcess(
                        Environment.ProcessPath,
                        ("--async " + "\"" + asyncName + "\"" + " --slot " + "\"" + _SlotName.Text + "\"" + " --launcher " + "\"" + ToolSelect.SelectedItem.ToString() + "\""),
                        true);
                    break;
            } 
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

        if (launcherList.Count > 0)
        {
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
        }
        else
        {
            SelectedLauncher.ItemsSource = new List<string>()
            {
                "None"
            };

            SelectedVersion.ItemsSource = new List<string>()
            {
                "None"
            };

            SelectedLauncher.SelectedItem = "None";
            SelectedVersion.SelectedItem = "None";
        }

        PatchSelect.Click += async (_, _) =>
        {
            isEditing = true;
            Patch.Text = await IOManager.PickFile(this.VisualRoot as Window);
            DebouncedFinishedEditing();
        };

        DoneEditing.Click += (_, _) =>
        {
            Save();
            SwitchMode();
        };

        DeleteSlot.Click += (_, _) =>
        {
            ConfirmationWindow confirm = new ConfirmationWindow(thisSlot.settings[slotName]);
            confirm.Closing += (_, _) =>
            {
                if (confirm.confirmed)
                {
                    IOManager.DeleteSlot(asyncName, thisSlot.settings[slotName]);
                    RequestRemoval?.Invoke();
                }
            };
        };

        SelectedLauncher.SelectionChanged += _ChangedLauncher;
        SlotSelector.SelectionChanged += _ChangedSlot;

        SlotName.TextChanged += (_, _) =>
        {
            _SlotName.Text = SlotName.Text;
            isEditing = true;
            Debouncer.Debounce(DebouncedFinishedEditing, 1);
        };

        DownloadPatch.Click += async (_, _) => 
        {
            AutoDownload();
        };

        ReDownloadPatch.Click += async (_, _) =>
        {
            Patch.Text = await WebManager.DownloadPatch(asyncName, thisSlot.settings[slotName], selectedSlot.cache.patchURL, true);
            IOManager.SetSlotSetting(asyncName, thisSlot.settings[slotName], rom, "");
            Save();
        };

        if (thisSlot.settings[patch] != "")
        {
            DownloadPatch.IsVisible = false;
            ReDownloadPatch.IsVisible = true;
        }

        AutomaticPatchButton.Click += (_, _) =>
        {
            SwitchPatchMode();
        };

        ManualPatchButton.Click += (_, _) =>
        {
            SwitchPatchMode();
        };

        if (room == null || room.slots.Count == 0)
        {
            SwitchPatchMode();
        }
    }

    public async Task AutoDownload()
    {
        Patch.Text = await WebManager.DownloadPatch(asyncName, thisSlot.settings[slotName], selectedSlot.cache.patchURL);
        DownloadPatch.IsVisible = false;
        ReDownloadPatch.IsVisible = true;
        Save();
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

    public void Save()
    {
        Cache_Slot newSlot = new Cache_Slot();
        newSlot.settings[baseLauncher] = SelectedLauncher.SelectedItem.ToString();
        newSlot.settings[version] = SelectedVersion.SelectedItem.ToString();
        newSlot.settings[patch] = Patch.Text;
        newSlot.settings[slotName] = SlotName.Text;
        newSlot.settings[rom] = thisSlot.settings[rom];
        string newName = IOManager.SaveSlot(asyncName, newSlot, thisSlot);

        newSlot.settings[slotName] = newName;
        SlotName.Text = newName;
        _SlotName.Text = newName;

        thisSlot = newSlot;
    }

    public void ClosingSave()
    {
        if (!PlayMode.IsVisible)
        {
            Save();
        }
    }

    public void DebouncedFinishedEditing()
    {
        isEditing = false;
        FinishedEditing?.Invoke();
    }

    private void SetColor()
    {
        Cache_CustomLauncher cache = IOManager.LoadCacheLauncher(thisSlot.settings[SlotSettings.baseLauncher]);
        if (cache.customSettings.ContainsKey(GeneralSettings.backgroundColor.ToString()))
        {
            BackgroundSetter.SetBackground(BackgroundColor, cache.customSettings[GeneralSettings.backgroundColor.ToString()]);
            return;
        } else if (cache.customSettings.ContainsKey(GeneralSettings.foregroundColor.ToString()))
        {
            BackgroundSetter.SetBackground(BackgroundColor, cache.customSettings[GeneralSettings.foregroundColor.ToString()]);
            return;
        } else
        {
            BackgroundSetter.SetBackground(BackgroundColor, GeneralSettings.foregroundColor);
        }
    }
    
    private void _ChangedSlot(object? sender, SelectionChangedEventArgs e)
    {
        if (SlotSelector.SelectedItem == null)
        {
            return;
        }
        selectedSlot = (Cache_DisplaySlot)SlotSelector.SelectedItem;

        SlotName.Text = selectedSlot.slotName;
        DownloadPatch.IsEnabled = selectedSlot.cache.patchURL != "";
        ReDownloadPatch.IsEnabled = selectedSlot.cache.patchURL != "";

        SelectedLauncher.ItemsSource = IOManager.GetLaunchersForGame(selectedSlot.cache.gameName);
        SelectedLauncher.SelectedIndex = 0;

        if (selectedSlot.cache.trackerURL != "")
        {
            if (!toolList.Contains("Tracker"))
            {
                ListSorter.AddSorted(toolList, "Tracker");
            }
        } else
        {
            if (toolList.Contains("Tracker"))
            {
                toolList.Remove("Tracker");
            }
        }

        

        ToolSelect.SelectedIndex = 0;
    }

    private void _ChangedLauncher(object? sender, SelectionChangedEventArgs e)
    {
        if(SelectedLauncher.SelectedItem == null)
        {
            return;
        }
        List<string> versions = IOManager.GetDownloadedVersions(SelectedLauncher.SelectedItem.ToString());
        SelectedVersion.ItemsSource = versions;

        if (versions.Contains(thisSlot.settings[version]))
        {
            SelectedVersion.SelectedItem = thisSlot.settings[version];
        }
        else
        {
            SelectedVersion.SelectedIndex = 0;
        }
        Save();
        SetColor();
    }

    public Cache_Slot GetCache()
    {
        return thisSlot;
    }
}