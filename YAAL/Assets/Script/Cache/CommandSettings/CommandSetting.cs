using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;

namespace YAAL
{
    public class CommandSetting<TEnum> : Interface_CommandSetting where TEnum : struct, Enum
    {
        public string commandType { set; get; } = "";
        public Dictionary<TEnum, string> InstructionSetting = new Dictionary<TEnum, string>();

        public void SetDefaultSetting(Dictionary<TEnum, string> defaultValue)
        {
            InstructionSetting = defaultValue;
        }

        public void SetSetting(TEnum key, string value) {
            if (InstructionSetting.ContainsKey(key))
            {
                InstructionSetting[key] = value;
            } else
            {
                ErrorManager.ThrowError(
                    "CommandSetting - Invalid setting key",
                    "Tried to set setting " + key + " on a command of type " + commandType + ". Please report this.");
            }
        }

        public void SetSetting(string key, string value)
        {
            if (Enum.TryParse<TEnum>(key, out TEnum result))
            {
                SetSetting(result, value);
            } else
            {
                ErrorManager.ThrowError(
                    "CommandSetting - Invalid setting key", 
                    "Try to set setting " + key + " but it is not a valid key for a command of type " + commandType
                    );
            }
        }

        public string GetSetting(TEnum key)
        {
            if (InstructionSetting.ContainsKey(key))
            {
                return InstructionSetting[key];
            }
            ErrorManager.ThrowError(
                "CommandSetting - Invalid setting key",
                "Tried to get setting " + key + " on a command of type " + commandType + ". Please report this.");
            return "";
        }

        public void SetCommandType(string newType)
        {
            commandType = newType;
        }

        public string GetCommandType()
        {
            return commandType;
        }
    }

    public interface Interface_CommandSetting
    {
        public void SetCommandType(string newValue);
        public string GetCommandType();
        public void SetSetting(string key, string value); // must only be used by CommandSetting itself, as it's generic
    }
}
