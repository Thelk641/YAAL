using YAAL;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System;
using System.Linq;
using static YAAL.LauncherSettings;
using static YAAL.OpenSettings;

public class Open : Instruction<OpenSettings>
{

    public Open()
    {
        instructionType = "Open";
    }
    public override bool Execute()
    {
        if (this.InstructionSetting[OpenSettings.path] == "")
        {
            ErrorManager.AddNewError(
                "Open - Target not set",
                "The custom launcher contains an Open instruction without a target, this is not allowed."
                );
            return false;
        }

        

        List<string> splitPath = customLauncher.SplitAndParse(this.InstructionSetting[OpenSettings.path]);
        List<string> splitArgs = customLauncher.SplitAndParse(this.InstructionSetting[OpenSettings.args]);
        List<string> splitKeys = customLauncher.SplitAndParse(this.InstructionSetting[OpenSettings.processName]);

        if (splitKeys.Count > 1 && splitKeys.Count != splitPath.Count) {
            ErrorManager.AddNewError(
                "Open - Invalid number of keys",
                "Open was given " + splitKeys.Count + " keys for " + splitPath.Count + " process, this is not allowed. Either pick one key per process, or only one to apply to them all, or none."
                );
            return false;
        }

        Dictionary<string, string> splitInput = new Dictionary<string, string>();
        

        if(splitArgs.Count == 0)
        {
            foreach (var item in splitPath)
            {
                splitInput[item] = "";
            }
        } else
        {
            if(splitPath.Count != splitArgs.Count)
            {
                ErrorManager.AddNewError(
                    "Open - Number of args doesn't match number of targets",
                    "Open was asked to open " + splitPath.Count + " programs or URLs, but was provided " + splitArgs.Count + " arguments. This is not allowed, either pass one per target or include the args in the target."
                    );
                return false;
            }
            for (int i = 0; i < splitPath.Count; i++)
            {
                splitInput[splitPath[i].Trim()] = splitArgs[i].Trim();
            }
        }

        string path = "";
        string args = "";

        int j = 0;

        foreach (var item in splitInput)
        {
            if(item.Key == "")
            {
                continue;
            }
            string parsed = customLauncher.ParseTextWithSettings(item.Key);
            SeparateArgsFromPath(parsed, item.Value, out path, out args);
            if (!(File.Exists(path) || Directory.Exists(path) || WebManager.IsValidURL(path)))
            {
                ErrorManager.AddNewError(
                    "Open - Target doesn't exist",
                    "The custom launcher tried to open file or folder " + path + " but it doesn't appear to exist"
                    );
                return false;
            }
            try
            {

                Cache_Process keyedProcess = ProcessManager.StartKeyedProcess(path, args);
                if (keyedProcess == null)
                {
                    ErrorManager.AddNewError(
                        "Open - Failed to start keyedProcess",
                        "Something went wrong while starting keyedProcess, see other errors for more informations."
                        );
                    return false;
                }

                string key;
                switch (splitKeys.Count)
                {
                    case 0:
                        key = "";
                        break;
                    case 1:
                        key = splitKeys[0];
                        break;
                    default:
                        key = splitKeys[j];
                        ++j;
                        break;
                }

                keyedProcess.Setup(key, customLauncher);
                keyedProcess.Start();
                customLauncher.NoteProcess(keyedProcess);
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                    "Open - Process threw an exception",
                    "Trying to open " + path + " raised the following exception : " + e.Message
                    );
                return false;
            }
        }
        return true;
    }

    private void SeparateArgsFromPath(string settingPath, string settingArgs, out string path, out string args)
    {
        if (File.Exists(settingPath))
        {
            path = settingPath;
            if (settingArgs != "\"\"")
            {
                args = settingArgs;
            } else
            {
                args = "";
            }
            return;
        }

        string[] cutPath;

        if (settingPath.StartsWith("\"") || settingPath.StartsWith("\\\""))
        {
            cutPath = settingPath.Split("\"");
            path = cutPath[0] + cutPath[1];
            args = "";

            for (int i = 2; i < cutPath.Length; i++)
            {
                args += cutPath[i] + " ";
            }

            args = args.Trim();

            if (cutPath[2].Trim() == "")
            {
                args = "\"" + args;
            }

            if (cutPath[cutPath.Length - 1] == "")
            {
                args += "\"";
            }
        } else
        {
            cutPath = settingPath.Trim('"').Split(" ");
            path = cutPath[0];
            args = "";
            for (int i = 1; i < cutPath.Length; i++)
            {
                if (File.Exists(path))
                {
                    args += cutPath[i];
                    for (int j = i + 1; j < cutPath.Length; j++)
                    {
                        args += " " + cutPath[j];
                    }
                    break;
                }
                path += " " + cutPath[i];
            }
        }

        if(settingArgs != "\"\"")
        {
            args += settingArgs;
        }
        customLauncher.ParseTextWithSettings(args);
        path = path.Trim('"').Trim();
    }
}
