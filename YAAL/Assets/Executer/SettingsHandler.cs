using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YAAL;
using YAAL.Assets.Script.Cache;
using YAAL.Assets.Scripts;
using static YAAL.AsyncSettings;
using static YAAL.LauncherSettings;
using static YAAL.PreviousAsyncSettings;
using static YAAL.SlotSettings;

public class SettingsHandler
{
    public UnifiedSettings settings = new UnifiedSettings();
    private Executer _baseLauncher = null;
    private Executer executer;
    private ProcessHandler processHandler;
    private InstructionHandler instructionHandler;
    private Parser parser;

    public SettingsHandler (Executer newExecuter)
    {
        executer = newExecuter;
    }
    

    public void SetHandlers()
    {
        processHandler = executer.ProcessHandler;
        instructionHandler = executer.InstructionHandler;
        parser = executer.Parser;
    }

    public void ReadSlot(string async, string slot)
    {
        foreach (var item in IOManager.GetSettings(async, slot).settings)
        {
            settings[item.Key] = item.Value;
        }
    }

    public void ReadCache(Cache_CustomLauncher cache)
    {
        foreach (var item in IOManager.GetGeneralSettings().settings)
        {
            settings[item.Key] = item.Value;
        }

        foreach (var item in cache.settings)
        {
            settings[item.Key] = item.Value;
        }

        foreach (var item in cache.customSettings)
        {
            settings[item.Key] = item.Value;
        }
    }

    public Cache_CustomLauncher WriteCache()
    {
        Cache_CustomLauncher output = new Cache_CustomLauncher();
        output.settings = settings.GetLauncherSettings()!;
        output.customSettings = settings.GetCustomSettings();

        return output;
    }

    public void AddApworld(string target)
    {
        if (target.Contains(";"))
        {
            foreach (var value in IOManager.SplitPathList(target))
            {
                this.settings[LauncherSettings.apworld] += "\"" + value + "\";";
            }
        }
        else
        {
            this.settings[LauncherSettings.apworld] += "\"" + target + "\";";
        }

        settings.Set(requiresVersion, true);
    }

    public List<string> GetApworlds()
    {
        List<string> temp = new List<string>();
        bool hasAddedOwnApworld = false;
        foreach (var item in instructionHandler.listOfInstructions)
        {
            if(item.Key is Apworld apworld)
            {
                temp.Add(apworld.GetTarget());
            }

            if(!hasAddedOwnApworld && item.Key is Patch patch)
            {
                temp.Add("YAAL.apworld");
                hasAddedOwnApworld = true;
            }
        }

        if (_baseLauncher != null 
            && _baseLauncher.SettingsHandler.settings[LauncherSettings.launcherName] != settings[LauncherSettings.launcherName])
        {
            foreach (var item in _baseLauncher.SettingsHandler.GetApworlds())
            {
                if (!temp.Contains(item))
                {
                    temp.Add(item);
                }
            }
        }
        List<string> output = new List<string>();
        foreach (var item in temp)
        {
            foreach (var trueItem in parser.SplitString(item))
            {
                string cleaned = trueItem.Trim();
                if (cleaned != "" && cleaned != "${base:apworld}")
                {
                    output.Add(parser.ParseTextWithSettings(cleaned));
                }
            }
        }

        return output;
    }

    public Executer? GetBaseLauncher()
    {
        if(_baseLauncher != null)
        {
            return _baseLauncher;
        }

        string name = settings[SlotSettings.baseLauncher]!;
        if (name == null || name == "")
        {
            ErrorManager.AddNewError(
                "CustomLauncher - Tried to access empty launcher",
                "A customLauncher tried to access its baseLauncher (the game the tool is for), but this launcher doesn't have a baselauncher. Please report this issue."
                );
            processHandler.hasErroredOut = true;
            return null;
        }
        Executer output = new Executer();
        output.Load(settings[asyncName]!, settings[slotLabel]!, settings[baseLauncher]!);
        _baseLauncher = output;
        return output;
    }

    public void IgnoreBaseLauncher()
    {
        _baseLauncher = executer;
    }

    public void ForcePatchRequirement()
    {
        settings.Set(requiresVersion, true);
        settings.Set(requiresPatch, true);
    }

    public void SetSetting(Enum key, string value)
    {
        settings[key] = value;
    }

    public void SetSetting(string key, string value)
    {
        settings[key] = value;
    }

    public string GetSetting(Enum key)
    {
        return settings[key] ?? "";
    }

    public string GetSetting(string key)
    {
        return settings[key] ?? "";
    }
}