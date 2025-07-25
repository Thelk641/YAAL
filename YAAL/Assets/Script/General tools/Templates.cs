using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YAAL
{
    public static class Templates
    {
        public static Dictionary<string, Type> commandTemplates = new Dictionary<string, Type>()
        {
            {"Apworld", typeof(Command_Apworld)},
            {"Backup", typeof(Command_Backup) },
            {"Isolate", typeof(Command_Isolate) },
            {"Open", typeof(Command_Open) },
            {"Patch", typeof(Command_Patch) },
            {"RegEx", typeof(Command_RegEx) },
        };

        public static Dictionary<string, Type> instructionsTemplates = new Dictionary<string, Type>()
        {
            {"Apworld", typeof(Apworld)},
            {"Backup", typeof(Backup) },
            {"Isolate", typeof(Isolate) },
            {"Open", typeof(Open) },
            {"Patch", typeof(Patch) },
            {"RegEx", typeof(RegEx) },
        };

        public static List<String> commandNames = new List<String>()
        {
            "Apworld",
            "Backup",
            "Isolate",
            "Open",
            "Patch",
            "RegEx",
        };

        public static Dictionary<string, string> fixedSettings = new Dictionary<string, string>
        {
            {"previous_async", "Per game" },
            {"previous_slot", "Per game" },
            {"launcherName", "Per launcher" },
            {"asyncName", "Per async" },
            {"room", "Per async" },
            {"password", "Per async" },
            {"slotName", "Per slot"},
            {"slotInfo", "slotName:password@room"},
            {"patch", "Per slot" },
            {"rom", "Per slot" },
            {"version", "Per slot" },
            {"game", "Per slot" },
            {"apworld", "Set at runtime" }
        };

        public static List<string> defaultSettings = new List<string>
        {
            "aplauncher",
            "gameName",
            "githubURL",
            "filters",
            "renamePatch",
        };

        public static List<string> hiddenSettings = new List<string>
        {
            "Debug_AsyncName",
            "Debug_SlotName",
            "Debug_Patch",
            "Debug_baseLauncher",
        };

        public static Type GetInstructionTypeFromKey(string key)
        {
            foreach (var item in instructionsTemplates)
            {
                if (item.Key == key)
                {
                    return item.Value;
                }
            }

            ErrorManager.ThrowError(
                "Templates - Instruction doesn't exists",
                "Function 'GetInstructionTypeFromKey' got called with argument " + key + " which doesn't exist in instructionsTemplates. Please report this issue."
            );
            return null;
        }

        public static string GetCommandKey(Interface_Instruction instruction)
        {
            foreach (var item in commandTemplates)
            {
                if (item.Value == instruction.GetType())
                {
                    return item.Key;
                }
            }
            
            ErrorManager.ThrowError(
                "Templates - Command doesn't exists",
                "Function 'GetCommandKey' got called with argument " + instruction.GetType().ToString() + " which doesn't exist in commandTemplates. Please report this issue."
                );

            return "";
        }

        public static Type GetCommandTypeFromKey(string key)
        {
            foreach (var item in commandTemplates)
            {
                if (item.Key == key)
                {
                    return item.Value;
                }
            }
            
            ErrorManager.ThrowError(
                "Templates - Command doesn't exists",
                "Function 'GetCommandTypeFromKey' got called with argument " + key + " which doesn't exist in commandTemplates. Please report this issue."
            );
            return null;
        }
    }
}
