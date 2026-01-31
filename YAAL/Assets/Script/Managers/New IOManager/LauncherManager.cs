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
using YAAL.Assets.Script.Cache;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static YAAL.FileSettings;
using static YAAL.GeneralSettings;
using static YAAL.LauncherSettings;

namespace YAAL
{

    public static class LauncherManager
    {
        public static Dictionary<string, Cache_CustomLauncher> libraryCustomLauncher = new Dictionary<string, Cache_CustomLauncher>();

        public static string FindAvailableLauncherName(string gameName)
        {
            return IO_Tools.FindAvailableDirectoryName(SettingsManager.GetSaveLocation(ManagedApworlds), gameName);
        }

        public static void SaveLauncher(Cache_CustomLauncher toSave)
        {
            if (toSave.settings[launcherName] == null || toSave.settings[launcherName] == "")
            {
                ErrorManager.ThrowError(
                    "IOManager_Cache - Tried to save empty launcher",
                    "For some reason, SaveLauncher() was asked to save a launcher without a name. This is not allowed."
                    );
                return;
            }
            string launcherDirectory = SettingsManager.GetSaveLocation(ManagedApworlds);
            Directory.CreateDirectory(launcherDirectory);
            string savePath = Path.Combine(launcherDirectory, toSave.settings[launcherName], "launcher.json");
            string json = JsonConvert.SerializeObject(toSave, Formatting.Indented);
            FileManager.SaveFile(savePath, json);
            libraryCustomLauncher[toSave.settings[launcherName]] = toSave;
        }

        public static Cache_CustomLauncher LoadLauncher(string launcherName)
        {
            if (libraryCustomLauncher.ContainsKey(launcherName))
            {
                return libraryCustomLauncher[launcherName];
            }

            string savePath = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), launcherName, "launcher.json");
            if (!File.Exists(savePath))
            {
                return new Cache_CustomLauncher();
            }

            string json = File.ReadAllText(savePath);
            Trace.WriteLine("Loading launcher : " + launcherName);
            Cache_CustomLauncher output = null;
            try
            {
                output = JsonConvert.DeserializeObject<Cache_CustomLauncher>(json) ?? new Cache_CustomLauncher();
            }
            catch (Exception e)
            {
                ErrorManager.ThrowError(
                    "IOManager_Cache - Exception while loading a launcher",
                    "Trying to load launcher " + launcherName + " threw the following exception : " + e.Message
                    );
                Environment.Exit(0);
            }
            libraryCustomLauncher[launcherName] = output;
            return output;
        }

        public static bool UpdatePatch(string launcherName, string path, Cache_PreviousSlot newSlot)
        {
            string cachePath = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), launcherName, previous_async.GetFileName());
            Cache_PreviousSlot oldSlot = CacheManager.LoadCache<Cache_PreviousSlot>(cachePath);

            if (oldSlot.previousAsync == newSlot.previousAsync
                && oldSlot.previousSlot == newSlot.previousSlot
                && oldSlot.previousPatch == newSlot.previousPatch)
            {
                // We've started the same slot as last time, nothing else to do
                return true;
            }

            if (oldSlot.previousPatch != null && oldSlot.previousPatch != "")
            {
                string oldPatch = Path.Combine(path, IO_Tools.GetFileName(oldSlot.previousPatch));

                if (File.Exists(oldPatch))
                {
                    FileManager.HardDeleteFile(oldPatch);
                }
            }

            if (FileManager.CopyFile(newSlot.previousPatch, Path.Combine(path, IO_Tools.GetFileName(newSlot.previousPatch))))
            {
                return true;
            }
            else
            {
                ErrorManager.AddNewError(
                    "IOManager_FileExtra - Failed to update patch",
                    "Failed to copy " + newSlot.previousPatch + " to " + cachePath
                    );
                return false;
            }
        }
    }
}