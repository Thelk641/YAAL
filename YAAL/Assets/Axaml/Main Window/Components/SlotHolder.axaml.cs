using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YAAL.Assets.Scripts;
using static YAAL.ApworldSettings;
using static YAAL.AsyncSettings;
using static YAAL.SlotSettings;

namespace YAAL;

public partial class SlotHolder : UserControl
{
    private string asyncName;
    private Cache_Slot thisSlot;
    public Cache_Slot Slot { get { return thisSlot; } }
    private Cache_Room room;
    private List<Cache_RoomSlot> availableGames;
    public Cache_DisplaySlot selectedSlot;
    public Cache_CustomLauncher currentLauncher;
    public event Action RequestRemoval;
    public event Action SwitchedToBigger;
    public event Action SwitchedToSmaller;
    public event Action FinishedEditing;
    public event Action<double,double> ChangedHeight;
    public int hardCodedHeight = 112;
    public double previousHeight = 0;
    public bool isEditing = false;
    private ObservableCollection<string> toolList = new ObservableCollection<string>();
    private string previousTheme = "";
    
    public SlotHolder()
    {
        InitializeComponent();
    }

    public SlotHolder(Cache_Async async, Cache_Slot slot) : this()
    {
        asyncName = async.settings[AsyncSettings.asyncName];
        thisSlot = slot;
        room = async.room;

        if(LauncherSelector.SelectedItem is Cache_DisplayLauncher cache)
        {
            var test = cache.name;
        }

        UpdateAvailableSlot();

        SetupPlayMode();
        SetupEditMode();

        IOManager.UpdatedLauncher += (string updatedLauncher) =>
        {
            UpdateAvailableSlot();
        };

        AutoTheme.SetTheme(Transparent1, ThemeSettings.transparent);
        EventHandler handler = null;
        handler = (_, _) =>
        {
            Resize();
            SetScrollSpeed();
            this.LayoutUpdated -= handler;
        };
        this.LayoutUpdated += handler;

        UpdateItemList();
    }

    public void SetAsyncName(string newName)
    {
        asyncName = newName;
    }



    public void Resize()
    {
        double newHeight = hardCodedHeight;

        Cache_CustomTheme customTheme;

        if (currentLauncher != null && currentLauncher.settings.TryGetValue(LauncherSettings.customTheme, out string themeName))
        {
            customTheme = ThemeManager.LoadTheme(themeName);
        } else
        {
            customTheme = ThemeManager.GetDefaultTheme();
        }

        newHeight += (customTheme.topOffset + customTheme.bottomOffset) * 2;

        string combined = customTheme.topOffset.ToString() + ",*," + customTheme.bottomOffset.ToString();
        PlayEmptySpace1.RowDefinitions = new RowDefinitions(combined);
        PlayEmptySpace2.RowDefinitions = new RowDefinitions(combined);
        EditEmptySpace1.RowDefinitions = new RowDefinitions(combined);
        EditEmptySpace2.RowDefinitions = new RowDefinitions(combined);

        this.Height = newHeight;
        ChangedHeight?.Invoke(previousHeight, newHeight);
        previousHeight = newHeight;
    }

    public async Task AutoDownload(bool overridePatch = false)
    {
        Patch.Text = await WebManager.DownloadPatch(asyncName, thisSlot.settings[slotName], selectedSlot.cache.patchURL, overridePatch);

        if (!overridePatch)
        {
            DownloadPatch.IsVisible = false;
            ReDownloadPatch.IsVisible = true;
        }
        
        Save();
    }

    public void ClosingSave()
    {
        if (!PlayMode.IsVisible)
        {
            Save();
        }
    }

    public void Save()
    {
        Cache_Slot newSlot = new Cache_Slot();
        if (LauncherSelector.SelectedItem is Cache_DisplayLauncher cacheLauncher)
        {
            newSlot.settings[baseLauncher] = cacheLauncher.name;
        }

        if (SelectedVersion.SelectedItem is string selectedVersion)
        {
            newSlot.settings[version] = selectedVersion;
        }

        newSlot.settings[patch] = Patch.Text;
        newSlot.settings[slotName] = SlotName.Text;
        newSlot.settings[rom] = thisSlot.settings[rom];
        string newName = IOManager.SaveSlot(asyncName, newSlot, thisSlot);

        newSlot.settings[slotName] = newName;
        SlotName.Text = newName;
        _SlotName.Text = newName;

        thisSlot = newSlot;
    }

    public void SetBackgrounds()
    {
        //TODO
    }

    public void SetRoom(Cache_Room newRoom)
    {
        if (!isEditing)
        {
            DebouncedSetRoom(newRoom);
            return;
        }
        else
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
        if (room.slots.Count == 0)
        {
            SwitchPatchMode(true);
        }

        ToolSelect.SelectedIndex = 0;
    }

    public void SetupPlayMode()
    {
        _SlotName.Text = thisSlot.settings[slotName];

        if(WindowManager.GetMainWindow() is MainWindow window)
        {
            UpdateToolList();
        } else
        {
            WindowManager.DoneStarting += () =>
            {
                UpdateToolList();
            };
        }



        RealPlay.Click += async (_, _) =>
        {
            if (_SlotName.Text == "")
            {
                return;
            }

            if (currentLauncher.requiresPatch && thisSlot.settings[SlotSettings.patch] == "")
            {
                if (SlotSelector.SelectedItem is Cache_DisplaySlot selected && selected.cache.patchURL != "" && AutomaticPatch.IsVisible)
                {
                    await AutoDownload();
                }
                else
                {
                    ErrorManager.ThrowError(
                        "SlotHolder - No patch selected",
                        "You tried to launch a game that uses automatic patching, but haven't selected a patch. This is not allowed. Aborting launch.");
                    return;
                }
            }

            if (currentLauncher.requiresVersion && thisSlot.settings[SlotSettings.version] == "None installed")
            {
                ErrorManager.ThrowError(
                        "SlotHolder - No version selected",
                        "You tried to launch a game that uses automatic versionning or automatic patching, but haven't installed any version yet. This is not allowed. Aborting launch.");
                return;
            }


            ProcessManager.StartProcess(
                Environment.ProcessPath!,
                ("--async " + "\"" + asyncName + "\"" + " --slot " + "\"" + _SlotName.Text + "\""),
                true);
        };

        StartTool.Click += (_, _) =>
        {
            if (ToolSelect.SelectedItem == null)
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
                    if (room.cheeseTrackerURL != "")
                    {
                        ProcessManager.StartProcess(room.cheeseTrackerURL, "", true);
                    }
                    break;
                default:
                    ProcessManager.StartProcess(
                        Environment.ProcessPath!,
                        ("--async " + "\"" + asyncName + "\"" + " --slot " + "\"" + _SlotName.Text + "\"" + " --launcher " + "\"" + ToolSelect.SelectedItem.ToString() + "\""),
                        true);
                    break;
            }
        };

        UpdateItems.Click += (_, _) =>
        {
            UpdateItemList();
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

        Action finished = () =>
        {
            isEditing = false;
            Save();
            FinishedEditing?.Invoke();
        };

        UpdateAvailableLaunchers();

        PatchSelect.Click += async (_, _) =>
        {
            isEditing = true;
            Patch.Text = await IOManager.PickFile(this.VisualRoot as Window);
            finished();
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

        LauncherSelector.SelectionChanged += _ChangedLauncher;
        SlotSelector.SelectionChanged += _ChangedSlot;

        SlotName.TextChanged += (_, _) =>
        {
            _SlotName.Text = SlotName.Text;
            isEditing = true;
            Debouncer.Debounce(finished, 1);
        };

        DownloadPatch.Click += async (_, _) =>
        {
            AutoDownload(false);
        };

        ReDownloadPatch.Click += async (_, _) =>
        {
            AutoDownload(true);
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

    public async void UpdateItemList()
    {
        if (!room.slots.TryGetValue(thisSlot.settings[slotName], out Cache_RoomSlot roomSlot) || roomSlot == null){
            return;
        }

        UpdateItems.IsEnabled = false;
        Cache_ItemTracker cache = await WebManager.ParseTrackerItems(roomSlot.trackerURL);
        TrackerItemHolder.Children.Clear();
        foreach (var item in cache.items)
        {
            TextBox box = new TextBox();
            box.Text = item;
            box.IsVisible = true;
            TrackerItemHolder.Children.Add(box);
            box.SetValue(AutoTheme.AutoThemeProperty, null);
            box.Background = new SolidColorBrush(Colors.Transparent);

            int height = 30;
            box.Height = height;
            box.MinHeight = height;
            box.MaxHeight = height;
            box.TextWrapping = TextWrapping.NoWrap;
            box.AcceptsReturn = false;
            box.VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center;
            box.Margin = new Thickness(0, 2, 0, 2);
        }
        UpdateItems.IsEnabled = true;
    }
    public void SwitchMode()
    {
        if (PlayMode.IsVisible)
        {
            PlayMode.IsVisible = false;
            EditMode.IsVisible = true;
        }
        else
        {
            PlayMode.IsVisible = true;
            EditMode.IsVisible = false;
        }
    }

    public void SwitchPatchMode()
    {
        SwitchPatchMode(AutomaticPatch.IsVisible);
    }

    public void SwitchPatchMode(bool manual)
    {
        if (manual)
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

    public void UpdateAvailableSlot()
    {
        // TODO : needs to check that this actually works, not sure it does
        List<Cache_RoomSlot> filteredSlots = new List<Cache_RoomSlot>();
        List<Cache_RoomSlot> unfilteredSlots = new List<Cache_RoomSlot>();
        List<string> games = IOManager.GetGameList();
        List<string> slots = IOManager.GetSlotList(asyncName);

        foreach (var item in room.slots)
        {
            if (slots.Contains(item.Key) && item.Key != thisSlot.settings[slotName])
            {
                // This item already has another SlotHolder dedicated to it, let's ignore it
                continue;
            } else
            {
                if (games.Contains(item.Value.gameName))
                {
                    filteredSlots.Add(item.Value);
                } else
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
            header.slotName = "-- Implemented";
            possibleSlots.Add(header);
        }

        foreach (var item in filteredSlots)
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

        if (unfilteredSlots.Count > 0)
        {
            Cache_DisplaySlot header = new Cache_DisplaySlot();
            header.slotName = "-- Not implemented";
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

        if(possibleSlots.Count == 0)
        {
            Cache_RoomSlot emptySlot = new Cache_RoomSlot();
            emptySlot.slotName = "None";

            Cache_DisplaySlot empty = new Cache_DisplaySlot();
            empty.SetSlot(emptySlot);

            possibleSlots.Add(empty);
        }

        SlotSelector.ItemsSource = possibleSlots;

        if (selected != null)
        {
            SlotSelector.SelectedItem = selected;
        }
        else
        {
            if(possibleSlots.Count == 1)
            {
                SlotSelector.SelectedIndex = 0;
            } else
            {
                SlotSelector.SelectedIndex = 1;
            }
                
        }
    }
    
    private void UpdateAvailableLaunchers()
    {
        List<string> launcherList = IOManager.GetLauncherList();

        if (launcherList.Count > 0 && SlotSelector.SelectedItem is Cache_DisplaySlot displaySlot)
        {
            List<Cache_DisplayLauncher> list = IOManager.GetLaunchersForGame(displaySlot.cache.gameName);
            LauncherSelector.ItemsSource = list;

            foreach (var item in list)
            {
                if (item.name == thisSlot.settings[baseLauncher])
                {
                    LauncherSelector.SelectedItem = item;
                    break;
                }
            }

            _ChangedLauncher(null, null);
        }
        else
        {
            LauncherSelector.ItemsSource = new List<string>()
            {
                "None"
            };

            SelectedVersion.ItemsSource = new List<string>()
            {
                "None"
            };

            LauncherSelector.SelectedItem = "None";
            SelectedVersion.SelectedItem = "None";
        }
    }

    private void UpdateAvailableVersions()
    {
        if(LauncherSelector.SelectedItem is Cache_DisplayLauncher cache)
        {
            var prevSelection = SelectedVersion.SelectedItem;
            List<string> versions = IOManager.GetVersions(cache.name);

            if(versions.Count == 0)
            {
                versions.Add("None installed");
            }

            SelectedVersion.ItemsSource = versions;

            if (versions.Contains(thisSlot.settings[version]))
            {
                SelectedVersion.SelectedItem = thisSlot.settings[version];
            }
            else
            {
                SelectedVersion.SelectedIndex = 0;
            }
            if (prevSelection != null)
            {
                Save();
            }
            currentLauncher = IOManager.LoadCacheLauncher(cache.name);
        }
    }

    // EVENTS

    private void _ChangedSlot(object? sender, SelectionChangedEventArgs e)
    {
        if (SlotSelector.SelectedItem == null)
        {
            return;
        }
        selectedSlot = (Cache_DisplaySlot)SlotSelector.SelectedItem;

        if(selectedSlot.slotName != "None")
        {
            SlotName.Text = selectedSlot.slotName;
        }
        
        DownloadPatch.IsEnabled = selectedSlot.cache.patchURL != "";
        ReDownloadPatch.IsEnabled = selectedSlot.cache.patchURL != "";

        UpdateAvailableLaunchers();

        if (selectedSlot.cache.trackerURL != "")
        {
            if (!toolList.Contains("Tracker"))
            {
                ListSorter.AddSorted(toolList, "Tracker");
            }
            thisSlot.settings[SlotSettings.slotTrackerURL] = selectedSlot.cache.trackerURL;
        }
        else
        {
            if (toolList.Contains("Tracker"))
            {
                toolList.Remove("Tracker");
            }
            thisSlot.settings[SlotSettings.slotTrackerURL] = "";
        }

        ToolSelect.SelectedIndex = 0;

        UpdateItemList();

        if (LauncherSelector.SelectedItem == null && LauncherSelector.ItemsSource is List<Cache_DisplayLauncher> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].isHeader)
                {
                    LauncherSelector.SelectedItem = list[i];
                    break;
                }
            }
        }
    }

    private void _ChangedLauncher(object? sender, SelectionChangedEventArgs e)
    {
        if (LauncherSelector.SelectedItem == null)
        {
            return;
        }

        UpdateAvailableVersions();
        
        SetBackgrounds();
    }

    private void SetScrollSpeed()
    {
        Viewer.RemoveHandler(ScrollViewer.PointerWheelChangedEvent, AdjustedScrollSpeed);

        Viewer.AddHandler(ScrollViewer.PointerWheelChangedEvent, AdjustedScrollSpeed, RoutingStrategies.Tunnel);
    }

    private void AdjustedScrollSpeed(object? sender, PointerWheelEventArgs e)
    {
        double wheelFactor = 10f * App.Settings.Zoom;

        var sv = (ScrollViewer)sender!;
        var oldOffset = sv.Offset;                
        double delta = e.Delta.Y;                  

        double newY = oldOffset.Y - (delta * wheelFactor);

        if (newY < 0) newY = 0;

        sv.Offset = new Vector(oldOffset.X, newY);

        e.Handled = true;
    }

    public async void UpdateToolList()
    {
        ToolSelect.ItemsSource = IOManager.GetToolList().Result;
        ToolSelect.SelectedIndex = 0;
    }
}