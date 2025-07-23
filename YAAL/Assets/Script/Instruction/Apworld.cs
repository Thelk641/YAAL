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
                launchername = baseSettings[launcherName];
                version = baseSettings[SlotSettings.version];
            }

            target = customLauncher.ParseTextWithSettings(target);

            if (target.Contains(";"))
            {
                foreach (var item in IOManager.SplitPathList(target))
                {
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
            } else
            {
                bool success = IOManager.UpdateFileToVersion(
                    this.InstructionSetting[apworldTarget], 
                    this.settings[launcherName], 
                    this.settings[SlotSettings.version], 
                    this.InstructionSetting[necessaryFile]
                    );
                if(!success)
                {
                    ErrorManager.AddNewError(
                        "Apworld - Failed to update file",
                        "Couldn't update all the necessary files and had to abort."
                        );
                }
                return success;
            }
        }

        public string GetTarget()
        {
            return this.InstructionSetting[apworldTarget];
        }
    }
}
