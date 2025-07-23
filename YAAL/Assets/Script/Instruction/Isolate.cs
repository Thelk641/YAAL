using YAAL.Assets.Scripts;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.IsolateSettings;
using static YAAL.GeneralSettings;

namespace YAAL
{

    // Doc : auto-restore types, use key to filter which process it looks at: 
    // - Process : when a process exits
    // - Output : when a process's output contains string 
    // - Combined : whatever happens first between Process mode and Output mode
    // - Timer : when timer expires (in seconds)
    public class Isolate : Instruction<IsolateSettings>
    {
        public bool waitingForRestore = false;
        private float time;
        private List<string> targets = new List<string>();

        public Isolate()
        {
            instructionType = "Isolate";
        }

        public override bool Execute()
        {
            List<string> launcherTargets = customLauncher.GetApworlds();
            targets = new List<string>();

            if (launcherTargets.Count == 0) 
            {
                ErrorManager.AddNewError(
                            "Isolate - Empty target list",
                            "Tried to isolate apworlds, but none were given. Use the Apworld command to define them."
                            );
                return false;
            }
            
            // The user might want to version files other than the apworld, we need to ignore them
            foreach (var item in launcherTargets)
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(item);
                    string custom_world = Path.Combine(dir.Parent.Parent.FullName, "ArchipelagoLauncher.exe");
                    string worlds = Path.Combine(dir.Parent.Parent.Parent.FullName, "ArchipelagoLauncher.exe");

                    if (File.Exists(custom_world) || File.Exists(worlds))
                    {
                        targets.Add(item);
                    }
                }
                catch (Exception)
                {

                }
            }

            if (targets.Count == 0)
            {
                ErrorManager.AddNewError(
                            "Isolate - Empty target list",
                            "Tried to isolate apworlds, but none were given. Use the Apworld command to define them."
                            );
                return false;
            }

            if (!IOManager.IsolateApworlds(settings[aplauncher], targets))
            {
                string targetList = "";
                foreach (var item in targets)
                {
                    targetList += item + " ";
                }

                ErrorManager.AddNewError(
                            "Isolate - Failed to isolate",
                            "Couldn't isolate the following apworlds : " + targetList
                            );
                return false;
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
                    customLauncher.NoteBackup(this);
                    time = float.Parse(this.InstructionSetting[timer], CultureInfo.InvariantCulture.NumberFormat) * 0.1f;
                    Debouncer.timer.Tick += Timer;
                    return true;
                default:
                        ErrorManager.AddNewError(
                            "Isolate - Invalid modeSelect",
                            "Isolate's autorestore condition is set to : " + this.InstructionSetting[modeSelect] + " which isn't valid. Please report this issue."
                            );
                        return false;
            }
        }

        private void Timer(object? sender, EventArgs e)
        {
            time -= 0.1f;
            if(time <= 0f)
            {
                Restore();
            }
        }

        public override void ParseProcess(object? sender, EventArgs e)
        {
            Debug.WriteLine("Isolate is parsing a process");
            Restore();
        }

        public override void ParseOutputData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && e.Data.Contains(this.InstructionSetting[outputToLookFor]))
            {
                Restore();
            }
        }

        public bool Restore()
        {
            if (!waitingForRestore)
            {
                // something is asking us to Restore(), but we've not isolated anything yet
                // so we've got nothing else to do
                return true;
            }
            waitingForRestore = false;

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

            try
            {
                if(IOManager.RestoreApworlds(settings[aplauncher]))
                {
                    customLauncher.NoteRestore(this);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                        "Isolate - Restore threw an exception",
                        "The restore process threw the following exception : " + e.Message);
                return false;
            }
        }
    }
}
