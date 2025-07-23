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
        public bool waitingForRestore = false;
        private float time;

        public Backup()
        {
            instructionType = "Backup";
        }

        public override bool Execute()
        {
            string target = customLauncher.ParseTextWithSettings(this.InstructionSetting[BackupSettings.target]);
            Debug.WriteLine(System.IO.File.Exists(target));

            if (this.InstructionSetting[defaultFile] != "")
            {
                string newDefault;
                if(!IOManager.CopyToDefault(customLauncher.GetSetting(launcherName), this.InstructionSetting[defaultFile], out newDefault))
                {
                    ErrorManager.AddNewError(
                        "Backup - Failed to copy defaults",
                        "Something went wrong while trying to copy new default : " + this.InstructionSetting[defaultFile]);
                    return false;
                }
                InstructionSetting[defaultFile] = newDefault;
            }

            Debug.WriteLine("Trying to backup " + target);


            if (IOManager.Backup(
                target,
                this.InstructionSetting[defaultFile],
                settings["asyncName"],
                settings["slotName"],
                (this.InstructionSetting[modeSelect] != "off")
                ))
            {
                switch (this.InstructionSetting[modeSelect])
                {
                    case "off":
                        return true;
                    case "process":
                        if(!customLauncher.AttachToClosing(this, this.InstructionSetting[processName] ?? ""))
                        {
                            Restore();
                            return false;
                        }
                        waitingForRestore = true;
                        return true;
                    case "output":
                        if (!customLauncher.AttachToOutput(this, this.InstructionSetting[processName] ?? ""))
                        {
                            Restore();
                            return false;
                        }
                        waitingForRestore = true;
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
                        waitingForRestore = true;
                        return true;
                    case "timer":
                        waitingForRestore = true;
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
            ErrorManager.AddNewError(
                            "Backup - Failed to backup",
                            "Failed to backup: " + target
                            );
            return false;
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
            if (!waitingForRestore)
            {
                // something is asking us to Restore(), but we've not backed up anything yet
                // so we've got nothing else to do
                return true;
            }


            waitingForRestore = false;
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

            string target = customLauncher.ParseTextWithSettings(this.InstructionSetting[BackupSettings.target]);

            try
            {
                if (IOManager.Restore(
                    target,
                    settings[AsyncSettings.asyncName],
                    settings[SlotSettings.slotName]))
                {
                    customLauncher.NoteRestore(this);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                        "Backup - Restore threw an exception",
                        "The restore process threw the following exception : " + e.Message);
                return false;
            }
        }
    }
}
