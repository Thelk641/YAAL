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

namespace YAAL;

public partial class TestWindow : ScalableWindow
{
    private Cache_Async? temporaryAsync;
    private static TestWindow? _testWindow;
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

    public static TestWindow GetTestWindow(Window source)
    {
        if (_testWindow == null)
        {
            if(WindowManager.OpenWindow(WindowType.TestWindow, source) is TestWindow newWindow)
            {
                _testWindow = newWindow;
            } else
            {
                Debug.WriteLine("The new window isn't a test window. What !?");
                _testWindow = new TestWindow();
            }
        }
        else
        {
            _testWindow.Activate();
            _testWindow.Topmost = true;
            _testWindow.Topmost = false;
        }

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

        if (TemporaryAsync.IsVisible)
        {
            bool needToCreateSlot = false;

            if(temporaryAsync == null)
            {
                temporaryAsync = IOManager.CreateNewAsync("_temporary");
                needToCreateSlot = true;
            }

            if(RoomURL.Text != "")
            {
                if (WebManager.IsValidURL(RoomURL.Text!))
                {
                    temporaryAsync.settings[AsyncSettings.roomURL] = RoomURL.Text!;
                    temporaryAsync.room = await WebManager.ParseRoomURL(RoomURL.Text!);
                    temporaryAsync.settings[AsyncSettings.roomAddress] = temporaryAsync.room.address;
                    temporaryAsync.settings[AsyncSettings.roomPort] = temporaryAsync.room.port;
                    temporaryAsync.settings[AsyncSettings.room] = temporaryAsync.room.address + ":" + temporaryAsync.room.port;
                    temporaryAsync.settings[AsyncSettings.cheeseURL] = temporaryAsync.room.cheeseTrackerURL;
                } else if (RoomURL.Text!.Contains(":"))
                {
                    var splitURL = RoomURL.Text!.Split(":");
                    if(splitURL.Length == 2)
                    {
                        temporaryAsync.settings[AsyncSettings.roomAddress] = splitURL[0];
                        temporaryAsync.settings[AsyncSettings.roomPort] = splitURL[1];
                        temporaryAsync.settings[AsyncSettings.room] = RoomURL.Text!;
                    }
                } else
                {
                    temporaryAsync.settings[AsyncSettings.room] = RoomURL.Text!;
                }
            }

            temporaryAsync.settings[AsyncSettings.isTemporary] = true.ToString();
            IOManager.SaveAsync(temporaryAsync, temporaryAsync);

            Cache_Slot temporarySlot;

            if (needToCreateSlot)
            {
                temporarySlot = IOManager.CreateNewSlot(temporaryAsync, "_temporary");
            } else
            {
                temporarySlot = temporaryAsync.slots[0];
            }

            temporarySlot.settings[SlotSettings.patch] = Patch.Text ?? "";

            IOManager.SaveAsync(temporaryAsync, temporaryAsync);

            async = temporaryAsync;
            slot = temporarySlot;
        } else
        {
            if(AsyncSelector.SelectedItem is string asyncName && asyncName != "None available" && SlotSelector.SelectedItem is string slotName && slotName != "None available")
            {
                async = IOManager.GetAsync(asyncName);
                slot = IOManager.GetSlot(asyncName, slotName);
            } else
            {
                return;
            }
        }

        if(VersionSelector.SelectedItem is string version)
        {
            Cache_Slot oldSlot = new Cache_Slot();
            oldSlot.settings[SlotSettings.slotName] = slot.settings[SlotSettings.slotName];

            slot.settings[SlotSettings.version] = version;
            slot.settings[SlotSettings.baseLauncher] = LauncherName.Text ?? "";
            IOManager.SaveSlot(async.settings[AsyncSettings.asyncName], slot, oldSlot);
        }

        string args = "--async " + async.settings[AsyncSettings.asyncName] + " --slot " + slot.settings[SlotSettings.slotName] + " --launcher " + LauncherName.Text;

        ProcessManager.StartProcess(Environment.ProcessPath, args, true, false);
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