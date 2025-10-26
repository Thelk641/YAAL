using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.LauncherSettings;
using static YAAL.ApworldSettings;

namespace YAAL
{
    public class Apworld : Instruction<ApworldSettings>
    {
        public bool readyToRestore = false;

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

            //target = customLauncher.ParseTextWithSettings(target);
            List<string> apworlds = customLauncher.SplitAndParse(target);

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
    }
}
