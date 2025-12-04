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
using System.Globalization;

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

            string newFileName = this.InstructionSetting[rename];

            if (newFileName != "")
            {
                newFileName = customLauncher.ParseTextWithSettings(newFileName);
                if(newFileName == "")
                {
                    ErrorManager.AddNewError(
                        "Patch - Couldn't parse the new file name", 
                        "Something went wrong when parsing the new file name for your patch. Please check other errors for more informations."
                        );
                    return false;
                }
            }

            string patch = IOManager.MoveToSlotDirectory(
                settings[SlotSettings.patch],
                settings[AsyncSettings.asyncName],
                settings[slotName],
                newFileName
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
                if (this.InstructionSetting[target] == "")
                {
                    ErrorManager.AddNewError(
                        "Patch - Empty target",
                        "Patch was asked to copy a patch, but no target was given to it. This is not allowed."
                        );
                    return false;
                }
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
            string folder = IOManager.ToDebug(customLauncher.ParseTextWithSettings("${aplauncher}"));

            
            

            if (!File.Exists(Path.Combine(Path.GetDirectoryName(apLauncher), "custom_worlds", "YAAL.apworld")))
            {
                ErrorManager.AddNewError(
                    "Patch - Missing the YAAL apworld",
                    "For patching, YAAL requires its own apworld, please download it and put it in your custom_worlds folder"
                    );
                return false;
            }

            List<string> apworlds = null;

            if (this.InstructionSetting[optimize] == true.ToString())
            {
                apworlds = customLauncher.GetApworlds();
                bool addedYAAL = false;
                foreach (var item in apworlds)
                {
                    if (item.Contains("YAAL.apworld"))
                    {
                        addedYAAL = true;
                        break;
                    }
                }
                if (!addedYAAL)
                {
                    apworlds.Add("YAAL.apworld");
                }
                if(!IOManager.IsolateApworlds(apLauncher, apworlds))
                {
                    ErrorManager.AddNewError(
                        "Patch - Failed to isolate apworlds",
                        "Patch's optimization threw an error. Please see other errors for more information."
                        );
                    IOManager.RestoreApworlds(apLauncher, apworlds);
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
                IOManager.RestoreApworlds(apLauncher, apworlds);
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

            List<string> splitTarget = customLauncher.SplitString(this.InstructionSetting[target]);

            foreach (var item in splitTarget)
            {
                if(item == "")
                {
                    continue;
                }

                if (!IOManager.UpdatePatch(this.settings[launcherName], item.Trim().Trim('\"'), cache))
                {
                    ErrorManager.AddNewError(
                        "Patch - Updating patch failed",
                        "Something stopped YAAL from updating a patch, please see other errors for more informations."
                        );
                    return false;
                }
            }

            IOManager.UpdateLastAsync(this.settings[gameName], cache);

            return true;
        }
    }
}
