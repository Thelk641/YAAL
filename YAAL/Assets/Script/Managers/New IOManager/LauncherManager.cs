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
using System.Threading.Tasks;
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
        public static List<Cache_DisplayLauncher> libraryTools = new List<Cache_DisplayLauncher>();
        public static event Action<string>? UpdatedLauncher;

        // obsolete, to be redone when Game are a thing
        // still used in GameManager
        // still used in SlotHolder
        public static Cache_LauncherList launcherList = new Cache_LauncherList(); 

        public static void DeleteLauncher(string gameName)
        {
            FileManager.SoftDeleteFile(Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), gameName));
        }

        public static string FindAvailableLauncherName(string gameName)
        {
            return IO_Tools.FindAvailableDirectoryName(SettingsManager.GetSaveLocation(ManagedApworlds), gameName);
        }

        public static void GenerateLauncherList()
        {
            string path = SettingsManager.GetSaveLocation(FileSettings.launcherList);
            if (File.Exists(path))
            {
                FileManager.HardDeleteFile(path);
            }
            Cache_LauncherList newList = new Cache_LauncherList();
            foreach (var item in GetLauncherList())
            {
                Cache_CustomLauncher cache = LoadLauncher(item);
                newList.list[item] = cache.settings[LauncherSettings.gameName];
            }
            launcherList = newList;
        }

        public static List<string> GetLauncherList(bool includeHidden = false)
        {
            List<string> output = new List<string>();
            string managedApworlds = SettingsManager.GetSaveLocation(ManagedApworlds);
            Directory.CreateDirectory(managedApworlds);

            foreach (var item in Directory.GetDirectories(managedApworlds))
            {
                DirectoryInfo dir = new DirectoryInfo(item);

                if ((dir.Attributes & FileAttributes.Hidden) != 0 && !includeHidden)
                {
                    continue;
                }

                if (File.Exists(Path.Combine(item, launcher.GetFileName())))
                {
                    output.Add(dir.Name);
                }
            }

            return output;
        }

        public static Cache_PreviousSlot GetLastAsync(string launcherName)
        {
            string path = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), launcherName, previous_async.GetFileName());
            if (File.Exists(path))
            {
                return CacheManager.LoadCache<Cache_PreviousSlot>(path);
            }
            return new Cache_PreviousSlot();
        }

        public async static Task<List<Cache_DisplayLauncher>> GetToolList()
        {
            if (libraryTools.Count == 0)
            {
                await UpdateToolList();
            }
            return libraryTools;
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
                    "LauncherManager - Exception while loading a launcher",
                    "Trying to load launcher " + launcherName + " threw the following exception : " + e.Message
                    );
                Environment.Exit(0);
            }
            libraryCustomLauncher[launcherName] = output;
            return output;
        }

        public static string RenameLauncher(Cache_DisplayLauncher cache, string newName)
        {
            string trueName = FindAvailableLauncherName(newName);
            string oldPath = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), cache.name);
            string newPath = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), trueName);
            if (FileManager.MoveFile(oldPath, newPath))
            {
                cache.cache.settings[LauncherSettings.launcherName] = trueName;
                SaveLauncher(cache.cache);
                return trueName;
            }
            return cache.name;
        }

        public static void SaveLauncher(Cache_CustomLauncher toSave)
        {
            if (toSave.settings[launcherName] == null || toSave.settings[launcherName] == "")
            {
                ErrorManager.ThrowError(
                    "LauncherManager - Tried to save empty launcher",
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

        public static void UpdateLastAsync(string gameName, Cache_PreviousSlot cache)
        {
            string basePath = SettingsManager.GetSaveLocation(ManagedApworlds);
            foreach (var item in Directory.GetDirectories(basePath))
            {
                DirectoryInfo dir = new DirectoryInfo(item);
                string path = Path.Combine(item, launcher.GetFileName());
                if (File.Exists(path))
                {
                    Cache_CustomLauncher launcher = CacheManager.LoadCache<Cache_CustomLauncher>(path);
                    if (launcher.settings.TryGetValue(LauncherSettings.gameName, out var foundName) && foundName == gameName)
                    {
                        string toSave = Path.Combine(item, previous_async.GetFileName());
                        CacheManager.SaveCache<Cache_PreviousSlot>(toSave, cache);
                    }
                }
            }
        }

        public static void UpdateLauncherList()
        {
            launcherList = new Cache_LauncherList();
            var list = GetLauncherList();
            if (list.Count > 0)
            {
                foreach (var item in GetLauncherList())
                {
                    Cache_CustomLauncher cache = LoadLauncher(item);
                    launcherList.list[item] = cache.settings[LauncherSettings.gameName];
                }
            }

            GameManager.ReadGameList();
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
                    "LauncherManager - Failed to update patch",
                    "Failed to copy " + newSlot.previousPatch + " to " + cachePath
                    );
                return false;
            }
        }

        public async static Task UpdateToolList()
        {
            List<Cache_DisplayLauncher> output = new List<Cache_DisplayLauncher>();
            Cache_Tools cache = CacheManager.LoadCache<Cache_Tools>(SettingsManager.GetSaveLocation(tools));

            if (cache.customTools.Count > 0)
            {
                foreach (var item in cache.customTools)
                {
                    output.Add(item);
                }
                Cache_DisplayLauncher header = new Cache_DisplayLauncher();
                header.name = "-- Default Tools";
                header.isHeader = true;
                output.Add(header);
            }

            foreach (var item in cache.defaultTools)
            {
                output.Add(item);
            }

            libraryTools = output;
            if (WindowManager.GetMainWindow() is MainWindow window)
            {
                window.UpdateToolList();
            }
            else
            {
                var tcs = new TaskCompletionSource();

                void Handler()
                {
                    WindowManager.DoneStarting -= Handler;
                    tcs.TrySetResult();
                }

                WindowManager.DoneStarting += Handler;

                await tcs.Task;

                WindowManager.GetMainWindow()!.UpdateToolList();
            }
        }
    }
}