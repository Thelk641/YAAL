using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static YAAL.HardcodedSettings;

namespace YAAL
{
    public static class Templates
    {
        public static Dictionary<string, Type> commandTemplates = new Dictionary<string, Type>()
        {
            {"Apworld", typeof(Command_Apworld)},
            {"Backup", typeof(Command_Backup) },
            {"Display", typeof(Command_Display) },
            {"Isolate", typeof(Command_Isolate) },
            {"Input", typeof(Command_Input) },
            {"Open", typeof(Command_Open) },
            {"Patch", typeof(Command_Patch) },
            {"RegEx", typeof(Command_RegEx) },
            {"Wait", typeof(Command_Wait) },
        };

        public static Dictionary<string, Type> instructionsTemplates = new Dictionary<string, Type>()
        {
            {"Apworld", typeof(Apworld)},
            {"Backup", typeof(Backup) },
            {"Display", typeof(Display) },
            {"Isolate", typeof(Isolate) },
            {"Input", typeof(Input) },
            {"Open", typeof(Open) },
            {"Patch", typeof(Patch) },
            {"RegEx", typeof(RegEx) },
            {"Wait", typeof(Wait) },
        };

        public static List<String> commandNames = new List<String>()
        {
            "Apworld",
            "Backup",
            "Display",
            "Isolate",
            "Input",
            "Open",
            "Patch",
            "RegEx",
            "Wait"
        };

        public static Dictionary<string, string> fixedSettings = new Dictionary<string, string>
        {
            {"previous_async", "Per game" },
            {"previous_slot", "Per game" },
            {"launcherName", "Per launcher" },
            {"asyncName", "Per async" },
            {"room", "Per async" },
            {"roomAddress", "Per async" },
            {"roomPort", "Per async" },
            {"password", "Per async" },
            {"slotName", "Per slot"},
            {"slotInfo", "slotName:password@room"},
            {"patch", "Per slot" },
            {"rom", "Per slot" },
            {"version", "Per slot" },
            {"game", "Per slot" },
            {"apworld", "Set at runtime" }
        };

        public static Dictionary<HardcodedSettings, string> hardcodedSettings = new Dictionary<HardcodedSettings, string>
        {
            {previous_async, "Per game" },
            {previous_slot, "Per game" },
            {launcherName, "Per launcher" },
            {apworld, "Set at runtime" },
            {asyncName, "Per async" },
            {room, "Per async" },
            {roomAddress, "Per async" },
            {roomPort, "Per async" },
            {password, "Per async" },
            {slotName, "Per slot"},
            {connect, "--connect slotName:password@roomIP:roomPort"},
            {slotInfo, "slotName:password@roomIP:roomPort"},
            {patch, "Per slot" },
            {rom, "Per slot" },
            {version, "Per slot" },
            {apfolder, "Set automatically" },
            {lua_adventure, "Set automatically" },
            {lua_bizhawk, "Set automatically" },
            {lua_ladx, "Set automatically" },
            {lua_mmbn3, "Set automatically" },
            {lua_oot, "Set automatically" },
            {lua_sni, "Set automatically" },
            {lua_tolz, "Set automatically" },
        };

        public static List<string> defaultSettings = new List<string>
        {
            "aplauncher",
            "gameName",
            "githubURL",
            "filters",
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
