using YAAL.Assets.Scripts;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.BackupSettings;
using static YAAL.LauncherSettings;

namespace YAAL
{

    // Doc : auto-restore types, use key to filter which process it looks at: 
    // - Process : when a process exits
    // - Output : when a process's output contains string 
    // - Combined : whatever happens first between Process mode and Output mode
    // - Timer : when timer expires (in seconds)
    public class Backup : Instruction<BackupSettings>
    {
        private List<string> backedUpFile = new List<string>();
        private List<string> outputToLookFor = new List<string>();
        public bool waitingForRestore = false;
        private float time;

        public Backup()
        {
            instructionType = "Backup";
        }

        public override bool Execute()
        {
            List<string> splitTarget = executer.Parser.SplitAndParse(this.InstructionSetting[target]);
            List<string> splitDefault = executer.Parser.SplitAndParse(this.InstructionSetting[defaultFile]);
            Dictionary<string, string> BackupAndDefault = new Dictionary<string, string>();
            backedUpFile = new List<string>();

            if(splitTarget.Count == 0)
            {
                ErrorManager.AddNewError(
                    "Backup - No target",
                    "Backup is missing targets. If you did set some and you're getting this error, please report it."
                    );
                return false;
            }

            switch (splitDefault.Count)
            {
                case 0:
                    foreach (var item in splitTarget)
                    {
                        BackupAndDefault[item] = "";
                    }
                    break;
                case 1:
                    if (splitDefault[0].Trim() == "")
                    {
                        foreach (var item in splitTarget)
                        {
                            BackupAndDefault[item] = "";
                        }
                        break;
                    }

                    if (!IOManager.CopyToDefault(executer.SettingsHandler.GetSetting(launcherName), splitDefault[0], out string singleDefault))
                    {
                        ErrorManager.AddNewError(
                            "Backup - Failed to copy defaults",
                            "Something went wrong while trying to copy new default : " + this.InstructionSetting[defaultFile]);
                        return false;
                    }
                    this.InstructionSetting[defaultFile] = singleDefault.Trim().Trim(';');
                    executer.InstructionHandler.UpdateCache(this);
                    for (int i = 0; i < splitTarget.Count; i++)
                    {
                        BackupAndDefault[splitTarget[i]] = singleDefault;
                    }
                    break;
                default:
                    if (splitTarget.Count != splitDefault.Count)
                    {
                        ErrorManager.AddNewError(
                            "Backup - Invalid number of default file",
                            "Backup was asked to backup " + splitTarget.Count + " files or folders, but was only given only " + splitDefault.Count + " default files. Either provide one (and only one) default for each file / folder, or provide none."
                            );
                        return false;
                    }
                    List<string> trueDefaults = new List<string>();
                    string newDefault;
                    string newSetting = "";

                    foreach (var item in splitDefault)
                    {
                        if (item == "" || item == " ")
                        {
                            trueDefaults.Add("");
                            newSetting += "\" \"";
                            continue;
                        }
                        if (!IOManager.CopyToDefault(executer.SettingsHandler.GetSetting(launcherName), item, out newDefault))
                        {
                            ErrorManager.AddNewError(
                                "Backup - Failed to copy defaults",
                                "Something went wrong while trying to copy new default : " + this.InstructionSetting[defaultFile]);
                            return false;
                        }
                        trueDefaults.Add(newDefault);
                        newSetting += "\"" + newDefault + "\"; ";
                    }

                    this.InstructionSetting[defaultFile] = newSetting.Trim().Trim(';');
                    executer.InstructionHandler.UpdateCache(this);

                    for (int i = 0; i < splitTarget.Count; i++)
                    {
                        BackupAndDefault[splitTarget[i]] = trueDefaults[i];
                    }
                    break;
            }


            foreach (var item in BackupAndDefault)
            {
                if(IOManager.Backup(
                    item.Key,
                    item.Value,
                    settings["asyncName"],
                    settings["slotLabel"],
                    (this.InstructionSetting[modeSelect] != "off")))
                {
                    backedUpFile.Add(item.Key);
                } else
                {
                    ErrorManager.AddNewError(
                            "Backup - Failed to backup",
                            "Failed to backup: " + item.Key
                            );
                    return false;
                }
            }

            switch (this.InstructionSetting[modeSelect])
            {
                case "off":
                    return true;
                case "process":
                    if (!executer.ProcessHandler.AttachToClosing(this, this.InstructionSetting[processName] ?? ""))
                    {
                        Restore();
                        return false;
                    }
                    return true;
                case "output":
                    if (!executer.ProcessHandler.AttachToOutput(this, this.InstructionSetting[processName] ?? ""))
                    {
                        Restore();
                        return false;
                    }
                    outputToLookFor = executer.Parser.SplitString(this.InstructionSetting[BackupSettings.outputToLookFor]);
                    return true;
                case "combined":
                    if (!executer.ProcessHandler.AttachToClosing(this, this.InstructionSetting[processName] ?? ""))
                    {
                        Restore();
                        return false;
                    }
                    if (!executer.ProcessHandler.AttachToOutput(this, this.InstructionSetting[processName] ?? ""))
                    {
                        Restore();
                        return false;
                    }
                    outputToLookFor = executer.Parser.SplitString(this.InstructionSetting[BackupSettings.outputToLookFor]);
                    return true;
                case "timer":
                    time = float.Parse(this.InstructionSetting[timer], CultureInfo.InvariantCulture.NumberFormat) * 0.1f;
                    Debouncer.timer.Tick += Timer;
                    executer.AddWait(this);
                    return true;
                default:
                    ErrorManager.AddNewError(
                        "Backup - Invalid modeSelect",
                        "Backup's autorestore condition is set to : " + this.InstructionSetting[modeSelect] + " which isn't valid. Please report this issue."
                        );
                    return false;
            }
        }

        private void Timer(object? sender, EventArgs e)
        {
            time -= 0.1f;
            if(time <= 0f)
            {
                Trace.WriteLine("Timer finished ! Beginning restore");
                Restore();
            }
        }

        public override void ParseProcess(object? sender, EventArgs e)
        {
            Restore();
        }

        public override void ParseOutputData(object sender, DataReceivedEventArgs e)
        {
            if(e.Data != null)
            {
                foreach (var item in outputToLookFor)
                {
                    if (e.Data.Contains(item))
                    {
                        Restore();
                    }
                }
            }
        }

        public bool Restore()
        {
            if (backedUpFile.Count == 0)
            {
                // something is asking us to Restore(), but we've not backed up anything yet
                // so we've got nothing else to do
                return true;
            }

            if(this.InstructionSetting[modeSelect] == "timer")
            {
                Debouncer.timer.Tick -= Timer;
            }

            switch (this.InstructionSetting[modeSelect])
            {
                case "process":
                    executer.ProcessHandler.DetachToClosing(this, this.InstructionSetting[processName] ?? "");
                    break;
                case "output":
                    executer.ProcessHandler.DetachToOutput(this, this.InstructionSetting[processName] ?? "");
                    
                    break;
                case "combined":
                    executer.ProcessHandler.DetachToClosing(this, this.InstructionSetting[processName] ?? "");
                    executer.ProcessHandler.DetachToOutput(this, this.InstructionSetting[processName] ?? "");
                    break;
                case "timer":
                    Debouncer.timer.Tick -= Timer;
                    break;
                default:
                    break;
            }

            bool success = true;

            foreach (var item in backedUpFile)
            {
                try
                {
                    if(!IOManager.Restore(
                        item,
                        settings[AsyncSettings.asyncName],
                        settings[SlotSettings.slotLabel]))
                    {
                        success = false;
                    }
                }
                catch (Exception e)
                {
                    ErrorManager.AddNewError(
                        "Backup - Restore threw an exception",
                        "The restore process threw the following exception : " + e.Message);
                    success = false;
                }
            }

            executer.RemoveWait(this);
            return success;
        }
    }
}
