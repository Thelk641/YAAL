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

            // TODO : these are debug, they need to be replaced by the comments bellow
            string baseFolder = "D:\\Unity\\Avalonia port\\YAAL\\";
            //string baseFolder = AppContext.BaseDirectory;
            Set(aplauncher, "C:\\ProgramData\\Archipelago\\ArchipelagoLauncher.exe");
            //Set(aplauncher, "");
            Set("bizhawk", "I:\\Emulators\\Bizhawk\\EmuHawk.exe");

            // uppercase, folders
            saveLocation[ManagedApworlds] = Path.Combine(baseFolder, ManagedApworlds.GetFolderName());
            saveLocation[Async] = Path.Combine(baseFolder, Async.GetFolderName());
            saveLocation[Trash] = Path.Combine(baseFolder, Trash.GetFolderName());
            saveLocation[Logs] = Path.Combine(baseFolder, Logs.GetFolderName());

            // lowercase, files
            saveLocation[cache_download] = Path.Combine(saveLocation[ManagedApworlds], cache_download.GetFileName());
            saveLocation[userSettings] = Path.Combine(baseFolder, userSettings.GetFileName());
            saveLocation[backupList] = Path.Combine(saveLocation[ManagedApworlds], backupList.GetFileName());
            saveLocation[tools] = Path.Combine(saveLocation[ManagedApworlds], tools.GetFileName());
            

            IOManager.SaveCache<Cache_UserSettings>(saveLocation[userSettings], this);

            Directory.CreateDirectory(saveLocation[ManagedApworlds]);
            Directory.CreateDirectory(saveLocation[Async]);
            Directory.CreateDirectory(saveLocation[Trash]);
            Directory.CreateDirectory(saveLocation[Logs]);
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