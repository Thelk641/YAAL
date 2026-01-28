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

public class ProcessHandler
{
    InstructionHandler instructionHandler;
    SettingsHandler settingsHandler;
    Executer executer;
    public bool hasErroredOut = false;
    private List<Backup> backups = new List<Backup>();
    private List<Cache_Process> listOfProcess = new List<Cache_Process>();
    private Dictionary<Interface_Instruction, List<string>> instructionAttachedToClosing = new Dictionary<Interface_Instruction, List<string>>();
    private Dictionary<Interface_Instruction, List<string>> instructionAttachedToOutput = new Dictionary<Interface_Instruction, List<string>>();

    public ProcessHandler(Executer newExecuter, InstructionHandler newInstructionHandler, SettingsHandler newSettingsHandler)
    {
        executer = newExecuter;
        instructionHandler = newInstructionHandler;
        settingsHandler = newSettingsHandler;
    }

    public bool Execute()
    {
        hasErroredOut = false;
        if(settingsHandler.GetSetting(LauncherSettings.IsGame) == false.ToString())
        {
            settingsHandler.GetBaseLauncher();
        }

        foreach (var item in instructionHandler.listOfInstructions)
        {
            if(item.Key is Interface_Instruction instruction)
            {
                instruction.SetExecuteSettings(settingsHandler.settings);
                if(!instruction.Execute() || hasErroredOut)
                {
                    ErrorManager.AddNewError(
                        "Executer - Failed to Execute()",
                        "An instruction has returned false, leading to this launcher failing midway. The last instruction tried was of type : " + instruction.GetInstructionType() + ". Please check other errors for the main cause.");
                    ErrorManager.ThrowError();

                    List<Backup> list = new List<Backup>();
                    foreach (var potentialRestore in instructionHandler.listOfInstructions)
                    {
                        if(potentialRestore.Key is Backup backup)
                        {
                            if (!backup.Restore())
                            {
                                ErrorManager.AddNewError(
                                    "CustomLauncher - Failed to Restore()",
                                    "For some reason a restore failed."
                                    );
                                ErrorManager.ThrowError();
                            }
                        } else if (potentialRestore.Key is Isolate isolate)
                        {
                            if (!isolate.Restore())
                            {
                                ErrorManager.AddNewError(
                                    "CustomLauncher - Failed to Restore()",
                                    "For some reason a restore failed."
                                    );
                                ErrorManager.ThrowError();
                            }
                        }
                    }
                    return false;
                }
            }
        }

        Cache_PreviousSlot newSlot = new Cache_PreviousSlot()
        {
            previousAsync = settingsHandler.GetSetting(asyncName),
            previousSlot = settingsHandler.GetSetting(slotLabel),
            previousPatch = settingsHandler.GetSetting(patch),
            previousVersion = settingsHandler.GetSetting(version),
            previousRoom = settingsHandler.GetSetting(roomAddress) + ":" + settingsHandler.GetSetting(roomPort),
            previousPort = settingsHandler.GetSetting(roomPort)
        };

        IOManager.UpdateLastAsync(settingsHandler.GetSetting(gameName), newSlot);
        return true;
    }

    public bool AttachToClosing(Interface_Instruction instruction, string key)
    {
        if (key == "")
        {
            return true;
        }

        try
        {
            instructionAttachedToClosing.Add(instruction, new List<string>());
        }
        catch (Exception e)
        {
            ErrorManager.AddNewError(
                "Executer.ProcessHandler - Tried attaching instruction twice",
                "Trying to attach an instruction triggered the following exception : " + e.Message
                );
            hasErroredOut = true;
            return false;
        }

        string cleaned;

        foreach (var item in executer.Parser.SplitString(key))
        {
            cleaned = item.Trim();
            if (cleaned == "")
            {
                continue;
            }
            foreach (var proc in listOfProcess)
            {
                if (proc.key == cleaned)
                {
                    proc.GetProcess().Exited += instruction.ParseProcess;
                }
            }

            try
            {
                instructionAttachedToClosing[instruction].Add(cleaned);
                executer.AddWait(instruction);
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                "Executer.ProcessHandler - Tried attaching key twice",
                "Trying to attach an instruction triggered the following exception : " + e.Message
                );
                hasErroredOut = true;
                return false;
            }

        }

        return true;
    }

    public void DetachToClosing(Interface_Instruction instruction, string key)
    {
        if (key == "")
        {
            return;
        }

        string[] trueKeys = key.Split(";");
        string cleaned;

        foreach (var item in trueKeys)
        {
            cleaned = item.Trim();
            if (cleaned == "")
            {
                continue;
            }
            foreach (var proc in listOfProcess)
            {
                if (proc.key == cleaned)
                {
                    proc.GetProcess().Exited -= instruction.ParseProcess;
                    break;
                }
            }
        }

        instructionAttachedToClosing.Remove(instruction);
    }

    public bool AttachToOutput(Interface_Instruction instruction, string key)
    {
        if (key == "")
        {
            return true;
        }

        try
        {
            instructionAttachedToOutput.Add(instruction, new List<string>());
        }
        catch (Exception e)
        {
            ErrorManager.AddNewError(
                "Executer.ProcessHandler - Tried attaching instruction twice",
                "Trying to attach an instruction triggered the following exception : " + e.Message
                );
            hasErroredOut = true;
            return false;
        }

        string cleaned;

        foreach (var item in executer.Parser.SplitString(key))
        {
            cleaned = item.Trim();
            if (cleaned == "")
            {
                continue;
            }
            foreach (var proc in listOfProcess)
            {
                if (proc.key == cleaned)
                {
                    proc.GetProcess().OutputDataReceived += instruction.ParseOutputData;
                }
            }

            try
            {
                instructionAttachedToOutput[instruction].Add(cleaned);
                executer.AddWait(instruction);
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                "Executer.ProcessHandler - Tried attaching key twice",
                "Trying to attach an instruction triggered the following exception : " + e.Message
                );
                hasErroredOut = true;
                return false;
            }

        }

        return true;
    }

    public void DetachToOutput(Interface_Instruction instruction, string key)
    {
        if (key == "")
        {
            return;
        }

        string[] trueKeys = key.Split(";");
        string cleaned;

        foreach (var item in trueKeys)
        {
            cleaned = item.Trim();
            if (cleaned == "")
            {
                continue;
            }
            foreach (var proc in listOfProcess)
            {
                if (proc.key == cleaned)
                {
                    proc.GetProcess().OutputDataReceived -= instruction.ParseOutputData;
                    break;
                }
            }
        }

        instructionAttachedToOutput.Remove(instruction);
    }

    public void NoteProcess(Cache_Process cache)
    {
        if (cache.key == "")
        {
            // this process is not supposed to be interacted with
            return;
        }

        foreach (var item in instructionAttachedToClosing)
        {
            foreach (var keys in item.Value)
            {
                if (keys == cache.key.Trim())
                {
                    Trace.WriteLine("Key is correct, subscribing " + item.Key.GetInstructionType() + " to key " + cache.key);
                    cache.GetProcess().Exited += item.Key.ParseProcess;
                }
            }
        }

        foreach (var item in instructionAttachedToOutput)
        {
            foreach (var keys in item.Value)
            {
                if (keys == cache.key.Trim())
                {
                    Trace.WriteLine("Key is correct, subscribing " + item.Key.GetInstructionType() + " to key " + cache.key);
                    cache.GetProcess().OutputDataReceived += item.Key.ParseOutputData;
                }
            }
        }

        listOfProcess.Add(cache);
    }
}