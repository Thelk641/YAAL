using YAAL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using static YAAL.FileSettings;
using static YAAL.AsyncSettings;
using static YAAL.SlotSettings;

namespace YAAL
{

    public static partial class IOManager
    {
        public static Cache_Async CreateNewAsync(string name)
        {
            Cache_Async newAsync = new Cache_Async();
            newAsync.settings[asyncName] = name;
            string folderPath = Path.Combine(GetSaveLocation(Async), name);
            Directory.CreateDirectory(folderPath);
            SaveCache<Cache_Async>(Path.Combine(folderPath, multiworld.GetFileName()), newAsync);
            return newAsync;
        }

        public static Cache_Slot CreateNewSlot(Cache_Async async, string name)
        {
            Cache_Slot newSlot = new Cache_Slot();
            string asyncPath = Path.Combine(GetSaveLocation(Async), async.settings[asyncName]);
            Directory.CreateDirectory(Path.Combine(asyncPath, name));
            async.slots.Add(newSlot);
            newSlot.settings[slotName] = name;
            SaveCache<Cache_Async>(Path.Combine(asyncPath, multiworld.GetFileName()), async);
            return newSlot;
        }

        public static Cache_Async GetAsync(string name)
        {
            string folderPath = Path.Combine(GetSaveLocation(Async), name);
            string file = Path.Combine(folderPath, multiworld.GetFileName());
            if (!Directory.Exists(folderPath) || !File.Exists(file))
            {
                return CreateNewAsync(name);
            }
            return LoadCache<Cache_Async>(file);
        }

        public static Cache_Slot GetSlot(string asyncName, string slotName)
        {
            string asyncFolder = Path.Combine(GetSaveLocation(Async), asyncName);
            string slotFolder = Path.Combine(asyncFolder, slotName);
            if (!Directory.Exists(slotFolder))
            {
                return CreateNewSlot(GetAsync(asyncName), slotName);
            }
            Cache_Async cache = LoadCache<Cache_Async>(Path.Combine(asyncFolder, multiworld.GetFileName()));

            foreach (var item in cache.slots)
            {
                if (item.settings[SlotSettings.slotName] == slotName)
                {
                    return item;
                }
            }

            return CreateNewSlot(GetAsync(asyncName), slotName);
        }

        public static void SetAsyncSetting(string async, string key, string value)
        {

            if(Enum.TryParse(key, out AsyncSettings setting))
            {
                Cache_Async cache_Async = GetAsync(async);
                cache_Async.settings[setting] = value;
                if (key == "room")
                {
                    cache_Async.ParseRoomInfo();
                }
                SaveCache<Cache_Async>(Path.Combine(GetSaveLocation(Async), async, multiworld.GetFileName()), cache_Async);
            } else
            {
                ErrorManager.ThrowError(
                    "IOManager_Async - Invalid setting name",
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
            SaveCache<Cache_Async>(Path.Combine(GetSaveLocation(Async), async, FileSettings.Async.GetFileName()), cache_Async);
        }

        public static void SetSlotSetting(string async, string slot, SlotSettings key, string value)
        {
            Cache_Async cache_Async = GetAsync(async);


            foreach (var item in cache_Async.slots)
            {
                if (item.settings[slotName] == slot)
                {
                    item.settings[key] = value;
                    SaveCache<Cache_Async>(Path.Combine(GetSaveLocation(Async), async, multiworld.GetFileName()), cache_Async);
                    return;
                }
            }
            
        }

        public static List<string> GetAsyncList()
        {
            List<string> output = new List<string>();
            string asyncs = GetSaveLocation(Async);
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

        public static List<string> GetSlotList(string async)
        {
            Cache_Async cache = GetAsync(async);
            List<string> output = new List<string>();
            foreach (var item in cache.slots)
            {
                output.Add(item.settings[slotName]);
            }
            return output;
        }

        public static Cache_Async SaveAsync(Cache_Async oldAsync, Cache_Async newAsync)
        {
            if (oldAsync.settings[asyncName] != newAsync.settings[asyncName])
            {
                string dir = Path.Combine(GetSaveLocation(Async), newAsync.settings[asyncName]);
                string emptydir = Path.Combine(GetSaveLocation(Async), newAsync.settings[asyncName]);
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
                
                string oldDir = Path.Combine(GetSaveLocation(Async), oldAsync.settings[asyncName]);
                MoveFile(oldDir, emptydir);
            }
            SaveCache<Cache_Async>(Path.Combine(GetSaveLocation(Async), newAsync.settings[asyncName], FileSettings.Async.GetFileName()), newAsync);
            return newAsync;
        }

        public static void SaveSlot(string async, Cache_Slot newSlot, Cache_Slot oldSlot)
        {
            Cache_Async cache = GetAsync(async);
            Cache_Async newCache = (Cache_Async)cache.Clone();

            if (newCache.slots.Contains(oldSlot))
            {
                newCache.slots.Remove(oldSlot);
            }
            newCache.slots.Add(newSlot);
            SaveAsync(cache, newCache);
        }

        public static void DeleteAsync(string asyncName)
        {
            SoftDeleteFile(Path.Combine(GetSaveLocation(Async), asyncName));
        }

        public static void DeleteSlot(string asyncName, string slotName)
        {
            Cache_Async cache = GetAsync(asyncName);
            Cache_Slot slot = null;
            foreach (var item in cache.slots)
            {
                if (item.settings[SlotSettings.slotName] == slotName)
                {
                    slot = item;
                    break;
                }
            }

            if(slot != null)
            {
                cache.slots.Remove(slot);
                SaveCache<Cache_Async>(Path.Combine(GetSaveLocation(Async), cache.settings[AsyncSettings.asyncName], FileSettings.Async.GetFileName()), cache);
            }
        }
    }
}