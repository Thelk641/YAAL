using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Utilities;
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
    private AsyncHolder holder;
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
    private string previousName = "";
    private bool starting = true;
    
    public SlotHolder()
    {
        InitializeComponent();
    }

    public SlotHolder(Cache_Async async, Cache_Slot slot, AsyncHolder holder) : this()
    {
        asyncName = async.settings[AsyncSettings.asyncName];
        thisSlot = slot;
        room = async.room;
        this.holder = holder;
        previousName = slot.settings[SlotSettings.slotName];

        SetupPlayMode();
        SetupEditMode();

        IOManager.UpdatedLauncher += (string updatedLauncher) =>
        {
            UpdateAvailableSlot();
            UpdateAvailableVersions();
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

        starting = false;
        
        Dispatcher.UIThread.Post(() =>  { UpdateTheme(); });
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
            customTheme = new Cache_CustomTheme();
        }

        newHeight += (customTheme.topOffset + customTheme.bottomOffset);

        string combined = customTheme.topOffset.ToString() + ",*," + customTheme.bottomOffset.ToString();
        PlayEmptySpace.RowDefinitions = new RowDefinitions(combined);
        EditEmptySpace.RowDefinitions = new RowDefinitions(combined);

        this.Height = newHeight;
        ChangedHeight?.Invoke(previousHeight, newHeight);
        previousHeight = newHeight;
        //Debug.WriteLine("New height : " + newHeight);
        ThemeHolder.InvalidateArrange();
        ThemeHolder.InvalidateMeasure();
        ThemeHolder.Measure(new Size(0, newHeight));
        ThemeHolder.Arrange(new Rect(0, 0, 0, newHeight));
        //Debug.WriteLine("ThemeHolder : " + ThemeHolder.Bounds);
        
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
        if (starting)
        {
            return;
        }

        Cache_Slot newSlot = new Cache_Slot();
        if (LauncherSelector.SelectedItem is Cache_DisplayLauncher cacheLauncher)
        {
            newSlot.settings[baseLauncher] = cacheLauncher.name;
        } else
        {
            newSlot.settings[baseLauncher] = thisSlot.settings[baseLauncher];
        }

        if (SelectedVersion.SelectedItem is string selectedVersion)
        {
            newSlot.settings[version] = selectedVersion;
        } else
        {
            newSlot.settings[version] = thisSlot.settings[version];
        }



        newSlot.settings[patch] = Patch.Text ?? "";
        newSlot.settings[slotName] = SlotName.Text ?? "";

        if(selectedSlot != null)
        {
            newSlot.settings[slotTrackerURL] = selectedSlot.cache.trackerURL;
        } else
        {
            newSlot.settings[slotTrackerURL] = thisSlot.settings[slotTrackerURL];
        }

        // if the patch changes, the rom will be updated somewhere else
        newSlot.settings[rom] = thisSlot.settings[rom];


        string newName = IOManager.SaveSlot(asyncName, newSlot, thisSlot);

        newSlot.settings[slotName] = newName;
        SlotName.Text = newName;
        _SlotName.Text = newName;

        thisSlot = newSlot;
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
        if (newRoom.slots.ContainsKey(thisSlot.settings[slotName]) && !thisSlot.settings.ContainsKey(slotTrackerURL))
        {
            thisSlot.settings[slotTrackerURL] = newRoom.slots[thisSlot.settings[slotName]].trackerURL;
        }
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
            try
            {
                if (_SlotName.Text == "")
                {
                    return;
                }

                if (currentLauncher.settings[LauncherSettings.requiresPatch] == true.ToString() && thisSlot.settings[SlotSettings.patch] == "")
                {
                    if (SlotSelector.SelectedItem is Cache_DisplaySlot selected && selected.cache.patchURL != "" && AutomaticPatch.IsVisible)
                    {
                        await AutoDownload();
                    }
                    else
                    {
                        ErrorManager.ThrowError(
                            "SlotHolder - No patch selected",
                            "You haven't selected a patch. By default every launcher is set to require one, you can change this in the launcher's settings.");
                        return;
                    }
                }

                if (currentLauncher.settings[LauncherSettings.requiresVersion] == true.ToString() && thisSlot.settings[SlotSettings.version] == "None installed")
                {
                    ErrorManager.ThrowError(
                        "SlotHolder - No version selected",
                        "You haven't installed a version. By default every launcher is set to require one, you can change this in the launcher's settings.");
                    return;
                }


                ProcessManager.StartProcess(
                    Environment.ProcessPath!,
                    ("--async " + "\"" + asyncName + "\"" + " --slot " + "\"" + _SlotName.Text + "\""),
                    true);
            }
            catch (Exception e)
            {
                ErrorManager.ThrowError(
                    "SlotHolder - Trying to launch game raised an exception",
                    "Trying to open slot " + _SlotName.Text + " raised the following exception : " + e.Message);
            }
        };

        StartTool.Click += (_, _) =>
        {
            if (ToolSelect.SelectedItem == null)
            {
                return;
            }

            if(ToolSelect.SelectedItem is Cache_DisplayLauncher cache && !cache.isHeader)
            {
                ProcessManager.StartProcess(
                        Environment.ProcessPath!,
                        ("--async " + "\"" + asyncName 
                        + "\"" + " --slot " + "\"" + _SlotName.Text 
                        + "\"" + " --launcher " + "\"" + cache.name + "\""),
                        true);
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
            if (WindowManager.OpenWindow(WindowType.ConfirmationWindow, WindowManager.GetMainWindow()) is ConfirmationWindow confirm)
            {
                confirm.Setup(thisSlot.settings[slotName]);

                confirm.Closing += (_, _) =>
                {
                    if (confirm.confirmed)
                    {
                        IOManager.DeleteSlot(asyncName, thisSlot.settings[slotName]);
                        RequestRemoval?.Invoke();
                    }
                };
            }
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

        _ChangedSlot(null, null);
    }

    public async void UpdateItemList()
    {
        if (starting || !room.slots.TryGetValue(thisSlot.settings[slotName], out Cache_RoomSlot roomSlot) || roomSlot == null){
            return;
        }

        UpdateItems.IsEnabled = false;
        Cache_ItemTracker cache;
        if (thisSlot.settings.ContainsKey(slotTrackerURL))
        {
            cache = await WebManager.ParseTrackerItems(thisSlot.settings[slotTrackerURL]);
        }
        else
        {
            cache = await WebManager.ParseTrackerItems(roomSlot.trackerURL);
        }

        if (!thisSlot.settings.ContainsKey(slotTrackerURL) || thisSlot.settings[slotTrackerURL] != cache.trackerURL)
        {
            thisSlot.settings[slotTrackerURL] = cache.trackerURL;
            Save();
        }
        TrackerItemHolder.Children.Clear();
        foreach (var item in cache.items)
        {
            TextBox box = new TextBox();
            box.Text = item;
            box.IsVisible = true;
            TrackerItemHolder.Children.Add(box);
            AutoTheme.SetTheme(box, ThemeSettings.off);
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

    public void UpdateSlotInfo()
    {
        selectedSlot = (Cache_DisplaySlot)SlotSelector.SelectedItem;


        if (selectedSlot.slotName != "None")
        {
            SlotName.Text = selectedSlot.slotName;
        }

        DownloadPatch.IsEnabled = selectedSlot.cache.patchURL != "";
        ReDownloadPatch.IsEnabled = selectedSlot.cache.patchURL != "";

        UpdateAvailableLaunchers();

        ToolSelect.SelectedIndex = 0;

        TrackerItemHolder.Children.Clear();

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
        if (!starting)
        {
            holder.UpdateSlotSelection(this);
        }

        Save();
    }

    public void UpdateAvailableSlot()
    {
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

        if (room.slots.Count == 0) {
            Cache_RoomSlot emptySlot = new Cache_RoomSlot();
            emptySlot.slotName = "None";

            Cache_DisplaySlot empty = new Cache_DisplaySlot();
            empty.SetSlot(emptySlot);

            possibleSlots.Add(empty);
            return;
        }

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

        if(SlotSelector.ItemsSource is List<Cache_DisplaySlot> oldList)
        {
            
            if(oldList.SequenceEqual(possibleSlots))
            {
                // if slot A changes, it triggers slot B to update its slot list to hide slot A's slot from the list
                // this, in return, triggers _changedSlot
                // which triggers slot A to update its slot list, which would restart the loop
                // this is a bad way to solve this, but it works, so good enough
                return;
            }
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
        if (SlotSelector.SelectedItem == null || starting)
        {
            return;
        }
        UpdateSlotInfo();
    }

    private void _ChangedLauncher(object? sender, SelectionChangedEventArgs e)
    {
        if (LauncherSelector.SelectedItem == null)
        {
            return;
        }

        UpdateAvailableVersions();
        
        UpdateTheme();
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

    public void UpdateToolList()
    {
        ToolSelect.ItemsSource = IOManager.GetToolList().Result;
        ToolSelect.SelectedIndex = 0;
    }

    public void UpdateTheme()
    {
        if (starting)
        {
            return;
        }

        if(currentLauncher != null && currentLauncher.settings.ContainsKey(LauncherSettings.customTheme))
        {
            string themeName = currentLauncher.settings[LauncherSettings.customTheme];
            //ThemeManager.ApplyTheme(ThemeHolder, themeName);
            //ThemeManager.ApplyTheme(PlayMode, themeName);
            ThemeManager.ApplyTheme(this, themeName);
        } else
        {
            ThemeManager.ApplyTheme(this, "");
        }
    }

    public Border GetBackgrounds()
    {
        return ThemeHolder;
    }

    public List<Border> GetForegrounds()
    {
        return new List<Border>() { Foreground, EditRow1, EditRow2 };
    }

    public Border GetPlayMode()
    {
        return PlayMode;
    }

    public Border GetEditMode()
    {
        return EditMode;
    }

    public List<Button> GetButtons()
    {
        return new List<Button>() 
        {
            RealPlay,
            StartTool,
            Edit,
            UpdateItems,
            DeleteSlot,
            PatchSelect,
            DownloadPatch,
            ReDownloadPatch,
            ManualPatchButton,
            AutomaticPatchButton,
            DoneEditing
        };
    }

    public List<ComboBox> GetComboBox()
    {
        return new List<ComboBox>()
        {
            ToolSelect,
            SlotSelector,
            LauncherSelector,
            SelectedVersion
        };
    }
}