using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace YAAL
{
    public class UnifiedSettings : ICloneable
    {
        private readonly Dictionary<Type, Dictionary<Enum, string>> enumSettings = new();
        private readonly Dictionary<string, string> customSettings = new();

        // Set for Enum keys
        public void Set<TEnum>(TEnum key, string value) where TEnum : Enum
        {
            var type = typeof(TEnum);
            if (!enumSettings.TryGetValue(type, out var dict))
            {
                dict = new Dictionary<Enum, string>();
                enumSettings[type] = dict;
            }

            dict[key] = value;
        }

        // Get for Enum keys
        public string? Get<TEnum>(TEnum key) where TEnum : Enum
        {
            var type = typeof(TEnum);
            if (enumSettings.TryGetValue(type, out var dict) && dict.TryGetValue(key, out var val))
                return val;

            // Optional fallback to string key
            return customSettings.TryGetValue(key.ToString(), out val) ? val : null;
        }

        // Set for string-based custom keys
        public void Set(string key, string value)
        {
            if (Enum.TryParse(key, out AsyncSettings asyncSetting))
            {
                if(asyncSetting == AsyncSettings.password && value == "")
                {
                    Set(asyncSetting, "None");
                } else
                {
                    Set(asyncSetting, value);
                }
                return;
            }

            if (Enum.TryParse(key, out SlotSettings slotSetting))
            {
                Set(slotSetting, value);
                return;
            }

            if(Enum.TryParse(key, out LauncherSettings launcherSetting))
            {
                Set(launcherSetting, value);
                return;
            }

            if (Enum.TryParse(key, out GeneralSettings generalSettings))
            {
                if(key == "aplauncher")
                {
                    try
                    {
                        string folder = Path.GetDirectoryName(value);
                        Set(GeneralSettings.apfolder, folder);
                    }
                    catch { }
                }
                Set(generalSettings, value);
                return;
            }

            customSettings[key] = value;
        }

        // Get for string-based custom keys
        public string? Get(string key)
        {
            if (customSettings.TryGetValue(key, out var val))
            {
                return val;
            }
                

            // Optional fallback: scan enum keys by name
            foreach (var dict in enumSettings.Values)
            {
                if (dict.FirstOrDefault(kvp => kvp.Key.ToString() == key) is { Key: not null, Value: var match })
                    return match;
            }

            return null;
        }

        // Indexer for string and enum keys (convenient syntax)
        public string? this[string key]
        {
            get => Get(key);
            set => Set(key, value!);
        }

        public string? this[Enum key]
        {
            get => GetEnum(key);
            set => SetEnum(key, value!);
        }

        // Internal helpers for enum key indexer (non-generic due to C# limitations)
        private void SetEnum(Enum key, string value)
        {
            var type = key.GetType();
            if (!enumSettings.TryGetValue(type, out var dict))
                enumSettings[type] = dict = new Dictionary<Enum, string>();

            dict[key] = value;
        }

        private string? GetEnum(Enum key)
        {
            var type = key.GetType();
            if (enumSettings.TryGetValue(type, out var dict) && dict.TryGetValue(key, out var val))
                return val;

            return null;
        }

        // Optional: expose raw access if needed
        public bool Has<TEnum>(TEnum key) where TEnum : Enum =>
            enumSettings.TryGetValue(typeof(TEnum), out var dict) && dict.ContainsKey(key);

        public bool Has(string key) {
            if (customSettings.ContainsKey(key))
            {
                return true;
            }

            foreach (var item in enumSettings.Values)
            {
                if(item.Keys.Any(e => e.ToString() == key))
                {
                    return true;
                }
            }

            return false;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
