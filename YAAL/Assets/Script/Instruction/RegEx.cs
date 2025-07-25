using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YAAL.Assets.Scripts;
using static YAAL.LauncherSettings;
using static YAAL.RegExSettings;

namespace YAAL
{
    public class RegEx : Instruction<RegExSettings>
    {
        // default regex : localhost|archipelago\.gg:\d+
        private bool alreadyOutputedVar = false;
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

            string[] splitPattern = this.InstructionSetting[regex].Split("/;");
            string[] splitReplacement = this.InstructionSetting[replacement].Split("/;");

            List<string> cleanedPattern = new List<string>();
            List<string> cleanedReplacement = new List<string>();

            foreach (var item in splitPattern)
            {
                if(item == "")
                {
                    continue;
                }
                cleanedPattern.Add(item.Trim());
            }

            foreach (var item in splitReplacement)
            {
                if (item == "")
                {
                    continue;
                }
                cleanedReplacement.Add(item.Trim());
            }

            if (this.InstructionSetting[modeInput] == "File")
            {
                if (this.InstructionSetting[targetFile] == "")
                {
                    ErrorManager.AddNewError(
                        "RegEx - No input file",
                        "RegEx doesn't have a file to read and modify."
                        );
                    return false;
                }
                if (!ApplyRegex(customLauncher.ParseTextWithSettings(this.InstructionSetting[targetFile]), cleanedPattern, cleanedReplacement))
                {
                    return false;
                }
            } else
            {
                if (this.InstructionSetting[targetString] == "")
                {
                    ErrorManager.AddNewError(
                        "RegEx - No input text",
                        "RegEx doesn't have a string to modify"
                        );
                    return false;
                }
                if (!ApplyRegex(customLauncher.ParseTextWithSettings(this.InstructionSetting[targetString]), cleanedPattern, cleanedReplacement))
                {
                    return false;
                }
            }
            
            return true;
        }

        private bool ApplyRegex(string rawTarget, List<string> cleanedPattern, List<string> cleanedReplacement)
        {
            string[] splitInput = rawTarget.Split("/;");
            List<string> cleanedInput = new List<string>();

            foreach (var item in splitInput)
            {
                if (item == "")
                {
                    continue;
                }
                cleanedInput.Add(item.Trim());
            }

            if (
            !(cleanedPattern.Count == 1 || cleanedPattern.Count == cleanedInput.Count)
            || !(cleanedReplacement.Count == 1 || cleanedPattern.Count == cleanedInput.Count)
            || cleanedPattern.Count > cleanedInput.Count
            || cleanedReplacement.Count > cleanedInput.Count)
            {
                ErrorManager.AddNewError(
                    "RegEx - Invalid number of pattern and replacement",
                    "RegEx was given " + cleanedPattern.Count + " patterns and " + cleanedReplacement.Count + " replacement. These two need to either be one or match the number of input files (" + cleanedInput.Count + ")."
                    );
                return false;
            }

            string target;
            string pattern;
            string replacement;
            string output;

            for (int i = 0; i < cleanedInput.Count; i++)
            {
                target = cleanedInput[i];

                if (cleanedPattern.Count == 1)
                {
                    pattern = cleanedPattern[0];
                }
                else
                {
                    pattern = cleanedPattern[i];
                }

                if (cleanedReplacement.Count == 1)
                {
                    replacement = cleanedReplacement[0];
                }
                else
                {
                    replacement = cleanedReplacement[i];
                }

                try
                {
                    Regex expression = new Regex(pattern);
                    output = "";
                    if (this.InstructionSetting[modeInput] == "File")
                    {
                        output = expression.Replace(IOManager.LoadFile(target), replacement);
                    } else
                    {
                        output = expression.Replace(target, replacement);
                    }



                    if (!SaveResult(output, i))
                    {
                        return false;
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

            return true;
        }

        private bool SaveResult(string result, int i)
        {
            if (this.InstructionSetting[modeOutput] == "File")
            {
                string[] splitOutputFile = this.InstructionSetting[outputFile].Split("/;");
                List<string> cleanedOutputFile = new List<string>();
                foreach (var item in splitOutputFile)
                {
                    if (item == "")
                    {
                        continue;
                    }
                    cleanedOutputFile.Add(item.Trim());
                }

                switch (cleanedOutputFile.Count)
                {
                    case 0:
                        ErrorManager.AddNewError(
                            "RegEx - Missing output file",
                            "RegEx was set to output to a file, but none were set. This is not allowed."
                            );
                        return false;
                    case 1:
                        return IOManager.SaveFile(cleanedOutputFile[0], result);
                    default:
                        if (cleanedOutputFile.Count < i)
                        {
                            ErrorManager.AddNewError(
                                "RegEx - Not enough output file set",
                                "If you set multiple output file, you need to set one per input file. You set " + cleanedOutputFile.Count + " outputs, but this tried to apply RegEx to input number " + i
                                );
                            return false;
                        }
                        return IOManager.SaveFile(cleanedOutputFile[i], result);
                }
            }
            else
            {
                string[] splitOutputVar = this.InstructionSetting[outputVar].Split("/;");
                List<string> cleanedOutputVar = new List<string>();
                foreach (var item in splitOutputVar)
                {
                    if (item == "")
                    {
                        continue;
                    }
                    cleanedOutputVar.Add(item.Trim());
                }

                switch (cleanedOutputVar.Count)
                {
                    case 0:
                        ErrorManager.AddNewError(
                            "RegEx - Missing output file",
                            "RegEx was set to output to a file, but none were set. This is not allowed."
                            );
                        return false;
                    case 1:
                        if (alreadyOutputedVar)
                        {
                            customLauncher.settings[cleanedOutputVar[0]] += ";" + result;
                        }
                        else
                        {
                            customLauncher.settings[cleanedOutputVar[0]] = result;
                            alreadyOutputedVar = true;
                        }
                        return true;
                    default:
                        if (cleanedOutputVar.Count < i)
                        {
                            ErrorManager.AddNewError(
                                "RegEx - Not enough output var set",
                                "If you set multiple output var, you need to set one per input file. You set " + cleanedOutputVar.Count + " var, but this tried to apply RegEx to input number " + i
                                );
                            return false;
                        }
                        customLauncher.settings[cleanedOutputVar[i]] = result;
                        return true;
                }
            }
        }
    }
}
