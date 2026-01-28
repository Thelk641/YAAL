using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using YAAL;
using YAAL.Assets.Script.Cache;
using static System.Net.Mime.MediaTypeNames;
using static YAAL.AsyncSettings;
using static YAAL.FileSettings;

namespace YAAL
{
    public static partial class IOManager
    {

        public static event Action<string> UpdatedLauncher;
        public static Dictionary<string, Cache_CustomLauncher> libraryCustomLauncher = new Dictionary<string, Cache_CustomLauncher>();
        public static List<Cache_DisplayLauncher> libraryTools = new List<Cache_DisplayLauncher>();

        public static string RenameLauncher(Cache_DisplayLauncher cache, string newName)
        {
            string trueName = FindAvailableLauncherName(newName);
            string oldPath = Path.Combine(GetSaveLocation(ManagedApworlds), cache.name);
            string newPath = Path.Combine(GetSaveLocation(ManagedApworlds), trueName);
            if (MoveFile(oldPath, newPath))
            {
                cache.cache.settings[LauncherSettings.launcherName] = trueName;
                SaveCacheLauncher(cache.cache);
                return trueName;
            }
            return cache.name;
        }

        public static void DeleteLauncher(string gameName)
        {
            SoftDeleteFile(Path.Combine(GetSaveLocation(ManagedApworlds), gameName));
        }

        public static List<string> GetLauncherList(bool includeHidden = false)
        {
            List<string> output = new List<string>();
            string managedApworlds = GetSaveLocation(ManagedApworlds);
            Directory.CreateDirectory(managedApworlds);

            foreach (var item in Directory.GetDirectories(managedApworlds))
            {
                DirectoryInfo dir = new DirectoryInfo(item);

                if((dir.Attributes & FileAttributes.Hidden) != 0 && !includeHidden)
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

        public static List<string> GetGameList()
        {
            if(games.Count == 0)
            {
                ReadGameList();
            }

            return games;
        }

        public static string GetFirstLauncherForGame(string toFind)
        {
            foreach (var item in launcherList.list)
            {
                string gameName = item.Value;
                if (gameName == toFind)
                {
                    return item.Key;
                }
            }

            return "";
        }

        public static List<Cache_DisplayLauncher> GetLaunchersForGame(string toFind)
        {
            List<Cache_DisplayLauncher> output = new List<Cache_DisplayLauncher>();
            List<string> otherGames = new List<string>();

            Cache_DisplayLauncher match = new Cache_DisplayLauncher();
            match.name = "-- Match game";
            match.isHeader = true;
            output.Add(match);


            foreach (var item in launcherList.list)
            {
                string gameName = item.Value;
                if (gameName == toFind)
                {
                    Cache_DisplayLauncher toAdd = new Cache_DisplayLauncher();
                    toAdd.name = item.Key;
                    output.Add(toAdd);
                } else
                {
                    otherGames.Add(item.Key);
                }
            }

            Cache_DisplayLauncher other = new Cache_DisplayLauncher();
            other.name = "-- Other games";
            other.isHeader = true;
            output.Add(other);

            foreach (var item in otherGames)
            {
                Cache_DisplayLauncher toAdd = new Cache_DisplayLauncher();
                toAdd.name = item;
                output.Add(toAdd);
            }

            return output;
        }

        public static void ReadGameList()
        {
            games = new List<string>();

            foreach (var launcher in launcherList.list)
            {
                string gameName = launcher.Value;
                if (!games.Contains(gameName))
                {
                    games.Add(gameName);
                }
            }
        }

        public static void GenerateLauncherList()
        {
            string path = GetSaveLocation(FileSettings.launcherList);
            if (File.Exists(path))
            {
                HardDeleteFile(path);
            }
            Cache_LauncherList newList = new Cache_LauncherList();
            foreach (var item in GetLauncherList())
            {
                Cache_CustomLauncher cache = LoadCacheLauncher(item);
                newList.list[item] = cache.settings[LauncherSettings.gameName];
            }
            launcherList = newList;
        }

        public static void UpdateLauncherList()
        {
            launcherList = new Cache_LauncherList();
            var list = GetLauncherList();
            if(list.Count > 0)
            {
                foreach (var item in GetLauncherList())
                {
                    Cache_CustomLauncher cache = LoadCacheLauncher(item);
                    launcherList.list[item] = cache.settings[LauncherSettings.gameName];
                }
            }
            
            ReadGameList();
        }

        public static string GetLauncherNameFromSlot(string async, string slot)
        {
            return GetSlot(async, slot).settings[SlotSettings.baseLauncher];
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

        public static Cache_Settings GetSettings(string async, string slot)
        {

            Cache_Settings output = new Cache_Settings();

            Cache_Async cache_async = GetAsync(async);
            Cache_Slot cache_Slot = GetSlot(async, slot);

            foreach (var item in cache_async.settings)
            {
                if(item.Value != "" || !output.settings.ContainsKey(item.Key.ToString()))
                {
                    output.settings[item.Key.ToString()] = item.Value;
                }
            }

            foreach (var item in cache_Slot.settings)
            {
                if (item.Value != "" || !output.settings.ContainsKey(item.Key.ToString()))
                {
                    output.settings[item.Key.ToString()] = item.Value;
                }
            }

            foreach (var item in cache_Slot.customSettings)
            {
                if (item.Value != "" || !output.settings.ContainsKey(item.Key.ToString()))
                {
                    output.settings[item.Key] = item.Value;
                }
            }


            return output;
        }

        public static string GetToolVersion(string asyncName, string toolName)
        {
            Cache_Async cache = GetAsync(asyncName);


            if (cache.toolVersions.ContainsKey(toolName))
            {
                return cache.toolVersions[toolName];
            } else
            {
                List<string> versions = GetVersions(toolName);
                if (versions.Count > 0) {
                    string selectedVersion = versions[0];
                    SetAsyncToolVersion(asyncName, toolName, selectedVersion);
                    return selectedVersion;
                }
            }

            return "None";
        }

        public async static Task UpdateToolList()
        {
            List<Cache_DisplayLauncher> output = new List<Cache_DisplayLauncher>();
            Cache_Tools cache = LoadCache<Cache_Tools>(GetSaveLocation(tools));

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
            if(WindowManager.GetMainWindow() is MainWindow window)
            {
                window.UpdateToolList();
            } else
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

        public async static Task<List<Cache_DisplayLauncher>> GetToolList()
        {
            if(libraryTools.Count == 0)
            {
                await UpdateToolList();
            }
            return libraryTools;
        }

        public static bool AddDefaultVersion(string launcherName, string versionName, string apworldPath)
        {
            if (!File.Exists(apworldPath))
            {
                ErrorManager.AddNewError(
                    "IOManager_Launcher - Apworld doesn't exist",
                    "File at " + apworldPath + " doesn't exists"
                    );
                return false;
            }
            string directoryPath = Path.Combine(GetSaveLocation(ManagedApworlds), launcherName, versionName);
            Directory.CreateDirectory(directoryPath);
            return CopyFile(apworldPath, Path.Combine(directoryPath, GetFileName(apworldPath)));
        }

        public static void UpdateLastAsync(string gameName, Cache_PreviousSlot cache)
        {
            string basePath = GetSaveLocation(ManagedApworlds);
            foreach (var item in Directory.GetDirectories(basePath))
            {
                DirectoryInfo dir = new DirectoryInfo(item);
                string path = Path.Combine(item, launcher.GetFileName());
                if (File.Exists(path))
                {
                    Cache_CustomLauncher launcher = LoadCache<Cache_CustomLauncher>(path);
                    if (launcher.settings.TryGetValue(LauncherSettings.gameName, out var foundName) && foundName == gameName)
                    {
                        string toSave = Path.Combine(item, previous_async.GetFileName());
                        SaveCache<Cache_PreviousSlot>(toSave, cache);
                    }
                }
            }
        }

        public static Cache_PreviousSlot GetLastAsync(string launcherName)
        {
            string path = Path.Combine(GetSaveLocation(ManagedApworlds), launcherName, previous_async.GetFileName());
            if (File.Exists(path))
            {
                return LoadCache<Cache_PreviousSlot>(path);
            }
            return new Cache_PreviousSlot();
        }

        public static string FindApworld(string apfolder, string fileName)
        {
            string custom_world = Path.Combine(apfolder, "custom_worlds", fileName);
            string lib_world = Path.Combine(apfolder, "lib", "worlds", fileName);

            if (File.Exists(custom_world))
            {
                return custom_world;
            }
            else if (File.Exists(lib_world))
            {
                return lib_world;
            }

            return fileName;
        }
    }
}
