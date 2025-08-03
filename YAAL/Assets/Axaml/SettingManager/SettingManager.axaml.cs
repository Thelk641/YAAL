using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static YAAL.GeneralSettings;

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
    private static SettingManager _tools;

    public Action? OnClosing;

    public SettingManager()
    {
        InitializeComponent();
        addSetting.Click += AddSetting;
        fixedSettings = Templates.fixedSettings;
        defaultSettings = Templates.defaultSettings;
        hiddenSettings = Templates.hiddenSettings;
    }

    public static SettingManager GetSettingsWindow(Dictionary<LauncherSettings, string> launcherSettings, Dictionary<string, string> customSettings)
    {
        if (_clmaker == null)
        {
            _clmaker = new SettingManager();
            _clmaker.Closing += (_, _) => { _clmaker = null; };
        }
        else
        {
            _clmaker.FixedSettingContainer.Children.Clear();
            _clmaker.CustomSettingContainer.Children.Clear();
            _clmaker.InternalSettingContainer.Children.Clear();
            
            _clmaker.Activate();
            _clmaker.Topmost = true;
            _clmaker.Topmost = false;
            _clmaker.Closing += (_, _) => { _clmaker = null; };
            
        }

        _clmaker.hasAddedACustom = false;
        _clmaker.hasAddedAFixed = false;
        _clmaker.settings = new List<Setting>();
        _clmaker.ReadSetting(launcherSettings, customSettings);
        return _clmaker;
    }

    public static SettingManager GetSettingsWindow(Dictionary<GeneralSettings, string> generalSettings, Dictionary<string, string> customSettings)
    {
        if (_general == null)
        {
            _general = new SettingManager();
            _general.Closing += (_, _) => { _general = null; };
        }
        else
        {
            _general.CustomSettingContainer.Children.Clear();
            _general.GeneralSettingContainer.Children.Clear();
            _general.Activate();
            _general.Topmost = true;
            _general.Topmost = false;
        }

        _general.hasAddedACustom = false;
        _general.hasAddedAFixed = false;
        _general.settings = new List<Setting>();
        _general.ReadSetting(generalSettings, customSettings);
        return _general;
    }


    public static SettingManager GetSettingsWindow(Dictionary<string, string> toParse)
    {
        if(_tools == null)
        {
            _tools = new SettingManager();
            _tools.Closing += (_, _) => { _general = null; };
            _tools.addSetting.IsVisible = false;
            _tools.Closing += (_, _) =>
             {
                 _tools.OnClosing?.Invoke();
                 _tools = null;
             };
        } else
        {
            _tools.OnClosing?.Invoke();
            _tools.ToolSettingContainer.Children.Clear();
        }

        _tools.ReadSetting(toParse);
        return _tools;
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

    public Dictionary<GeneralSettings, string> ParseSettings(out Dictionary<string, string> customSettings)
    {
        Dictionary<GeneralSettings, string> output = new Dictionary<GeneralSettings, string>();
        customSettings = new Dictionary<string, string>();

        foreach (var item in GeneralSettingContainer.Children)
        {
            if(item is Setting setting)
            {
                if(Enum.TryParse(setting.SettingNameText, out GeneralSettings generalSetting))
                {
                    output[generalSetting] = setting.SettingValueText;
                }
            }
        }

        try
        {
            output[apfolder] = Path.GetDirectoryName(output[aplauncher]).Trim();
            output[lua_adventure] = "--lua=\"" + Path.Combine(output[apfolder], "data", "lua", "connector_adventure.lua") + "\"";
            output[lua_bizhawk] = "--lua=\"" + Path.Combine(output[apfolder], "data", "lua", "connector_bizhawk_generic.lua") + "\"";
            output[lua_ff1] = "--lua=\"" + Path.Combine(output[apfolder], "data", "lua", "connector_ff1.lua") + "\"";
            output[lua_ladx] = "--lua=\"" + Path.Combine(output[apfolder], "data", "lua", "connector_ladx_bizhawk.lua") + "\"";
            output[lua_mmbn3] = "--lua=\"" + Path.Combine(output[apfolder], "data", "lua", "connector_mmbn3.lua") + "\"";
            output[lua_oot] = "--lua=\"" + Path.Combine(output[apfolder], "data", "lua", "connector_oot.lua") + "\"";
            output[lua_tolz] = "--lua=\"" + Path.Combine(output[apfolder], "data", "lua", "connector_tloz.lua") + "\"";
        }
        catch { }

        foreach (var item in CustomSettingContainer.Children)
        {
            if(item is Setting setting)
            {
                customSettings[setting.SettingNameText] = setting.SettingValueText;
            }
        }

        return output;
    }

    public Dictionary<string, string> ParseSetting()
    {
        Dictionary<string, string> output = new Dictionary<string, string>();

        foreach (var item in ToolSettingContainer.Children)
        {
            Setting_Tool setting = item as Setting_Tool;
            output[setting.SettingName.Text] = setting.Versions.SelectedItem.ToString(); 
        }

        return output;
    }

    private void ReadSetting(Dictionary<string, string> toParse)
    {
        List<string> Tools = IOManager.GetToolList();
        foreach (var tool in Tools) {
            Setting_Tool toAdd = new Setting_Tool();
            ToolSettingContainer.Children.Add(toAdd);
            toAdd.SettingName.Text = tool;
            List<string> versions = IOManager.GetDownloadedVersions(tool);
            toAdd.Versions.ItemsSource = versions;
            if (toParse.ContainsKey(tool) && versions.Contains(toParse[tool])){
                toAdd.Versions.SelectedItem = toParse[tool];
            } else
            {
                toAdd.Versions.SelectedIndex = 0;
            }
        }
    }

    private void ReadSetting(Dictionary<LauncherSettings, string> launcherSettings, Dictionary<string, string> customSettings)
    {
        foreach (var item in launcherSettings)
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
            toAdd.SetSetting(item.Key.ToString(), settingValue);
            toAdd.manager = this;
            settings.Add(toAdd);
            toAdd.SetBinary();
        }
        AddAllFixedSettings();

        foreach (var item in customSettings)
        {
            Setting toAdd = AddCustomSetting();
            toAdd.SetSetting(item.Key, item.Value);
            settings.Add(toAdd);
            toAdd.SetBinary();
        }
    }

    private void ReadSetting(Dictionary<GeneralSettings, string> GeneralSettings, Dictionary<string, string> customSettings)
    {
        if(GeneralSettings.Count > 0)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = "-- Fixed settings";
            GeneralSettingContainer.Children.Add(textBlock);
            textBlock.FontSize = 17;
        }

        foreach (var item in GeneralSettings)
        {
            Setting toAdd = new Setting_Fixed();
            toAdd.SetSetting(item.Key.ToString(), item.Value);
            settings.Add(toAdd);
            toAdd.SetBinary();
            GeneralSettingContainer.Children.Add(toAdd);
        }

        foreach (var item in customSettings)
        {
            Setting toAdd = AddCustomSetting();
            toAdd.SetSetting(item.Key, item.Value);
            toAdd.SetBinary();
        }
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