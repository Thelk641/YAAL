using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using YAAL.Assets.Scripts;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static YAAL.GeneralSettings;
using static YAAL.LauncherSettings;

namespace YAAL;

public partial class SettingManager : ScalableWindow
{
    private static SettingManager _general;
    private static SettingManager _clmaker;
    private static SettingManager _tools;

    private Dictionary<HiddenSettings, string> hidden = new Dictionary<HiddenSettings, string>();
    private List<HardcodedSettings> alreadyThere = new List<HardcodedSettings>();

    public Action? OnClosing;

    public SettingManager()
    {
        InitializeComponent();
        DataContext = App.Settings;
        AutoTheme.SetTheme(TrueBackground, ThemeSettings.backgroundColor);

        addSetting.Click += (_, _) =>
        {
            Setting newSetting = new Setting("", "", true);
            CustomSettingContainer.Children.Add(newSetting);

            if (!CustomSettings.IsVisible)
            {
                CustomSettings.IsVisible = true;
            }
        };
    }

    public static SettingManager GetSettingsWindow(Window window, Dictionary<GeneralSettings, string> generalSettings, Dictionary<string, string> customSettings)
    {
        if(_general == null)
        {
            _general = new SettingManager();
            _general.Closing += (_,_) =>
            {
                _general = null;
                window.Activate();
                window.Topmost = true;
                window.Topmost = false;
            };
        } else
        {
            _general.OnClosing?.Invoke();
            _general.Activate();
            _general.Topmost = true;
            _general.Topmost = false;
            _general.Clean();
        }
        _general.ParseSettings(generalSettings);
        _general.ParseSettings(customSettings);
        return _general; 
    }

    public static SettingManager GetSettingsWindow(Window window, Dictionary<LauncherSettings, string> launcherSettings, Dictionary<string, string> customSettings)
    {
        if (_clmaker == null)
        {
            _clmaker = new SettingManager();
            _clmaker.Closing += (_, _) =>
            {
                _clmaker = null;
                window.Activate();
                window.Topmost = true;
                window.Topmost = false;
            };
        }
        else
        {
            _clmaker.OnClosing?.Invoke();
            _clmaker.Activate();
            _clmaker.Topmost = true;
            _clmaker.Topmost = false;
            _clmaker.Clean();
        }
        _clmaker.ParseSettings(launcherSettings);
        _clmaker.ParseSettings(customSettings);
        _clmaker.CustomSettings.IsVisible = true;
        return _clmaker;
    }

    public static SettingManager GetSettingsWindow(Window window, Dictionary<string, string> customSettings)
    {
        if (_tools == null)
        {
            _tools = new SettingManager();
            _tools.CustomSettingsTag.Text = "Tool version settings";
            _tools.Closing += (_, _) =>
            {
                _tools = null;
                window.Activate();
                window.Topmost = true;
                window.Topmost = false;
            };
        }
        else
        {
            _tools.OnClosing?.Invoke();
            _tools.Activate();
            _tools.Topmost = true;
            _tools.Topmost = false;
            _tools.Clean();
        }
        _tools.ParseSettings(customSettings);
        return _tools;
    }

    public void Clean()
    {
        GeneralSettingContainer.Children.Clear();
        LauncherSettingContainer.Children.Clear();
        CustomSettingContainer.Children.Clear();
        OtherSettingContainer.Children.Clear();
        hidden = new Dictionary<HiddenSettings, string>();
        alreadyThere = new List<HardcodedSettings>();
    }

    public void ShowAllCategories()
    {
        GeneralSettings.IsVisible = true;
        LauncherSettings.IsVisible = true;
        CustomSettings.IsVisible = true;
        OtherSettings.IsVisible = true;
    }

    public void HideEmptyCategories()
    {
        if(GeneralSettingContainer.Children.Count == 0)
        {
            GeneralSettings.IsVisible = false;
        }
        if (LauncherSettingContainer.Children.Count == 0)
        {
            LauncherSettings.IsVisible = false;
        }
        if (OtherSettingContainer.Children.Count == 0)
        {
            OtherSettings.IsVisible = false;
        }
    }

    public void ParseSettings(Dictionary<string, string> toParse, bool canBeEdited = true)
    {
        Setting newSetting;
        ShowAllCategories();

        foreach (var item in toParse)
        {
            if(IsHardcoded(item.Key, item.Value) || IsHidden(item.Key, item.Value))
            {
                continue;
            }

            if(Enum.TryParse<GeneralSettings>(item.Key, out GeneralSettings generalSetting))
            {
                newSetting = new Setting(item.Key, item.Value, canBeEdited);
                GeneralSettingContainer.Children.Add(newSetting);
                newSetting.RequestRemoval += () => { GeneralSettingContainer.Children.Remove(newSetting); };
                continue;
            }

            if (Enum.TryParse<LauncherSettings>(item.Key, out LauncherSettings launcherSetting))
            {
                newSetting = new Setting(item.Key, item.Value, canBeEdited);
                LauncherSettingContainer.Children.Add(newSetting);
                newSetting.RequestRemoval += () => { LauncherSettingContainer.Children.Remove(newSetting); };
                continue;
            }

            newSetting = new Setting(item.Key, item.Value);
            CustomSettingContainer.Children.Add(newSetting);
            newSetting.RequestRemoval += () => { CustomSettingContainer.Children.Remove(newSetting); };
        }
        SortOtherSettings();
        HideEmptyCategories();
    }

    public void ParseSettings(Dictionary<GeneralSettings, string> toParse)
    {
        ShowAllCategories();
        foreach (var item in toParse)
        {
            if (IsHardcoded(item.Key.ToString(), item.Value) || IsHidden(item.Key.ToString(), item.Value))
            {
                continue;
            }

            Setting newSetting = new Setting(item.Key.ToString(), item.Value, false);
            GeneralSettingContainer.Children.Add(newSetting);

            if (item.Key == zoom)
            {
                newSetting.IsZoom();
            }
        }
        SortOtherSettings();
        HideEmptyCategories();
    }

    public void ParseSettings(Dictionary<LauncherSettings, string> toParse)
    {
        ShowAllCategories();
        foreach (var item in toParse)
        {
            if (IsHardcoded(item.Key.ToString(), item.Value) || IsHidden(item.Key.ToString(), item.Value))
            {
                continue;
            }

            Setting newSetting = new Setting(item.Key.ToString(), item.Value, false);
            LauncherSettingContainer.Children.Add(newSetting);
        }

        foreach (var item in Templates.hardcodedSettings)
        {
            if (!alreadyThere.Contains(item.Key))
            {
                Setting newSetting = new Setting(item.Key.ToString(), "", item.Value);
                OtherSettingContainer.Children.Add(newSetting);
            }
        }
        SortOtherSettings();
        HideEmptyCategories();
    }

    private bool IsHardcoded(string input, string value)
    {
        if(Enum.TryParse<HardcodedSettings>(input, out HardcodedSettings setting))
        {
            Setting newSetting = new Setting(input, value, Templates.hardcodedSettings[setting]);
            OtherSettingContainer.Children.Add(newSetting);
            return true;
        }

        return false;
    }

    private bool IsHidden(string input, string value)
    {
        if(Enum.TryParse<HiddenSettings>(input, out HiddenSettings setting)){
            hidden.Add(setting, value);
            return true;
        }
        return false;
    }

    private void SortOtherSettings()
    {
        List<string> list = new List<string>();
        List<Setting> duplicates = new List<Setting>();
        foreach (var item in OtherSettingContainer.Children)
        {
            if(item is Setting setting)
            {
                if (list.Contains(setting.SetName.Text))
                {
                    duplicates.Add(setting);
                } else
                {
                    list.Add(setting.SetName.Text);
                }
                
            }
        }
        list.Sort();

        foreach (var item in duplicates)
        {
            OtherSettingContainer.Children.Remove(item);
        }

        List<Setting> orderedList = new List<Setting>();
        for (int i = 0; i < list.Count; i++)
        {
            foreach (var item in OtherSettingContainer.Children)
            {
                if(item is Setting setting && setting.SetName.Text == list[i])
                {
                    orderedList.Add(setting);
                }
                
            }
        }

        for (int i = 0; i < orderedList.Count; i++)
        {
            int index = OtherSettingContainer.Children.IndexOf(orderedList[i]);
            OtherSettingContainer.Children.Move(OtherSettingContainer.Children.IndexOf(orderedList[i]), i);
        }
    }

    public Dictionary<string, string> OutputSettings(string origin)
    {
        Dictionary<string, string> output = new Dictionary<string, string>();

        string name;
        string value;
        foreach (var item in GeneralSettingContainer.Children)
        {
            if(item is Setting setting)
            {
                name = setting.GetValue(out value);
                if (value != "" || origin == "General")
                {
                    output[name] = ParseBinarySetting(value);
                }
            }
        }

        foreach (var item in LauncherSettingContainer.Children)
        {
            if (item is Setting setting)
            {
                name = setting.GetValue(out value);
                if (value != "" || origin == "CLMaker")
                {
                    output[name] = ParseBinarySetting(value);
                }
            }
        }

        foreach (var item in CustomSettingContainer.Children)
        {
            if (item is Setting setting)
            {
                name = setting.GetValue(out value);
                output[name] = ParseBinarySetting(value);
            }
        }

        foreach (var item in OtherSettingContainer.Children)
        {
            if (item is Setting setting)
            {
                name = setting.GetValue(out value);
                output[name] = ParseBinarySetting(value);
            }
        }

        foreach (var item in hidden)
        {
            output[item.Key.ToString()] = ParseBinarySetting(item.Value);
        }

        return output;
    }

    public Dictionary<GeneralSettings, string> OutputGeneralSettings(out Dictionary<string, string> customSettings)
    {
        Dictionary<string, string> toParse = OutputSettings("General");
        Dictionary<GeneralSettings, string> output = new Dictionary<GeneralSettings, string>();
        customSettings = new Dictionary<string, string>();
        foreach (var item in toParse)
        {
            if(Enum.TryParse<GeneralSettings>(item.Key, out GeneralSettings setting))
            {
                output[setting] = item.Value;
            } else
            {
                customSettings[item.Key] = item.Value;
            }
        }

        return output;
    }

    public Dictionary<LauncherSettings, string> OutputLauncherSettings(out Dictionary<string, string> customSettings)
    {
        Dictionary<string, string> toParse = OutputSettings("CLMaker");
        Dictionary<LauncherSettings, string> output = new Dictionary<LauncherSettings, string>();
        customSettings = new Dictionary<string, string>();
        foreach (var item in toParse)
        {
            if (Enum.TryParse<LauncherSettings>(item.Key, out LauncherSettings setting))
            {
                output[setting] = item.Value;
            }
            else
            {
                customSettings[item.Key] = item.Value;
            }
        }

        return output;
    }

    public string ParseBinarySetting(string input)
    {
        if (input == "true")
        {
            return true.ToString();
        }
        else if (input == "false")
        {
            return false.ToString();
        }

        return input;
    }
}