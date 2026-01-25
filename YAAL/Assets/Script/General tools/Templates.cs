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
        static Templates()
        {
            foreach (var item in commandInstruction)
            {
                commandNames.Add(item.Key.Name);
            }
        }

        public static Dictionary<Type, Type> commandInstruction = new Dictionary<Type, Type>()
        {
            { typeof(Apworld), typeof(Command_Apworld) },
            { typeof(Backup), typeof(Command_Backup) },
            { typeof(Display), typeof(Command_Display) },
            { typeof(Isolate), typeof(Command_Isolate) },
            { typeof(Input), typeof(Command_Input) },
            { typeof(Open), typeof(Command_Open) },
            { typeof(Patch), typeof(Command_Patch) },
            { typeof(RegEx), typeof(Command_RegEx) },
            { typeof(Wait), typeof(Command_Wait) },
        };

        public static List<String> commandNames = new List<String>();

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

        public static Type? GetCommand(string key)
        {
            foreach (var item in commandInstruction)
            {
                if (item.Key.Name == key)
                {
                    return item.Key;
                }
            }

            ErrorManager.ThrowError(
                "Templates - Command doesn't exists",
                "Function 'GetCommandTypeFromKey' got called with argument " + key + " which doesn't exist in commandInstruction. Please report this issue."
            );
            return null;
        }

        public static Type? GetInstruction(string key)
        {
            foreach (var item in commandInstruction)
            {
                if (item.Key.Name == key)
                {
                    return item.Key;
                }
            }

            ErrorManager.ThrowError(
                "Templates - Instruction doesn't exists",
                "Function 'GetInstruction' got called with argument " + key + " which doesn't exist in commandInstruction. Please report this issue."
            );
            return null;
        }

        public static Type? GetCommandWithEnum(string key)
        {
            Type commandType = GetCommand(key);
            if(commandType != null)
            {
                Type enumType = commandType.BaseType!.GetGenericArguments()[0];
                Type toAdd = typeof(CommandSetting<>).MakeGenericType(enumType);
                return toAdd;
            }
            return null;
        }

        public static Type? GetInstructionWithEnum(string key)
        {
            Type commandType = GetInstruction(key);
            if (commandType != null)
            {
                Type enumType = commandType.BaseType!.GetGenericArguments()[0];
                Type toAdd = typeof(Instruction<>).MakeGenericType(enumType);
                return toAdd;
            }
            return null;
        }
    }
}
