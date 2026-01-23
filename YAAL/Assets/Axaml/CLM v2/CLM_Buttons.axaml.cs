using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using YAAL.Assets.Scripts;
using static YAAL.ApworldSettings;

namespace YAAL;

public partial class CLM_Buttons : UserControl
{
    public Dictionary<Button, KeyValuePair<Avalonia.Svg.Skia.Svg, TextBlock>> buttonsList = new Dictionary<Button, KeyValuePair<Avalonia.Svg.Skia.Svg, TextBlock>>();
    private CLM_Selector selector;
    private CLM clm;
    private bool altMode = false;

    public CLM_Buttons()
    {
        InitializeComponent();
        Dispatcher.UIThread.Post(() => { ListButtons(); });
        AddButtonEvents();
    }

    public CLM_Buttons(CLM newCLM, CLM_Selector newSelector) : this()
    {
        clm = newCLM;
        selector = newSelector;
    }

    public void AddButtonEvents()
    {
        EmptyLauncher.Click += (_, _) =>
        {
            if (altMode)
            {
                SwitchMode(false);
            }

            NewLauncher launcherCreator = new NewLauncher();

            launcherCreator.Closing += (_, _) =>
            {
                if (!launcherCreator.create)
                {
                    return;
                }

                Cache_CustomLauncher cache = launcherCreator.launcher;
                IOManager.SaveCacheLauncher(cache);
                selector!.ReloadList(cache.settings[LauncherSettings.launcherName]);
            };
        };

        SaveLauncher.Click += (_, _) =>
        {
            clm.SaveLauncher();
        };

        Duplicate.Click += (_, _) =>
        {
            clm.SaveLauncher();
            Cache_DisplayLauncher cache = selector.GetCache();
            string duplicateName = IOManager.FindAvailableLauncherName(cache.name);
            cache.cache.settings[LauncherSettings.launcherName] = duplicateName;
            IOManager.SaveCacheLauncher(cache.cache);
            selector.ReloadList(duplicateName);
        };

        Delete.Click += (_, _) =>
        {
            if (altMode)
            {
                SwitchMode(false);
            }
            Cache_DisplayLauncher cache = selector.GetCache();
            if(cache.name != "" && WindowManager.OpenWindow(WindowType.ConfirmationWindow, clm) is ConfirmationWindow confirm)
            {
                confirm.Setup(cache.name);
                confirm.Closed += (_,_) =>
                {
                    if (confirm.confirmed)
                    {
                        IOManager.DeleteLauncher(cache.name);
                        selector.ReloadList();
                        selector.SelectFirst();
                    }
                };
            }
        };

        Settings.Click += (_, _) =>
        {
            if (altMode)
            {
                SwitchMode(false);
            }
            Cache_DisplayLauncher selectedLauncher = selector.GetCache();
            SettingManager settingManager = SettingManager.GetSettingsWindow(clm, selectedLauncher.cache.settings, selectedLauncher.cache.customSettings);
            settingManager.Show();

            settingManager.Closing += (_, _) =>
            {
                Dictionary<string, string> newCustomSettings;
                Dictionary<LauncherSettings, string> newSettings = settingManager.OutputLauncherSettings(out newCustomSettings);

                selectedLauncher.cache.settings = newSettings;
                selectedLauncher.cache.customSettings = newCustomSettings;

                if(selector.GetCache().name == selectedLauncher.name)
                {
                    clm.SaveLauncher();
                } else
                {
                    // we openned the screen, then selected another launcher, we just need to update this
                    IOManager.SaveCacheLauncher(selectedLauncher.cache);
                }
            };
        };

        Rename.Click += (_, _) =>
        {
            selector.SwitchMode();

            if (altMode)
            {
                SwitchMode(false);
                Trace.WriteLine("Tried to set focus");
                selector.SetFocus();
            }
        };
    }

    public void ListButtons()
    {
        List<Button> list = new List<Button>();
        list.Add(EmptyLauncher);
        list.Add(SaveLauncher);
        list.Add(Duplicate);
        list.Add(Delete);
        list.Add(Settings);
        list.Add(Rename);

        foreach (Button button in list)
        {
            var icon = button.FindDescendantOfType<Avalonia.Svg.Skia.Svg>();
            var text = button.FindDescendantOfType<TextBlock>();

            if (icon is Avalonia.Svg.Skia.Svg svg && text is TextBlock label)
            {
                buttonsList.Add(button, new KeyValuePair<Avalonia.Svg.Skia.Svg, TextBlock>(svg, label));
            }
        }
    }

    public void SwitchMode(bool altPressed)
    {
        if(altMode == altPressed)
        {
            return;
        }

        altMode = altPressed;
        foreach (var item in buttonsList)
        {
            item.Value.Key.IsVisible = !altPressed;
            item.Value.Value.IsVisible = altPressed;
        }
    }

    public void ProcessKey(Key key)
    {
        foreach (var item in buttonsList)
        {
            if(item.Value.Value.Text == key.ToString().ToUpper())
            {
                item.Key.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                return;
            }
        }
    }
}