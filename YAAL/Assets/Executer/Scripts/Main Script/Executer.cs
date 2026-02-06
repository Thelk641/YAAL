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

public class Executer
{
    public SettingsHandler SettingsHandler;
    public ProcessHandler ProcessHandler;
    public InstructionHandler InstructionHandler;
    public Parser Parser;

    public bool waitingForRestore = false;
    private List<Interface_Instruction> instructionWaiting = new List<Interface_Instruction>();
    public event Action? DoneRestoring;

    public Executer()
    {
        SettingsHandler = new SettingsHandler(this);
        InstructionHandler = new InstructionHandler(this, SettingsHandler);
        ProcessHandler = new ProcessHandler(this, InstructionHandler, SettingsHandler);
        Parser = new Parser(SettingsHandler, ProcessHandler);
        SettingsHandler.SetHandlers();
    }

    public void Launch(string async, string slot, string launcherName)
    {
        Load(async, slot, launcherName);
        ProcessHandler.Execute();
    }

    public void Load(string async, string slot, string launcherName)
    {
        LoadCache(LauncherManager.LoadLauncher(launcherName));
        SettingsHandler.ReadSlot(async, slot);
    }

    public void LoadCache(Cache_CustomLauncher toLoad)
    {
        SettingsHandler.ReadCache(toLoad);
        InstructionHandler.ReadCache(toLoad.instructionList);
    }

    public void SaveCache()
    {
        Cache_CustomLauncher toSave = SettingsHandler.WriteCache();
        toSave.instructionList = InstructionHandler.WriteCache();
        LauncherManager.SaveLauncher(toSave);
    }

    public void AddWait(Interface_Instruction instruction)
    {
        if (instructionWaiting.Contains(instruction))
        {
            return;
        }
        instructionWaiting.Add(instruction);
        waitingForRestore = true;
    }

    public void RemoveWait(Interface_Instruction instruction)
    {
        instructionWaiting.Remove(instruction);
        if (instructionWaiting.Count == 0)
        {
            waitingForRestore = false;
            DoneRestoring?.Invoke();
        }
    }
}