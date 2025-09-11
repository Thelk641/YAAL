using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.FileSettings;
using static YAAL.GeneralSettings;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace YAAL
{
    public class Cache_UserSettings
    {
        //public Dictionary<string, string> settings = new Dictionary<string, string>();

        public Dictionary<FileSettings, string> saveLocation = new Dictionary<FileSettings, string>();
        public Dictionary<GeneralSettings, string> generalSettings = new Dictionary<GeneralSettings, string>();
        public Dictionary<string, string> customSettings = new Dictionary<string, string>();
        public Cache_Theme generalTheme = new Cache_Theme() { name = "General Theme" };

        public void SetDefaultSettings()
        {
            if (generalSettings.ContainsKey(backgroundColor))
            {
                return;
            }

            generalSettings[backgroundColor] = "#000000";
            generalSettings[foregroundColor] = "#454545";
            generalSettings[dropdownColor] = "#1D1D1DFF";
            generalSettings[buttonColor] = "#5A5A5AFF";
        }

        public void SetDefaultPath()
        {
            if (saveLocation.ContainsKey(ManagedApworlds))
            {
                //We've just read User Settings, no need for defaults
                return;
            }

            Set(aplauncher, "C:\\ProgramData\\Archipelago\\ArchipelagoLauncher.exe");
            Set("bizhawk", "I:\\Emulators\\Bizhawk\\EmuHawk.exe");

            // uppercase, folders
            saveLocation[ManagedApworlds] = ManagedApworlds.GetFolderName();
            saveLocation[MinimumWorlds] = Path.Combine(saveLocation[ManagedApworlds], MinimumWorlds.ToString());
            saveLocation[Async] = Async.GetFolderName();
            saveLocation[Trash] = Trash.GetFolderName();
            saveLocation[Logs] = Logs.GetFolderName();
            saveLocation[Themes] = Themes.GetFolderName();
            saveLocation[Images] = Path.Combine(saveLocation[Themes], Images.ToString());
            saveLocation[Rendered] = Path.Combine(saveLocation[Themes], Rendered.ToString());

            // lowercase, files
            saveLocation[cache_download] = Path.Combine(saveLocation[ManagedApworlds], cache_download.GetFileName());
            saveLocation[backupList] = Path.Combine(saveLocation[ManagedApworlds], backupList.GetFileName());
            saveLocation[tools] = Path.Combine(saveLocation[ManagedApworlds], tools.GetFileName());
            saveLocation[launcherList] = "./" + launcherList.GetFileName();
            saveLocation[userSettings] = "./" + userSettings.GetFileName();

            Directory.CreateDirectory(IOManager.ProcessLocalPath(saveLocation[ManagedApworlds]));
            Directory.CreateDirectory(IOManager.ProcessLocalPath(saveLocation[Async]));
            Directory.CreateDirectory(IOManager.ProcessLocalPath(saveLocation[Trash]));
            Directory.CreateDirectory(IOManager.ProcessLocalPath(saveLocation[Logs]));
            Directory.CreateDirectory(IOManager.ProcessLocalPath(saveLocation[Themes]));
            Directory.CreateDirectory(IOManager.ProcessLocalPath(saveLocation[Images]));
            Directory.CreateDirectory(IOManager.ProcessLocalPath(saveLocation[Rendered]));
        }

        public string? this[string key]
        {
            get => Get(key);
            set => Set(key, value!);
        }

        public string? this[GeneralSettings key]
        {
            get => Get(key);
            set => Set(key, value!);
        }

        public void Set(string key, string value)
        {
            if(Enum.TryParse(key, out GeneralSettings setting))
            {
                Set(setting, value);
            } else
            {
                customSettings[key] = value;
            } 
        }

        public void Set(GeneralSettings key, string value)
        {
            generalSettings[key] = value;
        }

        public string Get(string key)
        {
            if (Enum.TryParse(key, out GeneralSettings setting))
            {
                return generalSettings[setting];
            }
            return customSettings[key];
        }

        public string Get(GeneralSettings key)
        {
            return generalSettings[key];
        }

        public Dictionary<string, string> GetSettings()
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (var item in generalSettings)
            {
                output[item.Key.ToString()] = item.Value;
            }
            foreach (var item in customSettings)
            {
                output[item.Key] = item.Value;
            }
            return output;
        }
    }
}