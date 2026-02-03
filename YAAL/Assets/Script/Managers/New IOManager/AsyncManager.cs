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
using static YAAL.AsyncSettings;
using static YAAL.SlotSettings;

namespace YAAL
{

    public static class AsyncManager
    {
        public static Cache_Async CreateNewAsync(string name)
        {
            name = IO_Tools.FindAvailableDirectoryName(SettingsManager.GetSaveLocation(Async), name);
            Cache_Async newAsync = new Cache_Async();
            newAsync.settings[asyncName] = name;
            string folderPath = Path.Combine(SettingsManager.GetSaveLocation(Async), name);
            Directory.CreateDirectory(folderPath);
            CacheManager.SaveCache<Cache_Async>(Path.Combine(folderPath, multiworld.GetFileName()), newAsync);
            return newAsync;
        }

        public static Cache_Slot CreateNewSlot(Cache_Async async, string name)
        {
            Cache_Slot newSlot = new Cache_Slot();
            string asyncPath = Path.Combine(SettingsManager.GetSaveLocation(Async), async.settings[asyncName]);
            name = IO_Tools.FindAvailableDirectoryName(asyncPath, name);
            Directory.CreateDirectory(Path.Combine(asyncPath, name));
            async.slots.Add(newSlot);
            newSlot.settings[slotLabel] = name;
            newSlot.settings[slotName] = name;
            CacheManager.SaveCache<Cache_Async>(Path.Combine(asyncPath, multiworld.GetFileName()), async);
            return newSlot;
        }

        public static void DeleteAsync(string asyncName)
        {
            FileManager.SoftDeleteFile(Path.Combine(SettingsManager.GetSaveLocation(Async), asyncName));
        }

        public static void DeleteSlot(string asyncName, string slotLabel)
        {
            try
            {

                Cache_Async cache = GetAsync(asyncName);
                Cache_Slot slot = null;
                foreach (var item in cache.slots)
                {
                    if (item.settings[SlotSettings.slotLabel] == slotLabel)
                    {
                        slot = item;
                        break;
                    }
                }

                if (slot != null)
                {
                    cache.slots.Remove(slot);
                    CacheManager.SaveCache<Cache_Async>(Path.Combine(SettingsManager.GetSaveLocation(Async), cache.settings[AsyncSettings.asyncName], multiworld.GetFileName()), cache);
                    FileManager.SoftDeleteFile(Path.Combine(SettingsManager.GetSaveLocation(Async), cache.settings[AsyncSettings.asyncName], slotLabel));
                }
            }
            catch (Exception e)
            {
                ErrorManager.ThrowError(
                    "AsyncManager - Deleting a slot threw an error",
                    "Please report this. Trying to delete slot " + slotLabel + " from async " + asyncName + " threw the following error : " + e.Message);
            }

        }

        public static Cache_Async GetAsync(string name)
        {
            string folderPath = Path.Combine(SettingsManager.GetSaveLocation(Async), name);
            string file = Path.Combine(folderPath, multiworld.GetFileName());
            if (!Directory.Exists(folderPath) || !File.Exists(file))
            {
                return CreateNewAsync(name);
            }

            Cache_Async output = CacheManager.LoadCache<Cache_Async>(file);

            foreach (var item in output.slots)
            {
                if (!item.settings.ContainsKey(SlotSettings.slotLabel))
                {
                    // Backward compatibility
                    item.settings[SlotSettings.slotLabel] = item.settings[SlotSettings.slotName];
                }
            }

            return output;
        }

        public static List<string> GetAsyncList()
        {
            List<string> output = new List<string>();
            string asyncs = SettingsManager.GetSaveLocation(Async);
            Directory.CreateDirectory(asyncs);

            foreach (var item in Directory.GetDirectories(asyncs))
            {
                DirectoryInfo dir = new DirectoryInfo(item);
                if (File.Exists(Path.Combine(item, multiworld.GetFileName())))
                {
                    output.Add(dir.Name);
                }
            }
            return output;
        }

        public static string GetLauncherNameFromSlot(string async, string slot)
        {
            return GetSlot(async, slot).settings[baseLauncher];
        }

        public static Cache_Settings GetSettings(string async, string slot)
        {

            Cache_Settings output = new Cache_Settings();

            Cache_Async cache_async = GetAsync(async);
            Cache_Slot cache_Slot = GetSlot(async, slot);

            foreach (var item in cache_async.settings)
            {
                if (item.Value != "" || !output.settings.ContainsKey(item.Key.ToString()))
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

        public static Cache_Slot GetSlot(string asyncName, string slotLabel)
        {
            string asyncFolder = Path.Combine(SettingsManager.GetSaveLocation(Async), asyncName);
            string slotFolder = Path.Combine(asyncFolder, slotLabel);
            if (!Directory.Exists(slotFolder))
            {
                return CreateNewSlot(GetAsync(asyncName), slotLabel);
            }
            Cache_Async cache = CacheManager.LoadCache<Cache_Async>(Path.Combine(asyncFolder, multiworld.GetFileName()));

            foreach (var item in cache.slots)
            {
                if (item.settings[SlotSettings.slotLabel] == slotLabel)
                {
                    return item;
                }
            }

            return CreateNewSlot(GetAsync(asyncName), slotLabel);
        }

        public static List<string> GetSlotList(string async)
        {
            Cache_Async cache = GetAsync(async);
            List<string> output = new List<string>();
            foreach (var item in cache.slots)
            {
                if (item.settings.ContainsKey(slotLabel))
                {
                    output.Add(item.settings[slotLabel]);
                }
                else
                {
                    // Backward compatibility
                    output.Add(item.settings[slotName]);
                }
            }
            return output;
        }
        
        public static string GetSlotDirectory(string async, string slot)
        {
            string output = Path.Combine(SettingsManager.GetSaveLocation(Async), async, slot);
            output = IO_Tools.ProcessLocalPath(output);
            Directory.CreateDirectory(output);
            return output;
        }

        public static string GetToolVersion(string asyncName, string toolName)
        {
            Cache_Async cache = GetAsync(asyncName);


            if (cache.toolVersions.ContainsKey(toolName))
            {
                return cache.toolVersions[toolName];
            }
            else
            {
                List<string> versions = VersionManager.GetVersions(toolName);
                if (versions.Count > 0)
                {
                    string selectedVersion = versions[0];
                    SetAsyncToolVersion(asyncName, toolName, selectedVersion);
                    return selectedVersion;
                }
            }

            return "None";
        }

        public static string MoveToSlotDirectory(string fileToMove, string asyncName, string slotName, string newName)
        {
            string workingName = "";

            if (newName == "")
            {
                workingName = IO_Tools.GetFileName(fileToMove);

            }
            else
            {
                workingName = newName + Path.GetExtension(IO_Tools.GetFileName(fileToMove));
            }


            string output = Path.Combine(
                SettingsManager.GetSaveLocation(Async),
                asyncName,
                slotName,
                workingName
                );

            if (output == fileToMove || File.Exists(output))
            {
                // We've already moved this file to the right directory in the past
                return fileToMove;
            }

            if (FileManager.MoveFile(fileToMove, output))
            {
                return output;
            }
            else
            {
                ErrorManager.AddNewError(
                "AsyncManager - Failed to move file to working directory",
                "Couldn't move file " + fileToMove + " to " + output
                );
                return "";
            }


        }

        public static Cache_Async SaveAsync(Cache_Async oldAsync, Cache_Async newAsync)
        {
            if (oldAsync.settings[asyncName] != newAsync.settings[asyncName])
            {
                string dir = Path.Combine(SettingsManager.GetSaveLocation(Async), newAsync.settings[asyncName]);
                string emptydir = Path.Combine(SettingsManager.GetSaveLocation(Async), newAsync.settings[asyncName]);
                int i = 0;
                if (Directory.Exists(emptydir))
                {
                    while (Directory.Exists(emptydir))
                    {
                        emptydir = dir + " - " + i;
                        ++i;
                    }
                    newAsync.settings[asyncName] += " - " + i;
                }

                string oldDir = Path.Combine(SettingsManager.GetSaveLocation(Async), oldAsync.settings[asyncName]);
                FileManager.MoveFile(oldDir, emptydir);
            }
            CacheManager.SaveCache<Cache_Async>(Path.Combine(SettingsManager.GetSaveLocation(Async), newAsync.settings[asyncName], multiworld.GetFileName()), newAsync);
            return newAsync;
        }

        public static string SaveSlot(string async, Cache_Slot newSlot, Cache_Slot oldSlot)
        {
            bool needSaving = false;
            foreach (var item in newSlot.settings)
            {
                if (item.Value != oldSlot.settings[item.Key])
                {
                    needSaving = true;
                    break;
                }
            }

            if (!needSaving)
            {
                return newSlot.settings[slotLabel];
            }
            Cache_Async cache = GetAsync(async);
            Cache_Async newCache = (Cache_Async)cache.Clone();

            Cache_Slot toRemove = null;
            string output = newSlot.settings[slotLabel];

            foreach (var item in newCache.slots)
            {
                if (item.settings[slotLabel] == oldSlot.settings[slotLabel])
                {
                    toRemove = item;
                    break;
                }
            }

            if (toRemove != null)
            {
                newCache.slots.Remove(toRemove);
            }

            if (oldSlot.settings[slotLabel] != newSlot.settings[slotLabel])
            {
                string asyncDir = Path.Combine(SettingsManager.GetSaveLocation(Async), async);
                string oldDir = Path.Combine(asyncDir, oldSlot.settings[slotLabel]);

                string newName = IO_Tools.FindAvailableDirectoryName(asyncDir, newSlot.settings[slotLabel]);
                FileManager.MoveFile(oldDir, Path.Combine(asyncDir, newName));
                newSlot.settings[slotLabel] = newName;
                output = newName;
            }


            newCache.slots.Add(newSlot);

            if (newSlot.settings[patch] != oldSlot.settings[patch])
            {
                FileManager.SoftDeleteFile(oldSlot.settings[patch]);
            }

            SaveAsync(cache, newCache);

            return output;
        }

        public static void SetAsyncSetting(string async, string key, string value)
        {

            if (Enum.TryParse(key, out AsyncSettings setting))
            {
                Cache_Async cache_Async = GetAsync(async);
                cache_Async.settings[setting] = value;
                if (key == "room")
                {
                    cache_Async.ParseRoomInfo();
                }
                CacheManager.SaveCache<Cache_Async>(Path.Combine(SettingsManager.GetSaveLocation(Async), async, multiworld.GetFileName()), cache_Async);
            }
            else
            {
                ErrorManager.ThrowError(
                    "AsyncManager - Invalid setting name",
                    "Tried to assign a value to setting " + key + " for async " + async + " but this async doesn't have such setting. Please report this issue."
                    );
            }
        }

        public static void SetAsyncToolVersion(string async, string name, string value)
        {
            Cache_Async cache_Async = GetAsync(async);
            cache_Async.toolVersions[name] = value;
            if (name == "room")
            {
                cache_Async.ParseRoomInfo();
            }
            CacheManager.SaveCache<Cache_Async>(Path.Combine(SettingsManager.GetSaveLocation(Async), async, multiworld.GetFileName()), cache_Async);
        }

        public static void SetSlotSetting(string async, string slot, SlotSettings key, string value)
        {
            Cache_Async cache_Async = GetAsync(async);

            foreach (var item in cache_Async.slots)
            {
                if (item.settings[slotLabel] == slot)
                {
                    item.settings[key] = value;
                    CacheManager.SaveCache<Cache_Async>(Path.Combine(SettingsManager.GetSaveLocation(Async), async, multiworld.GetFileName()), cache_Async);
                    return;
                }
            }
        }

        public static void SetSlotSetting(string async, string slot, string key, string value)
        {
            Cache_Async cache_Async = GetAsync(async);

            foreach (var item in cache_Async.slots)
            {
                if (item.settings[slotLabel] == slot)
                {
                    item.customSettings[key] = value;
                    CacheManager.SaveCache<Cache_Async>(Path.Combine(SettingsManager.GetSaveLocation(Async), async, multiworld.GetFileName()), cache_Async);
                    return;
                }
            }
        }
    }
}