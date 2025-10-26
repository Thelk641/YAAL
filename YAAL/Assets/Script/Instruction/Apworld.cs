using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.ApworldSettings;
using static YAAL.LauncherSettings;

namespace YAAL
{
    public class Apworld : Instruction<ApworldSettings>
    {
        public bool readyToRestore = false;
        private List<string> apworlds = new List<string>();

        public Apworld()
        {
            this.InstructionSetting[necessaryFile] = true.ToString();
            instructionType = "Apworld";
        }

        public override bool Execute()
        {
            string target = this.InstructionSetting[apworldTarget];
            string launchername = this.settings[launcherName];
            string version = this.settings[SlotSettings.version];
            if (target.Contains("${base:apworld}"))
            {
                UnifiedSettings baseSettings = customLauncher.GetBaseLauncher().settings;
                Apworld baseApworld = new Apworld();
                baseApworld.customLauncher = customLauncher.GetBaseLauncher();
                foreach (var item in this.InstructionSetting)
                {
                    baseApworld.InstructionSetting[item.Key] = item.Value;
                }
                baseApworld.settings = this.settings.Clone() as UnifiedSettings;
                baseApworld.settings[launcherName] = baseSettings[launcherName];
                baseApworld.settings[SlotSettings.version] = baseSettings[SlotSettings.version];
                baseApworld.InstructionSetting[apworldTarget] = "${apworld}";
                target = target.Replace("${base:apworld}", "");
                if (!baseApworld.Execute())
                {
                    return false;
                }
            }

            apworlds = customLauncher.SplitAndParse(target);

            if (this.InstructionSetting[optimize] == true.ToString())
            {
                if (!this.InstructionSetting.ContainsKey(processName) || this.InstructionSetting[processName] == "")
                {
                    ErrorManager.AddNewError(
                        "Apworld - Failed to find a restore target",
                        "If you use 'optimize apworlds', you must provide a variable name. Without one, YAAL would backup your apworlds, but never restore them, which is probably not your intension. If it is, give it a name not used anywhere else and it'll bypass this error."
                        );
                    return false;
                }

                string apLauncher = customLauncher.ParseTextWithSettings("${aplauncher}");
                if (!IOManager.IsolateApworlds(apLauncher, apworlds))
                {
                    ErrorManager.AddNewError(
                        "Apworld - Failed to isolate apworlds",
                        "Apworld's optimization threw an error. Please see other errors for more information."
                        );
                    IOManager.RestoreApworlds(apLauncher, apworlds);
                    return false;
                }

                if(!customLauncher.AttachToClosing(this, this.InstructionSetting[processName]))
                {
                    ErrorManager.AddNewError(
                        "Apworld - Failed to attach to a process",
                        "Customlauncher failed to parse the variable name. Please see other errors for more information."
                        );
                    IOManager.RestoreApworlds(apLauncher, apworlds);
                    return false;
                }
            }

            switch (apworlds.Count)
            {
                case 0:
                    if (this.InstructionSetting[necessaryFile] == true.ToString())
                    {
                        ErrorManager.AddNewError(
                        "Apworld - Didn't provide an apworld",
                        "An Apworld instruction was set to be necessary, yet no files were listed in it. This is not allowed."
                        );
                        return false;
                    }
                    return true;
                case 1:
                    bool success = IOManager.UpdateFileToVersion(
                    apworlds[0],
                    this.settings[launcherName],
                    this.settings[SlotSettings.version],
                    this.InstructionSetting[necessaryFile]
                    );
                    if (!success)
                    {
                        ErrorManager.AddNewError(
                            "Apworld - Failed to update file",
                            "Couldn't update all the necessary files and had to abort."
                            );
                    }
                    return success;
                default:
                    foreach (var item in apworlds)
                    {
                        if(item == "")
                        {
                            continue;
                        }
                        if (!IOManager.UpdateFileToVersion(item, launchername, version, this.InstructionSetting[necessaryFile]))
                        {
                            ErrorManager.AddNewError(
                            "Apworld - Failed to update a file from a list",
                            "Couldn't update all the necessary files in a given list, had to abort."
                            );
                            return false;
                        }
                    }

                    return true;
            }
        }

        public string GetTarget()
        {
            return this.InstructionSetting[apworldTarget];
        }

        public override void ParseProcess(object? sender, EventArgs e)
        {
            string apLauncher = customLauncher.ParseTextWithSettings("${aplauncher}");
            IOManager.RestoreApworlds(apLauncher, apworlds);
        }
    }
}
