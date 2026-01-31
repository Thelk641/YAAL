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
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YAAL;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static YAAL.FileSettings;
using static YAAL.GeneralSettings;

namespace YAAL
{

    public static class CacheManager
    {
        public static T LoadCache<T>(string path) where T : new()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (!File.Exists(path))
            {
                return DefaultManager.GetDefault<T>();
            }

            string json = FileManager.LoadFile(path);
            T output;
            try
            {
                output = JsonConvert.DeserializeObject<T>(json) ?? new T();
                return output;
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                    "IOManager_Cache - Failed to Load Cache",
                    "Trying to load cache " + path + " lead to the following exception : " + e.Message
                    );
                ErrorManager.ThrowError();
                Environment.Exit(1);
            }

            return new T();
        }

        public static void SaveCache<T>(string path, T cache)
        {
            string json = JsonConvert.SerializeObject(cache, Formatting.Indented);
            FileManager.SaveFile(path, json);
        }

        public static Cache_Windows GetWindowSettings()
        {
            return LoadCache<Cache_Windows>(SettingsManager.GetSaveLocation(windows));
        }

        public static void SetWindowSettings(Cache_Windows newSettings)
        {
            SaveCache<Cache_Windows>(SettingsManager.GetSaveLocation(windows), newSettings);
        }
    }
}