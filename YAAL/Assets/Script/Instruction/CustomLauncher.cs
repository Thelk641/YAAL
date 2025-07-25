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
    public static event Action DoneRestoring;

    // Permanent settings, saved in launcher.json
    // Defaults set in New Launcher.axaml.cs
    public Dictionary<LauncherSettings, string> selfsettings = new Dictionary<LauncherSettings, string>();
    public Dictionary<string, string> customSettings = new Dictionary<string, string>();

    // Temporary settings, not saved, used when executing
    public UnifiedSettings settings = new UnifiedSettings();


    public bool Execute()
    {
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
            
            if (!instruction.Execute())
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
            previousSlot = this.settings[slotName] ?? "",
            previousPatch = this.settings[patch] ?? "",
            previousVersion = this.settings[version] ?? ""
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
            settings[item.Key] = item.Value;
        }

        if (!isGame)
        {
            settings[version] = IOManager.GetToolVersion(async, selfsettings[launcherName]);
        }

        if (isDebug && selfsettings[Debug_Patch] != null)
        {
            settings[patch] = selfsettings[Debug_Patch];
        }

        // pretty sure these two are just useless, but just in case someone wants them one day...
        Cache_PreviousSlot cache = IOManager.GetLastAsync(this.selfsettings[launcherName]);
        settings[previousAsync] = cache.previousAsync;
        settings[previousSlot] = cache.previousSlot;
    }

    public void ReadCache(Cache_CustomLauncher cache)
    {
        this.selfsettings = cache.settings;
        this.customSettings = cache.customSettings;
        listOfInstructions = new List<Interface_Instruction>();
        foreach (var item in cache.instructions)
        {
            // item.Key is "Nbr-InstructionName"
            int dashIndex = item.Key.IndexOf('-');
            string cleaned = item.Key.Substring(dashIndex + 1);

            Templates.instructionsTemplates.TryGetValue(cleaned, out var instructionType);
            var instruction = Activator.CreateInstance(instructionType) as Interface_Instruction;
            listOfInstructions.Add(instruction);
            instruction.SetCustomLauncher(this);
            instruction.SetSettings(item.Value);
            Apworld apworldInstruction = instruction as Apworld;
            if (apworldInstruction != null)
            {
                string target = apworldInstruction.GetTarget();
                if (target.Contains(";"))
                {
                    foreach (var value in IOManager.SplitPathList(target))
                    {
                        apworld.Add(value);
                    }
                }
                else
                {
                    apworld.Add(target);
                }
            }
        }
        this.isGame = cache.isGame;
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

    public string GetSetting(string key)
    {
        if (Enum.TryParse(key, out LauncherSettings setting))
        {
            return selfsettings[setting];
        }
        else
        {
            ErrorManager.ThrowError(
            "CustomLauncher - Setting doesn't exists",
            "Tried to access setting " + key + " but it sadly doesn't exists in selfSettings. Please report this issue.");
            return "";
        }
    }

    public string GetSetting(LauncherSettings key)
    {
        return selfsettings[key];
    }

    public string ParseTextWithSettings(string text)
    {
        CustomLauncher baseLauncher = null;
        if(settings[SlotSettings.baseLauncher] != this.selfsettings[launcherName])
        {
            baseLauncher = GetBaseLauncher();
        }

        string tempString = text;

        foreach (Match m in Regex.Matches(text, @"\$\{baseSetting:(?<key>[^}]+)\}"))
        {
            string key = m.Groups["key"].Value;
            string pattern = "${baseSetting:" + key + "}";

            tempString = tempString.Replace(pattern, baseLauncher.ParseTextWithSettings("${" + key + "}"));
        }

        text = tempString;

        if(text.Contains("${base:apworld}"))
        {
            string apworldList = "\"";
            foreach (var item in baseLauncher.apworld)
            {
                apworldList += item + ";";
            }
            text = text.Replace("${base:apworld}", apworldList + "\"");
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
                bool needsQuote;
                foreach (var item in splitText)
                {
                    needsQuote = false;
                    if (item == "" || item == "\"" || item == " ")
                    {
                        continue;
                    }

                    cleaned = item;

                    if (item.StartsWith("\"") || item.EndsWith("\""))
                    {

                        do
                        {
                            cleaned = cleaned.Trim('\"').Trim();
                        } while (cleaned.StartsWith("\"") || cleaned.EndsWith("\""));

                        needsQuote = true;
                        output += "\"";
                    }

                    string[] split = cleaned.Split('}');
                    if (settings.Has(split[0]))
                    {
                        output = output + settings[split[0]];
                    }
                    else
                    {
                        output = output + split[0];
                    }

                    if (needsQuote)
                    {
                        output += "\"";
                    }

                    output = output + split[1] + " ";
                }

                text = output.Trim();
                ++i;
            }
        }

        if(text.Contains(".apworld") && !text.Contains("\\"))
        {
            string custom_world = Path.Combine(settings[GeneralSettings.apfolder], "custom_folder", text);
            string lib_world = Path.Combine(settings[GeneralSettings.apfolder], "lib", "worlds", text);

            if (File.Exists(custom_world))
            {
                text = custom_world;
            } else if (File.Exists(lib_world))
            {
                text = lib_world;
            }
        }

        return text;
    }

    public void SetSlotSetting(SlotSettings key, string value)
    {
        IOManager.SetSlotSetting(this.settings[asyncName], this.settings[slotName], key, value);
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
                return null;
            }
            CustomLauncher output = IOManager.LoadLauncher(settings[SlotSettings.baseLauncher]);
            output.ReadSettings(settings[asyncName], settings[slotName]);
            _baseLauncher = output;
            return output;
        } else
        {
            return _baseLauncher;
        }
    }

    public List<string> GetApworlds()
    {
        List<string> output = new List<string>();
        bool hasAddedOwnApworld = false;
        foreach (var item in listOfInstructions)
        {
            Apworld apworld = item as Apworld;
            if(apworld != null)
            {
                output.Add(apworld.GetTarget());
            }

            if (!hasAddedOwnApworld)
            {
                Patch patch = item as Patch;
                if(patch != null)
                {
                    output.Add(Path.Combine(settings[GeneralSettings.apfolder], "custom_worlds", "YAAL.apworld"));
                }
            }
        }

        if (_baseLauncher != null)
        {
            foreach (var item in _baseLauncher.GetApworlds())
            {
                if (!output.Contains(item))
                {
                    output.Add(item);
                }
            }
        }
        List<string> temp = new List<string>();
        foreach (var item in output)
        {
            if (item.Contains(";"))
            {
                foreach (var item1 in item.Split(";"))
                {
                    temp.Add(item1.Trim());
                }
            } else
            {
                temp.Add(item.Trim());
            }
        }

        output = new List<string>();

        foreach (var item in temp)
        {
            if(item.Trim() != "" && item != "${base:apworld}")
            {
                output.Add(ParseTextWithSettings(item));
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

        string[] trueKeys = key.Split(";");
        string cleaned;

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
            return false;
        }

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
                    proc.GetProcess().Exited += instruction.ParseProcess;
                }
            }

            try
            {
                instructionAttachedToClosing[instruction].Add(cleaned);
                NoteBackup(instruction);
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                "CustomLauncher - Tried attaching key twice",
                "Trying to attach an instruction triggered the following exception : " + e.Message
                );
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

        string[] trueKeys = key.Split(";");
        string cleaned;

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
            return false;
        }

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
                    proc.GetProcess().OutputDataReceived += instruction.ParseOutputData;
                }
            }

            try
            {
                instructionAttachedToOutput[instruction].Add(cleaned);
                NoteBackup(instruction);
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                "CustomLauncher - Tried attaching key twice",
                "Trying to attach an instruction triggered the following exception : " + e.Message
                );
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
                    Debug.WriteLine("Key is correct, subscribing " + item.Key.GetInstructionType() + " to key " + cache.key);
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
                    Debug.WriteLine("Key is correct, subscribing " + item.Key.GetInstructionType() + " to key " + cache.key);
                    cache.GetProcess().OutputDataReceived += item.Key.ParseOutputData;
                }
            }
        }

        listOfProcess.Add(cache);
    }

    public void NoteBackup(Interface_Instruction instruction)
    {
        instructionWaiting.Add(instruction);
        waitingForRestore = true;
    }

    public void NoteRestore(Interface_Instruction instruction)
    {
        Debug.WriteLine("Done restoring for instruction of type " + instruction.GetInstructionType());
        instructionWaiting.Remove(instruction);
        if(instructionWaiting.Count == 0)
        {
            waitingForRestore = false;
            DoneRestoring?.Invoke();
        }
    }

    public async Task<string> GetUpdate()
    {
        string newLink = selfsettings[githubURL] ?? "";
        if(newLink == "" || !WebManager.IsValidGitURL(newLink))
        {
            return "";
        }

        List<string> downloadables = await WebManager.GetVersions(newLink);
        List<string> downloaded = IOManager.GetDownloadedVersions(GetSetting(launcherName));

        if (downloadables[0] == downloadables[0])
        {
            return "";
        } else
        {
            return downloadables[0];
        }
    }
}