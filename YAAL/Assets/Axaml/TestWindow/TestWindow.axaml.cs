using YAAL.Assets.Script.Cache;
using YAAL.Assets.Scripts;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static YAAL.LauncherSettings;
using static YAAL.SlotSettings;
using System.Collections;
using Avalonia.Platform;
using System.Reactive.Concurrency;

namespace YAAL;

public partial class TestWindow : ScalableWindow
{
    private Cache_Async? temporaryAsync;
    private static TestWindow? _testWindow;
    private bool isGame = false;
    public TestWindow()
    {
        InitializeComponent();
        AutoTheme.SetTheme(TrueBackground, ThemeSettings.backgroundColor);

        Launch.Click += (_, _) => { LaunchTest(); };
        
        Restore.Click += (_, _) => { IOManager.RestoreAll(); };

        Temporary.Click += _SwitchMode;
        Existing.Click += _SwitchMode;

        this.Closing += (_,_) =>
        {
            if(temporaryAsync != null)
            {
                IOManager.DeleteAsync(temporaryAsync.settings[AsyncSettings.asyncName]);
            }
            _testWindow = null;
        };

        List<string> asyncList = IOManager.GetAsyncList();

        if(asyncList.Count == 0)
        {
            asyncList.Add("None available");
        }

        AsyncSelector.ItemsSource = asyncList;

        AsyncSelector.SelectionChanged += (_, _) =>
        {
            if(AsyncSelector.SelectedItem is string asyncName)
            {
                List<string> slotList = new List<string>();

                if (asyncName != "None available")
                {
                    slotList = IOManager.GetSlotList(asyncName);
                }

                if(slotList.Count == 0)
                {
                    slotList.Add("None available");
                }

                SlotSelector.ItemsSource = slotList;
                SlotSelector.SelectedIndex = 0;

                WindowManager.UpdateComboBox(SlotSelector);
            }
        };

        WindowManager.UpdateComboBox(AsyncSelector);
        AsyncSelector.SelectedIndex = 0;
    }

    public static TestWindow GetTestWindow(Window source, bool isGame)
    {
        if (_testWindow == null)
        {
            if(WindowManager.OpenWindow(WindowType.TestWindow, source) is TestWindow newWindow)
            {
                _testWindow = newWindow;
            } else
            {
                Trace.WriteLine("The new window isn't a test window. What !?");
                _testWindow = new TestWindow();
            }
        }
        else
        {
            _testWindow.Activate();
            _testWindow.Topmost = true;
            _testWindow.Topmost = false;
        }

        _testWindow.isGame = isGame;
        return _testWindow;
    }

    public void Setup(string name, List<string> newVersions, bool requiresPatch, bool requiresVersion)
    {
        LauncherName.Text = name;
        VersionSelector.ItemsSource = newVersions;
        VersionSelector.SelectedIndex = 0;

        Patch.IsEnabled = requiresPatch;
        File.IsEnabled = requiresPatch;
        VersionSelector.IsEnabled = requiresVersion;
    }

    private async void LaunchTest()
    {
        Cache_Async async;
        Cache_Slot slot;

        try
        {
            if (TemporaryAsync.IsVisible)
            {
                bool needToCreateSlot = false;

                if (temporaryAsync == null)
                {
                    Trace.WriteLine("Creating a temporary async");
                    temporaryAsync = IOManager.CreateNewAsync("_temporary");
                    needToCreateSlot = true;
                }

                if (RoomURL.Text != null && RoomURL.Text != "")
                {
                    if (WebManager.IsValidURL(RoomURL.Text!))
                    {
                        Trace.WriteLine("Parsing RoomURL as full URL");
                        temporaryAsync.settings[AsyncSettings.roomURL] = RoomURL.Text!;
                        temporaryAsync.room = await WebManager.ParseRoomURL(RoomURL.Text!);
                        temporaryAsync.settings[AsyncSettings.roomAddress] = temporaryAsync.room.address;
                        temporaryAsync.settings[AsyncSettings.roomPort] = temporaryAsync.room.port;
                        temporaryAsync.settings[AsyncSettings.room] = temporaryAsync.room.address + ":" + temporaryAsync.room.port;
                        temporaryAsync.settings[AsyncSettings.cheeseURL] = temporaryAsync.room.cheeseTrackerURL;
                    }
                    else if (RoomURL.Text!.Contains(":"))
                    {
                        Trace.WriteLine("Parsing RoomURL as IP:port");
                        var splitURL = RoomURL.Text!.Split(":");
                        if (splitURL.Length == 2)
                        {
                            temporaryAsync.settings[AsyncSettings.roomAddress] = splitURL[0];
                            temporaryAsync.settings[AsyncSettings.roomPort] = splitURL[1];
                            temporaryAsync.settings[AsyncSettings.room] = RoomURL.Text!;
                        }
                    }
                    else
                    {
                        Trace.WriteLine("Parsing RoomURL as simple string");
                        temporaryAsync.settings[AsyncSettings.room] = RoomURL.Text!;
                    }
                } else
                {
                    Trace.WriteLine("RoomURL is either null or empty");
                }

                temporaryAsync.settings[AsyncSettings.isTemporary] = true.ToString();
                Trace.WriteLine("Saving temporary async (1)");
                IOManager.SaveAsync(temporaryAsync, temporaryAsync);

                Cache_Slot temporarySlot;

                if (needToCreateSlot)
                {
                    Trace.WriteLine("Creating temporary slot");
                    temporarySlot = IOManager.CreateNewSlot(temporaryAsync, "_temporary");
                }
                else
                {
                    temporarySlot = temporaryAsync.slots[0];
                }

                temporarySlot.settings[SlotSettings.patch] = Patch.Text ?? "";

                Trace.WriteLine("Saving temporary async (2)");
                IOManager.SaveAsync(temporaryAsync, temporaryAsync);

                async = temporaryAsync;
                slot = temporarySlot;
            }
            else
            {
                if (AsyncSelector.SelectedItem is string asyncName && asyncName != "None available" && SlotSelector.SelectedItem is string slotName && slotName != "None available")
                {
                    Trace.WriteLine("Reading async information");
                    async = IOManager.GetAsync(asyncName);
                    slot = IOManager.GetSlot(asyncName, slotName);
                }
                else
                {
                    Trace.WriteLine("Async/slot is either null, None, or invalid");
                    return;
                }
            }
        }
        catch (Exception e)
        {
            ErrorManager.ThrowError(
                "TestWindow - Exception while trying to get the slot",
                "Trying to get the async and slot information raised the following exception : " + e.Message);
            return;
        }

        string args = "";

        if(async.settings.ContainsKey(AsyncSettings.asyncName) && slot.settings.ContainsKey(SlotSettings.slotName))
        {
            args = "--async " + async.settings[AsyncSettings.asyncName] + " --slot " + slot.settings[SlotSettings.slotName] + " --launcher " + LauncherName.Text;
        } else
        {
            if(!async.settings.ContainsKey(AsyncSettings.asyncName))
            {
                ErrorManager.ThrowError(
                "TestWindow - Missing setting : asyncName",
                "You tried to access an async that doesn't have a name. This shouldn't ever happen, please report this issue.");
            } else
            {
                ErrorManager.ThrowError(
                "TestWindow - Missing setting : slotName",
                "You tried to access a slot that doesn't have a name. This shouldn't ever happen, please report this issue.");
            }
            return;
        }

        if (VersionSelector.SelectedItem is string version)
        {
            Cache_Slot oldSlot = new Cache_Slot();
            oldSlot.settings[SlotSettings.slotName] = slot.settings[SlotSettings.slotName];

            slot.settings[SlotSettings.version] = version;
            slot.settings[SlotSettings.baseLauncher] = LauncherName.Text ?? "";
            IOManager.SaveSlot(async.settings[AsyncSettings.asyncName], slot, oldSlot);
        }


        try
        {
            if (isGame)
            {
                args += " --ignoreBase";
            }

            ProcessManager.StartProcess(Environment.ProcessPath, args, true, false);
        }
        catch (Exception e)
        {
            ErrorManager.ThrowError(
                "TestWindow - Starting process threw an exception",
                "Trying to launch " + Environment.ProcessPath + " with args " + args + " threw the following exception : " + e.Message);
        }
        
    }

    private void _SwitchMode(object? sender, RoutedEventArgs e)
    {
        TemporaryAsync.IsVisible = !TemporaryAsync.IsVisible;
        ExistingAsync.IsVisible = !ExistingAsync.IsVisible;
    }

    private async void _FileSelect(object? sender, RoutedEventArgs e)
    {
        Patch.Text = await IOManager.PickFile(this);
    }
}