using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using static YAAL.LauncherSettings;
using static YAAL.RegExSettings;
using System.IO;
using System.Diagnostics;

namespace YAAL
{
    public class RegEx : Instruction<RegExSettings>
    {
        // default regex : localhost|archipelago\.gg:\d+
        public RegEx() 
        {
            instructionType = "RegEx";
        }
        public override bool Execute()
        {
            if (this.InstructionSetting[regex] == "")
            {
                ErrorManager.AddNewError(
                    "RegEx - No expression",
                    "RegEx doesn't have a pattern to look for."
                    );
                return false;
            }

            if (this.InstructionSetting[replacement] == "")
            {
                ErrorManager.AddNewError(
                    "RegEx - No replacement",
                    "RegEx doesn't have anything to replace the pattern with. It's very likely that the user didn't intend to do that, so it's not allowed just in case."
                    );
                return false;
            }

            if (this.InstructionSetting[modeInput] == "File")
            {
                if (this.InstructionSetting[targetFile] == "")
                {
                    ErrorManager.AddNewError(
                        "RegEx - No target",
                        "RegEx doesn't have a target."
                        );
                    return false;
                }

                if (!File.Exists(this.InstructionSetting[targetFile]))
                {
                    ErrorManager.AddNewError(
                        "RegEx - File doesn't exists",
                        "Target file " + this.InstructionSetting[targetFile] + " doesn't exists."
                        );
                    return false;
                }
                return ApplyRegex(IOManager.LoadFile(this.InstructionSetting[targetFile]));
            } else
            {
                if (this.InstructionSetting[targetString] == "")
                {
                    ErrorManager.AddNewError(
                        "RegEx - No target",
                        "RegEx doesn't have a target."
                        );
                    return false;
                }

                string input = customLauncher.ParseTextWithSettings(this.InstructionSetting[targetString]);
                return ApplyRegex(input);
            }
        }

        private bool ApplyRegex(string target)
        {
            try
            {
                Regex expression = new Regex(this.InstructionSetting[regex]);
                string output = expression.Replace(target, this.InstructionSetting[replacement]);

                if (this.InstructionSetting[modeOutput] == "File")
                {
                    return IOManager.SaveFile(this.InstructionSetting[outputFile], output);
                }
                else
                {
                    customLauncher.settings[this.InstructionSetting[outputVar]] = output;
                    return true;
                }
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                    "RegEx - Exception thrown",
                    "Trying to apply RegEx triggered the following exception : " + e.Message
                    );
                return false;
            }
        }
    }
}
