using DynamicData;
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
        private List<string> alreadyOutputedVar = new List<string>();
        private List<string> alreadyOutputedFile = new List<string>();
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

            List<string> splitPattern = executer.Parser.SplitString(this.InstructionSetting[regex]);
            List<string> splitReplacement = executer.Parser.SplitString(this.InstructionSetting[replacement]);

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
                if (!ApplyRegex(this.InstructionSetting[targetFile], splitPattern, splitReplacement))
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
                if (!ApplyRegex(this.InstructionSetting[targetString], splitPattern, splitReplacement))
                {
                    return false;
                }
            }
            
            return true;
        }

        private bool ApplyRegex(string rawTarget, List<string> cleanedPattern, List<string> cleanedReplacement)
        {
            List<string> splitInput = executer.Parser.SplitAndParse(rawTarget);

            if (
            !(cleanedPattern.Count == 1 || cleanedPattern.Count == splitInput.Count)
            || !(cleanedReplacement.Count == 1 || cleanedPattern.Count == splitInput.Count)
            || cleanedPattern.Count > splitInput.Count
            || cleanedReplacement.Count > splitInput.Count)
            {
                ErrorManager.AddNewError(
                    "RegEx - Invalid number of pattern and replacement",
                    "RegEx was given " + cleanedPattern.Count + " patterns and " + cleanedReplacement.Count + " replacement. These two need to either be one or match the number of input files (" + splitInput.Count + ")."
                    );
                return false;
            }

            string target;
            string pattern;
            string replacement;
            string output;

            alreadyOutputedFile = new List<string>();
            alreadyOutputedVar = new List<string>();

            for (int i = 0; i < splitInput.Count; i++)
            {
                target = splitInput[i];

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
                    string toReplace = executer.Parser.ParseTextWithSettings(replacement);
                    output = "";
                    string input = "";
                    if (this.InstructionSetting[modeInput] == "File")
                    {
                        input = FileManager.LoadFile(target);
                        output = expression.Replace(FileManager.LoadFile(target), toReplace);
                    } else
                    {
                        input = target;
                        output = expression.Replace(target, toReplace);
                    }

                    output = expression.Replace(input, toReplace);
                    
                    if(!expression.IsMatch(input)) 
                    {
                        ErrorManager.AddNewError(
                            "RegEX - Pattern not found",
                            "Pattern " + pattern + " couldn't be found in the target.");
                        return false;
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
                bool success = false;
                List<string> splitOutputFile = executer.Parser.SplitAndParse(this.InstructionSetting[outputFile]);

                switch (splitOutputFile.Count)
                {
                    case 0:
                        ErrorManager.AddNewError(
                            "RegEx - Missing output file",
                            "RegEx was set to output to a file, but none were set. This is not allowed."
                            );
                        return false;
                    case 1:
                        i = 0;
                        break;
                }

                if (splitOutputFile.Count < i)
                {
                    ErrorManager.AddNewError(
                        "RegEx - Not enough output file set",
                        "If you set multiple output file, you need to set one per input file. You set " + splitOutputFile.Count + " outputs, but this tried to apply RegEx to input number " + i
                        );
                    return false;
                }

                if (alreadyOutputedFile.Contains(splitOutputFile[i]))
                {
                    string toSave = FileManager.LoadFile(splitOutputFile[i]) + "; " + result;
                    success = FileManager.SaveFile(splitOutputFile[i], toSave);
                }
                else
                {
                    success = FileManager.SaveFile(splitOutputFile[i], result);
                    alreadyOutputedFile.Add(splitOutputFile[i]);
                }
                return success;
            }
            else
            {
                List<string> splitOutputVar = executer.Parser.SplitString(this.InstructionSetting[outputVar]);

                switch (splitOutputVar.Count)
                {
                    case 0:
                        ErrorManager.AddNewError(
                            "RegEx - Missing output file",
                            "RegEx was set to output to a file, but none were set. This is not allowed."
                            );
                        return false;
                    case 1:
                        i = 0;
                        break;
                }

                if (splitOutputVar.Count < i)
                {
                    ErrorManager.AddNewError(
                        "RegEx - Not enough output var set",
                        "If you set multiple output var, you need to set one per input var. You set " + splitOutputVar.Count + " outputs, but this tried to apply RegEx to input number " + i
                        );
                    return false;
                }

                if (alreadyOutputedVar.Contains(splitOutputVar[i]))
                {
                    if (executer.SettingsHandler.settings.Has(splitOutputVar[i]))
                    {
                        executer.SettingsHandler.settings[splitOutputVar[i]] = executer.SettingsHandler.settings[splitOutputVar[i]] + "; " + result;
                    }
                    else
                    {
                        executer.SettingsHandler.settings[splitOutputVar[i]] = result;
                    }
                }
                else
                {
                    executer.SettingsHandler.settings[splitOutputVar[i]] = result;
                    alreadyOutputedVar.Add(splitOutputVar[i]);
                }
                return true;
            }
        }
    }
}
