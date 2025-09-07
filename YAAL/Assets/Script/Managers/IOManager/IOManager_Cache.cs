using Avalonia.Controls.ApplicationLifetimes;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YAAL;
using static YAAL.FileSettings;
using static YAAL.LauncherSettings;

namespace YAAL
{
    public static partial class IOManager
    {
        public static T LoadCache<T>(string path) where T : new()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (!File.Exists(path))
            {
                return new T();
            }

            string json = File.ReadAllText(path);
            T output = new T();
            try
            {
                output = JsonConvert.DeserializeObject<T>(json) ?? new T();
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

            return output;
        }

        public static void SaveCache<T>(string path, T cache)
        {
            string json = JsonConvert.SerializeObject(cache, Formatting.Indented);
            SaveFile(path, json);
        }

        public static void SaveCacheLauncher(Cache_CustomLauncher toSave)
        {
            if (toSave.settings[launcherName] == null || toSave.settings[launcherName] == "")
            {
                ErrorManager.ThrowError(
                    "IOManager_Cache - Tried to save empty launcher",
                    "For some reason, SaveCacheLauncher() was asked to save a launcher without a name. This is not allowed."
                    );
                return;
            }
            string savePath = Path.Combine(GetSaveLocation(ManagedApworlds), toSave.settings[launcherName], "launcher.json");
            string json = JsonConvert.SerializeObject(toSave, Formatting.Indented);
            SaveFile(savePath, json);
        }

        public static string SaveCacheError(Cache_ErrorList error)
        {
            string savePath = Path.Combine(GetSaveLocation(Logs), (GetTime() + ".json"));

            string readableLog = "";

            foreach (var item in error.errors)
            {
                readableLog += $@"
                === ERROR: {item.name} ===

                {item.content}

                Stack Trace:
                {item.stackTrace}

                ===========================

                ";
            }

            readableLog = string.Join(Environment.NewLine,
                            readableLog.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                                       .Select(line => line.TrimEnd())       // remove trailing space
                                       .Select(line => line.TrimStart())     // remove leading space
                                       .Select(line => line.StartsWith("at ") ? "\t" + line : line)
                            );



            File.WriteAllText(savePath, readableLog.Trim());
            return Path.GetFullPath(savePath);
        }

        public static void ReadCacheError(string path)
        {
            string json = "";
            try
            {
                json = File.ReadAllText(path.Replace("\\\\", "\\").Trim('\"'));
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                    "Failed to find error file",
                    "Trying to display error threw the following exception : " + e.Message);
                return;
            }

            Cache_ErrorList output = new Cache_ErrorList();
            var pattern = @"=== ERROR: (?<name>.+?) ===\r?\n\r?\n(?<content>.*?)\r?\n\r?\nStack Trace:\r?\n(?<stack>.*?)\r?\n\r?\n=+";
            var matches = Regex.Matches(json, pattern, RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                ErrorManager.AddNewError(
                    match.Groups["name"].Value.Trim(),
                    match.Groups["content"].Value.Trim(),
                    match.Groups["stack"].Value.Trim());
            }
        }

        public static Cache_CustomLauncher LoadCacheLauncher(string launcherName)
        {
            string savePath = Path.Combine(GetSaveLocation(ManagedApworlds), launcherName, "launcher.json");
            if (!File.Exists(savePath))
            {
                return new Cache_CustomLauncher();
            }

            string json = File.ReadAllText(savePath);
            Debug.WriteLine("Loading launcher : " + launcherName);
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
            return output;
        }

        public static Cache_Theme GetTheme(string launcherName)
        {
            if (!GetLauncherList().Contains(launcherName))
            {
                return new Cache_Theme();
            }

            Cache_CustomLauncher cache = LoadCacheLauncher(launcherName);
            if(cache.customTheme != null)
            {
                return cache.customTheme;
            } else
            {
                return new Cache_Theme();
            }
        }

        public static string? GetGameNameFromApworld(string apworldPath)
        {
            using ZipArchive archive = ZipFile.OpenRead(apworldPath);

            // Look for any file that ends with __init__.py
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.Contains("__init__.py"))
                {
                    using StreamReader reader = new StreamReader(entry.Open());
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var match = Regex.Match(line, @"^\s*game\s*=\s*[""'](?<game>[^""']+)[""']\s*$");
                        if (match.Success)
                        {
                            return match.Groups["game"].Value;
                        }
                    }
                }
            }

            return ""; // Not found
        }

        public static void NoteBackup(Cache_Backup toAdd)
        {
            Cache_BackupList list = LoadCache<Cache_BackupList>(GetSaveLocation(backupList));
            list.backupList.Add(toAdd);
            SaveCache<Cache_BackupList>(GetSaveLocation(backupList), list);
        }

        public static void NoteRestore(Cache_Backup toRemove)
        {
            Cache_BackupList list = LoadCache<Cache_BackupList>(GetSaveLocation(backupList));
            List<Cache_Backup> newList = new List<Cache_Backup>();

            foreach (var item in list.backupList)
            {
                if (item.originalFile != toRemove.originalFile)
                {
                    newList.Add(item);
                }
            }
            list.backupList = newList;
            SaveCache<Cache_BackupList>(GetSaveLocation(backupList), list);
        }

        public static string GetTemporaryFilePath(string originalFile)
        {
            Cache_BackupList cache = LoadCache<Cache_BackupList>(GetSaveLocation(backupList));
            foreach (var item in cache.backupList)
            {
                if (item.originalFile == originalFile)
                {
                    return item.backedUpFile;
                }
            }
            return "";
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

            SaveCache<Cache_UserSettings>(Path.Combine(AppContext.BaseDirectory, userSettings.GetFileName()), settings);
        }
    }
}
