using YAAL;
using YAAL.Assets.Script.Cache;
using YAAL.Assets.Scripts;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;
using static YAAL.LauncherSettings;
using static YAAL.PreviousAsyncSettings;
using static YAAL.SlotSettings;
using System.Text;
using Avalonia.Input;
using Avalonia.Utilities;

public class CustomLauncher
{
    public List<Interface_Instruction> listOfInstructions = new List<Interface_Instruction>();
    public List<Backup> backups = new List<Backup>();
    public List<Cache_Process> listOfProcess = new List<Cache_Process>();
    public Dictionary<Interface_Instruction, List<string>> instructionAttachedToClosing = new Dictionary<Interface_Instruction, List<string>>();
    public Dictionary<Interface_Instruction, List<string>> instructionAttachedToOutput = new Dictionary<Interface_Instruction, List<string>>();
    public List<string> apworld = new List<string>();
    public bool isGame = true;
    public CustomLauncher _baseLauncher = null;
    public bool waitingForRestore = false;
    private List<Interface_Instruction> instructionWaiting = new List<Interface_Instruction>();
    public event Action DoneRestoring;
    public bool requiresPatch = false;
    public bool requiresVersion = false;
    private bool hasErroredOut = false;

    // Permanent settings, saved in launcher.json
    // Defaults set in DefaultManager
    public Dictionary<LauncherSettings, string> selfsettings = new Dictionary<LauncherSettings, string>();
    public Dictionary<string, string> customSettings = new Dictionary<string, string>();

    // Temporary settings, not saved, used when executing
    public UnifiedSettings settings = new UnifiedSettings();

    public CustomLauncher()
    {
        settings[LauncherSettings.launcherName] = "New Launcher";
        DoneRestoring += () => { Trace.WriteLine("Done restoring !"); };
    }


    public bool Execute()
    {
        hasErroredOut = false;
        listOfProcess = new List<Cache_Process>();
        instructionAttachedToOutput = new Dictionary<Interface_Instruction, List<string>>();
        instructionAttachedToClosing = new Dictionary<Interface_Instruction, List<string>>();
        if (!isGame)
        {
            GetBaseLauncher();
        }

        foreach (var instruction in listOfInstructions)
        {
            instruction.SetExecuteSettings(settings);
            
            if (!instruction.Execute() || hasErroredOut)
            {
                ErrorManager.AddNewError(
                    "CustomLauncher - Failed to Execute()",
                    "An instruction has returned false, leading to this launcher failing midway. The last instruction tried was of type : " + instruction.GetInstructionType() + ". Please check other errors for the main cause.");
                ErrorManager.ThrowError();

                List<Backup> list = new List<Backup>();

                foreach (var item in listOfInstructions)
                {
                    switch (item.GetInstructionType())
                    {
                        case "Backup":
                            Backup backup = item as Backup;
                            if (!backup.Restore())
                            {
                                ErrorManager.AddNewError(
                                    "CustomLauncher - Failed to Restore()",
                                    "For some reason a restore failed."
                                    );
                                ErrorManager.ThrowError();
                            }
                            break;
                        case "Isolate":
                            Isolate isolate = item as Isolate;
                            if (!isolate.Restore())
                            {
                                ErrorManager.AddNewError(
                                    "CustomLauncher - Failed to Restore()",
                                    "For some reason a restore failed."
                                    );
                                ErrorManager.ThrowError();
                            }
                            break;
                    }
                }
                return false;
            }
        }

        Cache_PreviousSlot newSlot = new Cache_PreviousSlot()
        {
            previousAsync = this.settings[asyncName] ?? "",
            previousSlot = this.settings[slotLabel] ?? "",
            previousPatch = this.settings[patch] ?? "",
            previousVersion = this.settings[version] ?? "",
            previousRoom = this.settings[roomAddress] ?? "" + ":" + this.settings[roomPort] ?? "",
            previousPort = this.settings[roomPort] ?? ""
        };

        IOManager.UpdateLastAsync(this.settings[gameName] ?? "", newSlot);
        return true;
    }

    public void MoveInstructionUp(Interface_Instruction instruction)
    {
        int index = listOfInstructions.IndexOf(instruction);
        listOfInstructions.Remove(instruction);
        listOfInstructions.Insert(index - 1, instruction);
    }

    public void MoveInstructionDown(Interface_Instruction instruction)
    {
        int index = listOfInstructions.IndexOf(instruction);
        listOfInstructions.Remove(instruction);
        listOfInstructions.Insert(index + 1, instruction);
    }

    public void ResetInstructionList(List<Interface_Instruction> newList)
    {
        listOfInstructions = newList;
    }

    public void RemoveInstruction(Interface_Instruction instruction)
    {
        listOfInstructions.Remove(instruction);
    }

    public void ReadSettings(string async, string slot, bool isDebug = false)
    {
        settings = new UnifiedSettings();
        // Read all the default settings
        foreach (var item in selfsettings)
        {
            settings[item.Key] = item.Value;
        }

        Dictionary<string, string> slotSettings = IOManager.GetSettings(async, slot).settings;

        // Read all the general, async-specific and slot-specific settings
        foreach (var item in slotSettings)
        {
            settings[item.Key] = item.Value;
        }

        // Read all the custom settings, these might override the ones above
        foreach (var item in customSettings)
        {
            if(item.Value != "")
            {
                settings[item.Key] = item.Value;
            }
        }

        if (!isGame)
        {
            settings[version] = IOManager.GetToolVersion(async, selfsettings[launcherName]);
        }

        if (isDebug && selfsettings[Debug_Patch] != null)
        {
            settings[patch] = selfsettings[Debug_Patch];
        }

        Cache_PreviousSlot cache = IOManager.GetLastAsync(this.selfsettings[launcherName]);
        settings[previousAsync] = cache.previousAsync;
        settings[previousSlot] = cache.previousSlot;
        settings[previousPatch] = cache.previousPatch;
        settings[previousVersion] = cache.previousVersion;
        settings[previousRoom] = cache.previousRoom;
        settings[previousPort] = cache.previousPort;
    }

    public bool ReadCache(Cache_CustomLauncher cache)
    {
        this.settings[LauncherSettings.apworld] = "";
        this.selfsettings = cache.settings;
        this.customSettings = cache.customSettings;
        listOfInstructions = new List<Interface_Instruction>();
        foreach (var item in cache.instructionList)
        {
            if(item is Interface_CommandSetting command)
            {
                string commandName = command.GetCommandType();

                if (Templates.GetCommandWithEnum(commandName) is Type commandType
                    && Templates.GetInstruction(commandName) is Type instructionType
                    && Activator.CreateInstance(instructionType) is Interface_Instruction tempInstruction)
                {
                    dynamic commandSetting = Convert.ChangeType(item, commandType);
                    dynamic instruction = Convert.ChangeType(tempInstruction, instructionType);
                    instruction.SetSettings(commandSetting.InstructionSetting);
                    instruction.SetCustomLauncher(this);
                    listOfInstructions.Add(instruction);
                    if (instruction is Apworld apworldInstruction)
                    {
                        string target = apworldInstruction.GetTarget();
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
                        requiresVersion = true;
                    }
                    else if (instruction is Patch patchInstruction)
                    {
                        requiresVersion = true;
                        requiresPatch = true;
                    }
                } else
                {
                    ErrorManager.ThrowError(
                        "CustomLauncher - Failed to read cache",
                        "Something went wrong while reading an instruction of type " + commandName
                        );
                    return false;
                }
            }
            
        }
        this.isGame = cache.isGame;
        return true;
    }

    public Cache_CustomLauncher WriteCache()
    {
        Cache_CustomLauncher output = new Cache_CustomLauncher();
        int i = 0;
        foreach (Interface_Instruction instruction in listOfInstructions)
        {
            output.instructions[i + "-" + instruction.GetInstructionType()] = instruction.GetSettings();
            ++i;
        }
        output.settings = this.selfsettings;
        output.customSettings = this.customSettings;
        output.isGame = this.isGame;

        return output;
    }

    public void SetSetting(string key, string value)
    {
        // This is used to set settings for the next time around
        // So, for example, rom path if we've patched something successfully
        // For temporary settings like version during a Test,
        // use settings[name] = value instead
        if (Enum.TryParse(key, out LauncherSettings setting)) {
            selfsettings[setting] = value;
        } else
        {
            ErrorManager.ThrowError(
                "CustomLauncher - Invalid selfSetting key", 
                "Tried to set a value for key " + key + " but it isn't a valid selfSetting. Please report this issue."
                );
        }
            
    }

    public void SetSetting(LauncherSettings key, string value)
    {
        selfsettings[key] = value;
    }

    public void SetTemporarySetting(string key, string value)
    {
        settings[key] = value;
    }

    public string GetSetting(string key)
    {
        if (Enum.TryParse(key, out LauncherSettings setting))
        {
            if (selfsettings.ContainsKey(setting))
            {
                return selfsettings[setting];
            }
            return "";
        }
        else
        {
            ErrorManager.ThrowError(
            "CustomLauncher - Setting doesn't exists",
            "Tried to access setting " + key + " but it isn't a valid LauncherSetting. Please report this issue.");
            return "";
        }
    }

    public string GetSetting(LauncherSettings key)
    {
        return selfsettings[key];
    }

    public string? GetTemporarySetting(string key)
    {
        return settings[key];
    }

    public List<string> ParseTextWithSettings(List<string> input, bool clearQuote = true)
    {
        List<string> output = new List<string>();
        foreach (var item in input)
        {
            output.Add(ParseTextWithSettings(item, clearQuote));
        }
        return output;
    }
    
    public string ParseTextWithSettings(string text, bool clearQuote = true)
    {
        if(text == null)
        {
            ErrorManager.AddNewError(
                "CustomLauncher - Tried to parse null",
                "Something asked the Custom Launcher to parse a null text. This shouldn't ever happen. Please report this issue."
                );
            hasErroredOut = true;
            return "";
        }

        if (text.EndsWith(";"))
        {
            text = text.TrimEnd(';');
        }

        if (!text.Contains("${"))
        {
            while (text.StartsWith("\"") && clearQuote)
            {
                text = text.TrimStart('\"');
            }

            while (text.EndsWith("\"") && clearQuote)
            {
                text = text.TrimEnd('\"');
            }

            if (text.Contains(".apworld") && !text.Contains("\\"))
            {
                return IOManager.FindApworld(this.settings[LauncherSettings.apfolder], text);
            }

            return text;
        }


        CustomLauncher baseLauncher = null;
        if(settings[SlotSettings.baseLauncher] != this.selfsettings[launcherName])
        {
            baseLauncher = GetBaseLauncher();
        }

        string tempString = text;

        if (text.Contains("${baseSetting:"))
        {
            if (isGame)
            {
                ErrorManager.AddNewError(
                    "CustomLauncher - Tried to use tool-specific settings in non-tool launcher",
                    "YAAL was asked to parse a baseSetting in a game. This is not allowed. BaseSettings are only there for tools."
                    );
                hasErroredOut = true;
                return "";
            }
            foreach (Match m in Regex.Matches(text, @"\$\{baseSetting:(?<key>[^}]+)\}"))
            {

                string key = m.Groups["key"].Value;
                string pattern = "${baseSetting:" + key + "}";

                tempString = tempString.Replace(pattern, baseLauncher.ParseTextWithSettings("${" + key + "}"));
            }
        }

        text = tempString;

        if(text.Contains("${base:apworld}"))
        {
            if (isGame)
            {
                ErrorManager.AddNewError(
                    "CustomLauncher - Tried to use tool-specific settings in non-tool launcher",
                    "YAAL was asked to parse base:apworld in a game. This is not allowed. 'Base' options are only there for tools."
                    );
                hasErroredOut = true;
                return "";
            }
            text = text.Replace("${base:apworld}", baseLauncher.settings[LauncherSettings.apworld]);
        }

        if (text.Contains("${apworld}"))
        {
            string apworldList = "";
            foreach (var item in GetApworlds())
            {
                apworldList += "\"" + item + "\";";
            }

            text = text.Replace("${apworld}", apworldList);
        }

        int i = 0;
        while(text.Contains("${") && i < 10)
        {
            // Text might contain things like ${aplauncher}, we're replacing those by their value
            string[] splitText = text.Split("${");

            if (splitText.Length > 1)
            {
                string output = "";
                string cleaned;
                foreach (var item in splitText)
                {
                    if (item == "" || item == "\"" || item == " ")
                    {
                        continue;
                    }

                    if (!item.Contains("}"))
                    {
                        output += item;
                        continue;
                    }

                    cleaned = item;

                    if (item.Trim().StartsWith("\""))
                    {
                        do
                        {
                            cleaned = cleaned.TrimStart('\"').Trim();
                        } while (cleaned.StartsWith("\""));

                        output += "\"";
                    }

                    string[] split = cleaned.Split('}');
                    if(split.Length > 1)
                    {
                        if (settings.Has(split[0]))
                        {
                            try
                            {
                                if (split[0] == slotInfo.ToString())
                                {
                                    output =
                                        output
                                        + "\""
                                        + settings[slotName].Trim()
                                        + ":"
                                        + settings[password]
                                        + "@"
                                        + settings[roomAddress]
                                        + ":"
                                        + settings[roomPort]
                                        + "\"";
                                }
                                else if (split[0] == HardcodedSettings.connect.ToString())
                                {
                                    output =
                                        output
                                        + "\""
                                        + "--connect "
                                        + settings[slotName].Trim()
                                        + ":"
                                        + settings[password]
                                        + "@"
                                        + settings[roomAddress]
                                        + ":"
                                        + settings[roomPort]
                                        + "\"";
                                }
                                else
                                {
                                    output = output + settings[split[0]].Trim();
                                }
                            }
                            catch (Exception e)
                            {
                                ErrorManager.AddNewError(
                                    "CustomLauncher - Exception while reading settings",
                                    "Trying to parse " + split[0] + " lead to the following exception : " + e.Message
                                    );
                                hasErroredOut = true;
                                return "";
                            }
                            
                                
                        } else if (split[0] == "apDebug" && settings.Has(GeneralSettings.aplauncher))
                        {
                            output = output + IOManager.ToDebug(settings[GeneralSettings.aplauncher]);
                        } else
                        {
                            ErrorManager.AddNewError(
                                "CustomLauncher - Variable name doesn't exist.",
                                "Trying to parse text with settings failed, variable " + split[0] + " doesn't appear to exist."
                                );
                            hasErroredOut = true;
                            return "";
                        }

                        output = output + split[1];
                    } else
                    {
                        output = output + split[0];
                    }
                    
                }

                text = output.Trim();
                ++i;
            }
        }

        return text;
    }

    public List<string> SplitAndParse(string input, bool clearQuote = true)
    {
        List<string> output = new List<string>();

        if (input.Contains(";"))
        {
            List<string> parsedSplit = SplitString(input);
            foreach (var item in parsedSplit)
            {
                output.Add(ParseTextWithSettings(item, clearQuote));
            }
            return output;
        }

        string parsed = ParseTextWithSettings(input, clearQuote);

        
        if(parsed == "\" \"")
        {
            output.Add("");
        } else
        {
            output.Add(parsed);
        }
        return output;
    }

    public List<string> SplitString(string input)
    {
        List<string> output = new List<string>();
        if (!input.Contains(";"))
        {
            output.Add(input);
            return output;
        }

        bool inQuotes = false;
        StringBuilder current = new StringBuilder();

        foreach (char c in input)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                current.Append(c);
            }
            else if (c == ';' && !inQuotes)
            {
                if (current.ToString().EndsWith("\"\""))
                {
                    output.Add(current.ToString().Trim().Trim('\"').Trim() + "\"");
                } else
                {
                    output.Add(current.ToString().Trim().Trim('\"').Trim());
                }
                    
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
        {
            string toAdd = current.ToString().Trim();
            if(toAdd.Trim('\"').Trim() == "")
            {
                output.Add("");
            } else
            {
                output.Add(current.ToString());
            }
        }

        return output;
    }

    public void SetSlotSetting(SlotSettings key, string value)
    {
        IOManager.SetSlotSetting(this.settings[asyncName], this.settings[slotLabel], key, value);
        settings[key] = value;
    }

    public void Save()
    {
        IOManager.SaveLauncher(this);
    }

    public CustomLauncher GetBaseLauncher()
    {
        if(_baseLauncher == null)
        {
            string name = settings[SlotSettings.baseLauncher];
            if (name == null || name == "")
            {
                ErrorManager.AddNewError(
                    "CustomLauncher - Tried to access empty launcher",
                    "A customLauncher tried to access its baseLauncher (the game the tool is for), but this launcher doesn't have a baselauncher. Please report this issue."
                    );
                hasErroredOut = true;
                return null;
            }
            CustomLauncher output = IOManager.LoadLauncher(settings[SlotSettings.baseLauncher]);
            output.ReadSettings(settings[asyncName], settings[slotLabel]);
            _baseLauncher = output;
            return output;
        } else
        {
            return _baseLauncher;
        }
    }

    public List<string> GetApworlds()
    {
        List<string> temp = new List<string>();
        bool hasAddedOwnApworld = false;
        foreach (var item in listOfInstructions)
        {
            Apworld apworld = item as Apworld;
            if(apworld != null)
            {
                temp.Add(apworld.GetTarget());
            }

            if (!hasAddedOwnApworld)
            {
                Patch patch = item as Patch;
                if(patch != null)
                {
                    temp.Add("YAAL.apworld");
                    hasAddedOwnApworld = true;
                }
            }
        }

        if (_baseLauncher != null && _baseLauncher.selfsettings[LauncherSettings.launcherName] != this.selfsettings[LauncherSettings.launcherName])
        {
            foreach (var item in _baseLauncher.GetApworlds())
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
            foreach (var trueItem in SplitString(item))
            {
                string cleaned = trueItem.Trim();
                if (cleaned != "" && cleaned != "${base:apworld}")
                {
                    output.Add(ParseTextWithSettings(cleaned));
                }
            }
        }

        return output;
    }

    public bool AttachToClosing(Interface_Instruction instruction, string key)
    {
        if(key == "")
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
                "CustomLauncher - Tried attaching instruction twice",
                "Trying to attach an instruction triggered the following exception : " + e.Message
                );
            hasErroredOut = true;
            return false;
        }

        string cleaned;

        foreach (var item in SplitString(key))
        {
            cleaned = item.Trim();
            if(cleaned == "")
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
                AddWait(instruction);
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                "CustomLauncher - Tried attaching key twice",
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
            if(cleaned == "")
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
                "CustomLauncher - Tried attaching instruction twice",
                "Trying to attach an instruction triggered the following exception : " + e.Message
                );
            hasErroredOut = true;
            return false;
        }

        string cleaned;

        foreach (var item in SplitString(key))
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
                AddWait(instruction);
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                "CustomLauncher - Tried attaching key twice",
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
                if(keys == cache.key.Trim())
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
        if(instructionWaiting.Count == 0)
        {
            waitingForRestore = false;
            DoneRestoring?.Invoke();
        }
    }

    public bool ReadyToClose()
    {
        return instructionWaiting.Count == 0;
    }
}