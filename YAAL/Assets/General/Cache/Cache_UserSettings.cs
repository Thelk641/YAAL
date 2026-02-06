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
            // TODO : this is dbeug
#if DEBUG
            Set(aplauncher, "C:\\ProgramData\\Archipelago\\ArchipelagoLauncher.exe");
            Set("bizhawk", "I:\\Emulators\\Bizhawk\\EmuHawk.exe");
#else
            Set(aplauncher, "/path/to/ArchipelagoLauncher.exe");
#endif

            // uppercase, folders
            saveLocation[ManagedApworlds] = ManagedApworlds.GetFolderName();
            saveLocation[MinimumWorlds] = Path.Combine(saveLocation[ManagedApworlds], MinimumWorlds.ToString());
            saveLocation[Async] = Async.GetFolderName();
            saveLocation[Trash] = Trash.GetFolderName();
            saveLocation[Logs] = Logs.GetFolderName();
            saveLocation[Themes] = Themes.GetFolderName();
            saveLocation[FileSettings.Assets] = FileSettings.Assets.GetFolderName();
            saveLocation[Images] = Path.Combine(saveLocation[FileSettings.Assets], Images.ToString());
            saveLocation[Rendered] = Path.Combine(saveLocation[FileSettings.Assets], Rendered.ToString());

            // lowercase, files
            saveLocation[versions] = Path.Combine(saveLocation[ManagedApworlds], versions.GetFileName());
            saveLocation[backupList] = Path.Combine(saveLocation[ManagedApworlds], backupList.GetFileName());
            saveLocation[tools] = Path.Combine(saveLocation[ManagedApworlds], tools.GetFileName());
            saveLocation[launcherList] = "./" + launcherList.GetFileName();
            saveLocation[userSettings] = "./" + userSettings.GetFileName();
            saveLocation[windows] = "./" + windows.GetFileName();

            Directory.CreateDirectory(IO_Tools.ProcessLocalPath(saveLocation[ManagedApworlds]));
            Directory.CreateDirectory(IO_Tools.ProcessLocalPath(saveLocation[Async]));
            Directory.CreateDirectory(IO_Tools.ProcessLocalPath(saveLocation[Trash]));
            Directory.CreateDirectory(IO_Tools.ProcessLocalPath(saveLocation[Logs]));
            Directory.CreateDirectory(IO_Tools.ProcessLocalPath(saveLocation[Themes]));
            Directory.CreateDirectory(IO_Tools.ProcessLocalPath(saveLocation[Images]));
            Directory.CreateDirectory(IO_Tools.ProcessLocalPath(saveLocation[Rendered]));

            Cache_Tools cacheTools = new Cache_Tools();
            List<Cache_CustomLauncher> list = new List<Cache_CustomLauncher>();

            list.Add(DefaultManager.GetDefaultLauncher(DefaultTools.cheeseTracker));
            list.Add(DefaultManager.GetDefaultLauncher(DefaultTools.textClient));
            list.Add(DefaultManager.GetDefaultLauncher(DefaultTools.webTracker));

            string fulldir = IO_Tools.ProcessLocalPath(saveLocation[ManagedApworlds]);

            foreach (var item in list)
            {
                LauncherManager.SaveLauncher(item);

                DirectoryInfo trueDir = new DirectoryInfo(Path.Combine(fulldir, item.settings[LauncherSettings.launcherName]));
                trueDir.Attributes |= FileAttributes.Hidden;

                Cache_DisplayLauncher display = new Cache_DisplayLauncher();
                display.cache = item;
                display.name = item.settings[LauncherSettings.launcherName];

                cacheTools.defaultTools.Add(display);
            }

            CacheManager.SaveCache<Cache_Tools>(IO_Tools.ProcessLocalPath(saveLocation[tools]), cacheTools);
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