using YAAL;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
public abstract class Instruction<TEnum> : Interface_Instruction where TEnum : struct, Enum
{
    public Dictionary<TEnum, string> InstructionSetting = new Dictionary<TEnum, string>();
    public UnifiedSettings settings;
    public CustomLauncher customLauncher;
    public string instructionType;
    public abstract bool Execute();

    public Instruction()
    {
        foreach (TEnum item in Enum.GetValues(typeof(TEnum)))
        {
            InstructionSetting[item] = "";
        }
    }

    public void SetSetting(string key, string value) {
        if (Enum.TryParse<TEnum>(key, out var enumKey))
            InstructionSetting[enumKey] = value;
    }

    public void SetSettings(Dictionary<string, string> settings)
    {
        foreach (var item in settings)
        {
            SetSetting(item.Key, item.Value);
        }
    }

    public void SetExecuteSettings(UnifiedSettings newSettings)
    {
        settings = newSettings;
    }

    public string GetSetting(string key)
    {
        if (Enum.TryParse<TEnum>(key, out var enumKey))
            return InstructionSetting[enumKey];

        return "";
    }

    public Dictionary<string, string> GetSettings()
    {
        Dictionary<string, string> output = new Dictionary<string, string>();
        foreach (var item in InstructionSetting)
        {
            output[item.Key.ToString()] = item.Value;
        }
        return output;
    }

    public string GetInstructionType()
    {
        return instructionType;
    }

    public void SetCustomLauncher(CustomLauncher newCustomLauncher)
    {
        customLauncher = newCustomLauncher;
    }

    public CustomLauncher GetCustomLauncher()
    {
        return customLauncher;
    }

    public virtual void ParseProcess(object? sender, EventArgs e)
    {
        return;
    }

    public virtual void ParseOutputData(object sender, DataReceivedEventArgs e)
    {
        return;
    }
}
