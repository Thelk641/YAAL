using YAAL;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;

public interface Interface_Instruction
{
    public bool Execute();
    public void SetSetting(string key, string value);
    public void SetSettings(Dictionary<string, string> settings);
    public void SetExecuteSettings(UnifiedSettings settings);
    public string GetSetting(string key);
    public Dictionary<string, string> GetSettings();
    public string GetInstructionType();
    public CustomLauncher GetCustomLauncher();
    public void SetCustomLauncher(CustomLauncher customLauncher);
    public void ParseProcess(object? sender, EventArgs e);
    public void ParseOutputData(object sender, DataReceivedEventArgs e);
}



/*public interface Interface_Instruction
{
    public Instruction<Enum> instruction;
    public UnifiedSettings settings;
    public CustomLauncher customLauncher;
    public string instructionType;

    public Interface_Instruction()
    {

    }

    public Interface_Instruction(string newType)
    {
        instructionType = newType;
        Create(instructionType);
    }
    public void Create(string type)
    {
        switch (type)
        {
            case "Apworld":
                instruction = new Apworld();
                (instruction as Apworld).SetDefaultSettings();
                break;
            case "Backup":
                instruction = new Backup();
                (instruction as Backup).SetDefaultSettings();
                break;
            case "Open":
                instruction = new Open();
                (instruction as Open).SetDefaultSettings();
                break;
            case "Patch":
                instruction = new Patch();
                (instruction as Patch).SetDefaultSettings();
                break;
            case "RegEx":
                instruction = new RegEx();
                (instruction as RegEx).SetDefaultSettings();
                break;
            default:
                ErrorManager.ThrowError(
                    "InterfaceInstruction - Invalid type",
                    "Tried to generate an instruction of type " + type + " which doesn't exists. Please report this issue."
                    );
                break;
        }
    }
}
*/