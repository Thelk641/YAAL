using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public static void SaveLauncher(CustomLauncher toSave)
        {
            Debug.WriteLine("Saving launcher : " + toSave.selfsettings[LauncherSettings.launcherName]);
            Cache_CustomLauncher cache = toSave.WriteCache();
            SaveCacheLauncher(cache);
            UpdateLauncherList();
        }

        public static CustomLauncher LoadLauncher(string gameName)
        {
            if (!GetLauncherList().Contains(gameName)) 
            {
                ErrorManager.AddNewError(
                    "IOManager_Launcher - Invalid launcher name",
                    "IOManager was asked to load " + gameName + " but there doesn't appear to be a folder in /ManagedApworlds with this name containing a launcher.json"
                    );
                return null;
            }

            CustomLauncher output = new CustomLauncher();
            output.ReadCache(LoadCacheLauncher(gameName));
            return output;
        }

        public static void DeleteLauncher(string gameName)
        {
            SoftDeleteFile(Path.Combine(GetSaveLocation(ManagedApworlds), gameName));
        }

        public static List<string> GetLauncherList()
        {
            List<string> output = new List<string>();
            string managedApworlds = GetSaveLocation(ManagedApworlds);
            Directory.CreateDirectory(managedApworlds);

            foreach (var item in Directory.GetDirectories(managedApworlds))
            {
                DirectoryInfo dir = new DirectoryInfo(item);
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
            foreach (var item in launchers)
            {
                string gameName = item.selfsettings[LauncherSettings.gameName];
                if (gameName == toFind)
                {
                    return item.selfsettings[LauncherSettings.launcherName];
                }
            }

            return "";
        }

        public static List<string> GetLaunchersForGame(string toFind)
        {
            List<string> output = new List<string>();

            foreach (var item in launchers)
            {
                string gameName = item.selfsettings[LauncherSettings.gameName];
                if (gameName == toFind)
                {
                    output.Add(item.selfsettings[LauncherSettings.launcherName]);
                }
            }

            return output;
        }

        public static void ReadGameList()
        {
            games = new List<string>();

            foreach (var launcher in launchers)
            {
                string gameName = launcher.selfsettings[LauncherSettings.gameName];
                if (!games.Contains(gameName))
                {
                    games.Add(gameName);
                }
            }
        }

        public static void UpdateLauncherList()
        {
            launchers = new List<CustomLauncher>();
            foreach (var item in GetLauncherList())
            {
                CustomLauncher toAdd = LoadLauncher(item);
                launchers.Add(toAdd);
            }
            ReadGameList();
        }

        public static string GetLauncherNameFromSlot(string async, string slot)
        {
            return GetSlot(async, slot).settings[SlotSettings.baseLauncher];
        }

        public static Cache_Settings GetSettings(string async, string slot)
        {

            Cache_Settings output = new Cache_Settings();

            Cache_Async cache_async = GetAsync(async);
            Cache_Slot cache_Slot = GetSlot(async, slot);

            foreach (var item in settings.GetSettings())
            {
                string key = item.Key;
                string value = item.Value;
                output.settings[item.Key] = item.Value;
            }

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
                List<string> versions = GetDownloadedVersions(toolName);
                if (versions.Count > 0) {
                    string selectedVersion = versions[0];
                    SetAsyncToolVersion(asyncName, toolName, selectedVersion);
                    return selectedVersion;
                }
            }
            ErrorManager.ThrowError(
                "IOManager_Launcher - Couldn't find a version for a tool",
                "Tool " + toolName + " doesn't appear to have any versions installed. Please install one or, if you already have, report this issue."
                );

            return "";
        }

        public static void AddTool(CustomLauncher toAdd)
        {
            string path = GetSaveLocation(tools);
            string name = toAdd.GetSetting(LauncherSettings.launcherName.ToString());
            Cache_Tools cache = LoadCache<Cache_Tools>(path);

            if (!cache.toolList.Contains(name))
            {
                cache.toolList.Add(name);
                SaveCache<Cache_Tools>(path, cache);
            }
        }

        public static void RemoveTool(CustomLauncher toRemove)
        {
            string path = GetSaveLocation(tools);
            string name = toRemove.GetSetting(LauncherSettings.launcherName.ToString());
            Cache_Tools cache = LoadCache<Cache_Tools>(path);

            if (cache.toolList.Contains(name))
            {
                cache.toolList.Remove(name);
                SaveCache<Cache_Tools>(path, cache);
            }
        }

        public static List<string> GetToolList()
        {
            Cache_Tools cache = LoadCache<Cache_Tools>(GetSaveLocation(tools));
            return cache.toolList;
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
