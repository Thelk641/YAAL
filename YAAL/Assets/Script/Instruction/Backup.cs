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
        public bool waitingForRestore = false;
        private float time;

        public Backup()
        {
            instructionType = "Backup";
        }

        public override bool Execute()
        {
            List<string> splitTarget = customLauncher.SplitAndParse(this.InstructionSetting[target]);
            List<string> splitDefault = customLauncher.SplitAndParse(this.InstructionSetting[defaultFile]);
            Dictionary<string, string> BackupAndDefault = new Dictionary<string, string>();

            if(splitTarget.Count == 0)
            {
                ErrorManager.AddNewError(
                    "Backup - No target",
                    "Backup is missing targets. If you did set some and you're getting this error, please report it."
                    );
                return false;
            }

            if(splitDefault.Count > 0)
            {
                if(splitTarget.Count != splitDefault.Count)
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
                    if(item == "")
                    {
                        trueDefaults.Add("");
                        newSetting += "\" \"";
                        continue;
                    }
                    if (!IOManager.CopyToDefault(customLauncher.GetSetting(launcherName), item, out newDefault))
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
                customLauncher.Save();

                for (int i = 0; i < splitTarget.Count; i++)
                {
                    BackupAndDefault[splitTarget[i]] = trueDefaults[i];
                }
            } else
            {
                foreach (var item in splitTarget)
                {
                    BackupAndDefault[item] = "";
                }
            }


            foreach (var item in BackupAndDefault)
            {
                if(IOManager.Backup(
                item.Key,
                item.Value,
                settings["asyncName"],
                settings["slotName"],
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
                    if (!customLauncher.AttachToClosing(this, this.InstructionSetting[processName] ?? ""))
                    {
                        Restore();
                        return false;
                    }
                    return true;
                case "output":
                    if (!customLauncher.AttachToOutput(this, this.InstructionSetting[processName] ?? ""))
                    {
                        Restore();
                        return false;
                    }
                    return true;
                case "combined":
                    if (!customLauncher.AttachToClosing(this, this.InstructionSetting[processName] ?? ""))
                    {
                        Restore();
                        return false;
                    }
                    if (!customLauncher.AttachToOutput(this, this.InstructionSetting[processName] ?? ""))
                    {
                        Restore();
                        return false;
                    }
                    return true;
                case "timer":
                    time = float.Parse(this.InstructionSetting[timer], CultureInfo.InvariantCulture.NumberFormat) * 0.1f;
                    Debouncer.timer.Tick += Timer;
                    customLauncher.NoteBackup(this);
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
                Debug.WriteLine("Timer finished ! Beginning restore");
                Restore();
            }
        }

        public override void ParseProcess(object? sender, EventArgs e)
        {
            Debug.WriteLine("Backup : process exited, starting restore.");
            Restore();
        }

        public override void ParseOutputData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && e.Data.Contains(this.InstructionSetting[outputToLookFor]))
            {
                Debug.WriteLine("Pattern found in output, starting restore. Output : " + e.Data);
                Restore();
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
                    customLauncher.DetachToClosing(this, this.InstructionSetting[processName] ?? "");
                    break;
                case "output":
                    customLauncher.DetachToOutput(this, this.InstructionSetting[processName] ?? "");
                    
                    break;
                case "combined":
                    customLauncher.DetachToClosing(this, this.InstructionSetting[processName] ?? "");
                    customLauncher.DetachToOutput(this, this.InstructionSetting[processName] ?? "");
                    break;
                case "timer":
                    Debouncer.timer.Tick -= Timer;
                    break;
                default:
                    break;
            }

            //string target = customLauncher.ParseTextWithSettings(this.InstructionSetting[BackupSettings.target]);

            foreach (var item in backedUpFile)
            {
                try
                {
                    if(!IOManager.Restore(
                    item,
                    settings[AsyncSettings.asyncName],
                    settings[SlotSettings.slotName]))
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    ErrorManager.AddNewError(
                        "Backup - Restore threw an exception",
                        "The restore process threw the following exception : " + e.Message);
                    return false;
                }
            }

            customLauncher.NoteRestore(this);
            return true;
        }
    }
}
