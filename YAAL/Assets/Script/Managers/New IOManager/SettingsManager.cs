using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using YAAL;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static YAAL.FileSettings;
using static YAAL.GeneralSettings;

namespace YAAL
{

    public static class SettingsManager
    {
        private static Cache_UserSettings settings;

        static SettingsManager()
        {
            settings = CacheManager.LoadCache<Cache_UserSettings>(userSettings.GetFullPath());

            if (settings.generalSettings.ContainsKey(zoom) && double.TryParse(settings.generalSettings[zoom], out double newZoom))
            {
                App.Settings.Zoom = newZoom;
            }

            if (settings.saveLocation.Count == 0)
            {
                settings.SetDefaultPath();
                CacheManager.SaveCache<Cache_UserSettings>(userSettings.GetFullPath(), settings);
            }
        }

        public static Cache_Settings GetGeneralSettings()
        {
            Cache_Settings output = new Cache_Settings();

            foreach (var item in settings.GetSettings())
            {
                string key = item.Key;
                string value = item.Value;
                output.settings[item.Key] = item.Value;
            }

            return output;
        }

        public static string GetSaveLocation(FileSettings key)
        {
            if (!settings.saveLocation.ContainsKey(key))
            {
                ErrorManager.ThrowError(
                    "SettingsManager - Save location doesn't exist",
                    "The code tried to get the save location named " + key.ToString() + " but it doesn't seem to exist ?"
                    );
            }

            string path = IO_Tools.ProcessLocalPath(settings.saveLocation[key]);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            return path;
        }

        public static string GetSetting(GeneralSettings name)
        {
            try
            {
                if (settings.generalSettings.ContainsKey(name))
                {
                    return settings[name]!;
                }
                return "";
            }
            catch (Exception)
            {
                Trace.WriteLine(name.ToString());
                throw;
            }
        }
        
        public static Dictionary<GeneralSettings, string> GetUserSettings(out Dictionary<string, string> customSettings)
        {
            Dictionary<GeneralSettings, string> output = new Dictionary<GeneralSettings, string>();
            customSettings = new Dictionary<string, string>();
            foreach (var item in settings.generalSettings)
            {
                output[item.Key] = item.Value;
            }

            foreach (var item in settings.customSettings)
            {
                customSettings[item.Key] = item.Value;
            }

            return output;
        }

        public static void SetSetting(GeneralSettings name, string value)
        {
            settings.generalSettings[name] = value;
        }

        public static void SetUserSettings(Dictionary<GeneralSettings, string> newGeneral, Dictionary<string, string> newCustom)
        {
            foreach (var item in newGeneral)
            {
                settings.generalSettings[item.Key] = item.Value;
            }

            settings.customSettings = new Dictionary<string, string>();

            foreach (var item in newCustom)
            {
                settings.customSettings[item.Key] = item.Value;
            }

            CacheManager.SaveCache<Cache_UserSettings>(Path.Combine(userSettings.GetFullPath()), settings);
            ThemeManager.UpdateGeneralTheme();
        }
    }
}