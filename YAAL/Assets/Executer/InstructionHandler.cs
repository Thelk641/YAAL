using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YAAL;
using YAAL.Assets.Script.Cache;
using YAAL.Assets.Scripts;
using static YAAL.AsyncSettings;
using static YAAL.LauncherSettings;
using static YAAL.PreviousAsyncSettings;
using static YAAL.SlotSettings;

public class InstructionHandler
{
    public Dictionary<Interface_Instruction, Interface_CommandSetting> listOfInstructions = new Dictionary<Interface_Instruction, Interface_CommandSetting>();
    public SettingsHandler settings;
    public Executer executer;

    public InstructionHandler(Executer newExecuter, SettingsHandler newSettings)
    {
        executer = newExecuter;
        settings = newSettings;
    }

    public bool ReadCache(List<Interface_CommandSetting> list)
    {
        foreach (var item in list)
        {
            string commandName = item.GetCommandType();
            if (Templates.GetInstruction(commandName) is Type instructionType
                    && Activator.CreateInstance(instructionType) is Interface_Instruction instruction)
            {
                instruction.SetSettings(item.GetSettings());
                instruction.SetExecuter(executer);
                listOfInstructions[instruction] = item;
                if (instruction is Apworld apworldInstruction)
                {
                    settings.AddApworld(apworldInstruction.GetTarget());
                }
                else if (instruction is Patch patchInstruction)
                {
                    settings.ForcePatchRequirement();
                }
            }
            else
            {
                ErrorManager.ThrowError(
                    "CustomLauncher - Failed to read cache",
                    "Something went wrong while reading an instruction of type " + commandName
                    );
                return false;
            }
        }

        return true;
    }

    public List<Interface_CommandSetting> WriteCache()
    {
        List<Interface_CommandSetting> output = new List<Interface_CommandSetting>();
        foreach (var item in listOfInstructions)
        {
            output.Add(item.Value);
        }
        return output;
    }

    public void UpdateCache(Interface_Instruction source)
    {
        if (listOfInstructions.ContainsKey(source))
        {
            listOfInstructions[source].SetSettings(source.GetSettings());
            executer.SaveCache();
        }
    }

    public void SetSlotSetting(SlotSettings key, string value)
    {
        IOManager.SetSlotSetting(settings.GetSetting(AsyncSettings.asyncName), settings.GetSetting(SlotSettings.slotLabel), key, value);
        settings.SetSetting(key, value);
    }
}