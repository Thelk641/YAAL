using YAAL.Assets.Script.Cache;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.LauncherSettings;
using static YAAL.PatchSettings;
using static YAAL.SlotSettings;

namespace YAAL
{

    // C:\ProgramData\Archipelago\ArchipelagoLauncherDebug.exe -- "YAAL Patcher" "D:\Unity\Avalonia port\P1_Player1_o4AtBzFYSPe3EajXijDHDQ.apblue"
    public class Patch : Instruction<PatchSettings>
    {
        public Patch()
        {
            instructionType = "Patch";
        }

        public override bool Execute()
        {
            if (!settings.Has(SlotSettings.patch) || settings[SlotSettings.patch] == "")
            {
                ErrorManager.AddNewError(
                    "Patch - No target selected",
                    "The slot you're trying to use doesn't appear to have a patch file selected."
                    );
                return false;
            }

            string patch = IOManager.MoveToSlotDirectory(
                settings[SlotSettings.patch],
                settings[AsyncSettings.asyncName],
                settings[slotName]
                );

            if (patch == "")
            {
                return false;
            } else
            {
                if (customLauncher.GetSetting(Debug_Patch) == settings[SlotSettings.patch])
                {
                    customLauncher.SetSetting(Debug_Patch, patch);
                }
                this.settings[SlotSettings.patch] = patch;
                customLauncher.SetSlotSetting(SlotSettings.patch, patch);
            }

            if (this.InstructionSetting[mode] == "Apply")
            {
                if (settings[rom] != "")
                {
                    // We've already applied that patch, nothing else to do
                    return true;
                }
                return ApplyPatch();
            }
            else if (this.InstructionSetting[mode] == "Copy")
            {
                return CopyPatch();
            }
            ErrorManager.AddNewError(
                "Patch - Invalid patching mode",
                "Patch's selected mode is " + this.InstructionSetting[mode] + " which is not valid. Please report this issue."
                );
            return false;
        }

        private bool ApplyPatch()
        {
            string apLauncher = customLauncher.ParseTextWithSettings("${aplauncher}");
            string folder = Path.Combine(Path.GetDirectoryName(apLauncher), "ArchipelagoLauncherDebug.exe");

            

            if (!File.Exists(Path.Combine(Path.GetDirectoryName(apLauncher), "custom_worlds", "YAAL.apworld")))
            {
                ErrorManager.AddNewError(
                    "Patch - Missing the YAAL apworld",
                    "For patching, YAAL requires its own apworld, please download it and put it in your custom_worlds folder"
                    );
                return false;
            }

            if (this.InstructionSetting[optimize] == true.ToString())
            {
                List<string> apworlds = customLauncher.apworld;
                if (!apworlds.Contains("YAAL.apworld"))
                {
                    apworlds.Add("YAAL.apworld");
                }
                if(!IOManager.IsolateApworlds(apLauncher, apworlds))
                {
                    ErrorManager.AddNewError(
                        "Patch - Failed to isolate apworlds",
                        "Patch's optimization threw an error. Most likely, you tried to isolate twice without first restoring, this is not allowed."
                        );
                    IOManager.RestoreApworlds(apLauncher);
                    return false;
                }
            }

            string path = folder;
            string args = customLauncher.ParseTextWithSettings(" -- \"YAAL Patcher\" \"" + this.settings[patch] + "\"");

            Debug.WriteLine("Process : " + path + args);

            Process process = ProcessManager.StartProcess(path, args);

            if(process == null)
            {
                ErrorManager.AddNewError(
                    "Patch - Failed to start process", 
                    "Something stopped the patching process from starting, please see other errors for more informations."
                    );
                return false;
            }

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (this.InstructionSetting[optimize] == true.ToString())
            {
                IOManager.RestoreApworlds(apLauncher);
            }

            if (output.Contains("Patch created at"))
            {
                // Success : Patch created at path\to\patch with metadata: {'server': '', 'player': '', 'player_name': ''}"
                string patchedRom = output.Split("Patch created at ")[1].Split(" with metadata:")[0];
                customLauncher.SetSlotSetting(rom, patchedRom);
                return true;
            }
            else
            {
                ErrorManager.AddNewError(
                    "Patch - Failed to apply patch",
                    "Trying to apply " + this.settings[patch] + " failed. Here's the process's output :\n\n" + output
                    );
                return false;
            }
        }

        private bool CopyPatch()
        {
            Cache_PreviousSlot cache = new Cache_PreviousSlot();
            cache.previousPatch = this.settings[patch];
            cache.previousAsync = this.settings[AsyncSettings.asyncName];
            cache.previousSlot = this.settings[slotName];
            cache.previousVersion = this.settings[version];

            IOManager.UpdatePatch(this.settings[launcherName], this.InstructionSetting[target], cache);

            return true;
        }
    }
}
