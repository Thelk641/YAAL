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
    public Executer executer;
    public string instructionType;
    public abstract bool Execute();

    public Instruction()
    {
        foreach (TEnum item in Enum.GetValues(typeof(TEnum)))
        {
            InstructionSetting[item] = "";
        }
    }

    public virtual void SetSetting(TEnum item, string value)
    {
        InstructionSetting[item] = value;
    }

    public void SetSettings(Dictionary<Enum,string> newSettings)
    {
        foreach (var item in newSettings)
        {
            SetSetting((TEnum)item.Key, item.Value);
        }
    }

    public virtual void SetSetting(string key, string value) {
        if (Enum.TryParse<TEnum>(key, out var enumKey))
        {
            InstructionSetting[enumKey] = value;
        }
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
        {
            return InstructionSetting[enumKey];
        }
            
        return "";
    }

    public Dictionary<Enum, string> GetSettings()
    {
        Dictionary<Enum, string> output = new Dictionary<Enum, string>();
        foreach (var item in InstructionSetting)
        {
            output[item.Key] = item.Value;
        }
        return output;
    }

    public string GetInstructionType()
    {
        return instructionType;
    }

    public void SetExecuter(Executer newExecuter)
    {
        executer = newExecuter;
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
