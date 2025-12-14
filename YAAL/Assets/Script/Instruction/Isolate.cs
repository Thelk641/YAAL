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
        private List<string> outputToLookFor = new List<string>();

        public Isolate()
        {
            instructionType = "Isolate";
        }

        public override bool Execute()
        {
            List<string> launcherTargets = customLauncher.GetApworlds();
            targets = new List<string>();

            if (!customLauncher.isGame)
            {
                foreach (var item in customLauncher.GetBaseLauncher().GetApworlds())
                {
                    launcherTargets.Add(item);
                }
            }

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
                    string clean = customLauncher.ParseTextWithSettings(item.Trim('\"'));
                    DirectoryInfo dir = new DirectoryInfo(clean);
                    string custom_world = Path.Combine(dir.Parent.Parent.FullName, "ArchipelagoLauncher.exe");
                    string worlds = Path.Combine(dir.Parent.Parent.Parent.FullName, "ArchipelagoLauncher.exe");

                    if (!targets.Contains(clean) && (File.Exists(custom_world) || File.Exists(worlds)))
                    {
                        targets.Add(clean);
                    } else
                    {
                        ErrorManager.AddNewError(
                            "Isolate - Couldn't find an apworld",
                            "Apworld " + item + " doesn't seem to exist. This error can be ignored if that apworld isn't necessary."
                            );
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

            waitingForRestore = true;

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
                
                Restore();
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
                    return true;
                case "output":
                    if (!customLauncher.AttachToOutput(this, this.InstructionSetting[processName] ?? ""))
                    {
                        Restore();
                        return false;
                    }
                    outputToLookFor = customLauncher.SplitString(this.InstructionSetting[IsolateSettings.outputToLookFor]);
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
                    outputToLookFor = customLauncher.SplitString(this.InstructionSetting[IsolateSettings.outputToLookFor]);
                    return true;
                case "timer":
                    customLauncher.AddWait(this);
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
            Trace.WriteLine("Isolate is parsing a process");
            Restore();
        }

        public override void ParseOutputData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                foreach (var item in outputToLookFor)
                {
                    if (e.Data.Contains(item))
                    {
                        Trace.WriteLine("Pattern found in output, starting restore. Output : " + e.Data);
                        Restore();
                    }
                }
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
                if(IOManager.RestoreApworlds(settings[aplauncher], targets))
                {
                    customLauncher.RemoveWait(this);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                        "Isolate - Restore threw an exception",
                        "The restore process threw the following exception : " + e.Message);
                customLauncher.RemoveWait(this);
                return false;
            }
        }
    }
}
