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

        string path = "";
        string args = "";

        SeparateArgsFromPath(out path, out args);


        if (!(File.Exists(path) || Directory.Exists(path) || WebManager.IsValidURL(path))) {
            ErrorManager.AddNewError(
                "Open - Target doesn't exist",
                "The custom launcher tried to open file or folder " + path + " but it doesn't appear to exist"
                );
            return false;
        }

        try
        {

            Cache_Process keyedProcess = ProcessManager.StartKeyedProcess(path, args);
            if(keyedProcess == null)
            {
                ErrorManager.AddNewError(
                    "Open - Failed to start keyedProcess",
                    "Something went wrong while starting keyedProcess, see other errors for more informations."
                    );
                return false;
            }

            keyedProcess.Setup(this.InstructionSetting[processName], customLauncher);
            keyedProcess.Start();
            customLauncher.NoteProcess(keyedProcess);
            return true;
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

    private void SeparateArgsFromPath(out string path, out string args)
    {
        string settingPath = customLauncher.ParseTextWithSettings(this.InstructionSetting[OpenSettings.path]);
        if (File.Exists(settingPath))
        {
            path = settingPath;
            args = this.InstructionSetting[OpenSettings.args];
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



        args += this.InstructionSetting[OpenSettings.args];
        customLauncher.ParseTextWithSettings(args);
        path = path.Trim('"').Trim();
    }
}
