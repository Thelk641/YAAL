using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace YAAL;

public partial class SettingManager : Window
{
    bool hasAddedAFixed = false;
    bool hasAddedACustom = false;

    private Dictionary<string, string> fixedSettings = new Dictionary<string, string>();
    private List<string> defaultSettings = new List<string>();

    private Dictionary<string, string> internalSettings = new Dictionary<string, string>();
    private List<Setting> settings = new List<Setting>();

    private List<string> hiddenSettings = new List<string>();

    private static SettingManager _general;
    private static SettingManager _clmaker;

    public SettingManager()
    {
        InitializeComponent();
        addSetting.Click += AddSetting;
        fixedSettings = Templates.fixedSettings;
        defaultSettings = Templates.defaultSettings;
        hiddenSettings = Templates.hiddenSettings;
    }

    public SettingManager(Dictionary<LauncherSettings,string> toParse)
    {
        InitializeComponent();
        addSetting.Click += AddSetting;
        fixedSettings = Templates.fixedSettings;
        defaultSettings = Templates.defaultSettings;
        hiddenSettings = Templates.hiddenSettings;
        ReadSetting(toParse);
    }

    public static SettingManager GetSettingsWindow(string origin, Dictionary<LauncherSettings, string> toParse)
    {
        if(origin == "clmaker")
        {
            if(_clmaker == null)
            {
                return new SettingManager(toParse);
            }
            else
            {
                _clmaker.Activate();
                _clmaker.Topmost = true;
                _clmaker.Topmost = false;
                _clmaker.Closing += (object? sender, WindowClosingEventArgs e) => { _clmaker = null; };
                return _clmaker;
            }
        } else
        {
            if (_general == null)
            {
                return new SettingManager(toParse);
            }
            else
            {
                _general.Activate();
                _general.Topmost = true;
                _general.Topmost = false;
                _general.Closing += (object? sender, WindowClosingEventArgs e) => { _general = null; };
                return _general;
            }
        }
    }

    private void AddSetting(object? sender, RoutedEventArgs e)
    {
        Setting toAdd = AddCustomSetting();
        settings.Add(toAdd);
    }

    public Dictionary<LauncherSettings, string> ParseSetting(out Dictionary<string, string> customOutput)
    {
        customOutput = new Dictionary<string, string>();
        Dictionary<LauncherSettings, string> output = new Dictionary<LauncherSettings, string>();
        List<Setting> defaultSettings = new List<Setting>();
        List<Setting> customSettings = new List<Setting>();
        foreach (var item in settings)
        {
            Setting_Custom customCast = item as Setting_Custom;
            if(customCast != null)
            {
                // custom settings overwrite everything else
                if(customCast.SettingValueText != null && customCast.SettingValueText != "")
                {
                    customSettings.Add(item);
                }
            } else
            {
                Setting_Fixed defaultCast = item as Setting_Fixed;
                if(defaultCast != null)
                {
                    defaultSettings.Add(item);
                }
                // the last possibility is that it's a default setting
                // Something like "launchername" => "Per launcher"
                // We want to ignore those completely
            }  
        }

        foreach (var item in internalSettings)
        {
            if (Enum.TryParse(item.Key, out LauncherSettings setting))
            {
                output[setting] = item.Value;
            }
            else
            {
                ErrorManager.ThrowError(
                    "SettingManager - Invalid internalSetting key",
                    "Tried to set a value for key " + item.Key + " but it isn't a valid LauncherSetting. Please report this issue."
                    );
            }
        }

        foreach (var item in defaultSettings)
        {
            if (Enum.TryParse(item.SettingNameText, out LauncherSettings setting))
            {
                output[setting] = item.SettingValueText;
            }
            else
            {
                ErrorManager.ThrowError(
                    "SettingManager - Invalid defaultSetting key",
                    "Tried to set a value for key " + item.SettingNameText + " but it isn't a valid LauncherSetting. Please report this issue."
                    );
            }
        }

        foreach (var item in customSettings)
        {
            customOutput[item.SettingNameText] = item.SettingValueText;
        }
        return output;
    }

    private void ReadSetting(Dictionary<LauncherSettings, string> toParse)
    {
        foreach (var item in toParse)
        {
            Setting toAdd;
            string settingValue;

            if (hiddenSettings.Contains(item.Key.ToString()))
            {
                // This is a hidden setting (like apworld or Debug_*)
                // They're completely hidden
                internalSettings.Add(item.Key.ToString(), item.Value);
                continue;
            } else if (fixedSettings.ContainsKey(item.Key.ToString()))
            {
                // This is an internal setting, the user can't change it
                // It's only displayed as an easy way to find it without having to
                // go to the docs (ex : slotInfo)
                // Or it's modified elsewhere (ex : launcherName)
                internalSettings.Add(item.Key.ToString(), item.Value);
                continue;

            }
            else if (defaultSettings.Contains(item.Key.ToString()))
            {
                // This is a default setting, the user can change its value
                // but not its name, as it's used in the code
                // ex: aplauncher
                if (!hasAddedAFixed)
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = "-- Fixed settings";
                    FixedSettingContainer.Children.Add(textBlock);
                    hasAddedAFixed = true;
                    textBlock.FontSize = 17;
                }
                toAdd = new Setting_Fixed();
                FixedSettingContainer.Children.Add(toAdd);
                settingValue = item.Value;
            }
            else
            {
                // This is a custom setting the user added
                // They're free to change its name and its value
                toAdd = AddCustomSetting();
                settingValue = item.Value;
            }
            toAdd.SettingNameText = item.Key.ToString();
            toAdd.SetSetting(item.Key.ToString(), settingValue);
            toAdd.manager = this;
            settings.Add(toAdd);
            toAdd.SetBinary();
        }
        AddAllFixedSettings();
    }

    public Setting_Custom AddCustomSetting()
    {
        if (!hasAddedACustom)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = "-- Custom settings";
            CustomSettingContainer.Children.Add(textBlock);
            hasAddedACustom = true;
            textBlock.FontSize = 17;
        }
        Setting_Custom toAdd = new Setting_Custom();
        CustomSettingContainer.Children.Add(toAdd);
        return toAdd;
    }

    public void AddAllFixedSettings()
    {
        TextBlock textBlock = new TextBlock();
        textBlock.Text = "-- Internal settings";
        InternalSettingContainer.Children.Add(textBlock);
        textBlock.FontSize = 17;

        foreach (var item in fixedSettings)
        {
            Setting_Default toAdd = new Setting_Default();
            toAdd.SetSetting(item.Key, item.Value);
            InternalSettingContainer.Children.Add(toAdd);
            toAdd.manager = this;
        }
    }

    public void RemoveSetting(Setting toRemove) 
    {
        settings.Remove(toRemove);
    }


}