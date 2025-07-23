
using YAAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using static YAAL.FileSettings;

namespace YAAL
{
    public static partial class IOManager
    {
        public static void UpdateDownloadCache(string gameName, string version, List<string> inputList)
        {
            string path = Path.Combine(GetSaveLocation(ManagedApworlds), gameName, cache_download.GetFileName());
            Cache_Download cache = LoadCache<Cache_Download>(path);

            if (cache.downloadedInfos.ContainsKey(version))
            {
                List<string> alreadyDownloadedFiles = cache.downloadedInfos[version];
                foreach (var item in inputList)
                {
                    if (!alreadyDownloadedFiles.Contains(item))
                    {
                        alreadyDownloadedFiles.Add(item);
                    }
                }
                SaveCache<Cache_Download>(path, cache);
            }
            else
            {
                Cache_Download newCache = new Cache_Download();
                newCache.downloadedInfos.Add(version, inputList);
                foreach (var item in cache.downloadedInfos)
                {
                    newCache.downloadedInfos.Add(item.Key, item.Value);
                }
                SaveCache<Cache_Download>(path, newCache);
            }
        }

        public static void CreateNewDownloadCache(string gameName, string version) {
            string folder = Path.Combine(GetSaveLocation(ManagedApworlds), gameName);
            Directory.CreateDirectory(folder);
            string fullPath = Path.Combine(folder, cache_download.GetFileName());
            Cache_Download cache = new Cache_Download();
            cache.downloadedInfos.Add(version, new List<string>());
            SaveCache<Cache_Download>(fullPath, cache);
        }

        public static List<String> GetDownloadedVersions(string gameName)
        {
            string path = Path.Combine(GetSaveLocation(ManagedApworlds), gameName, cache_download.GetFileName());
            Cache_Download cache = LoadCache<Cache_Download>(path);
            List<string> downloadedVersions = new List<string>();
            foreach (var item in cache.downloadedInfos)
            {
                downloadedVersions.Add(item.Key);
            }
            return downloadedVersions;
        }

        public static bool HasThisBeenAlreadyDownloaded(string game, string version, string file)
        {
            string path = Path.Combine(GetSaveLocation(ManagedApworlds), game, cache_download.GetFileName());
            Cache_Download cache = LoadCache<Cache_Download>(path);

            if (!cache.downloadedInfos.ContainsKey(version))
            {
                return false;
            }

            return cache.downloadedInfos[version].Contains(file);
        }
    }
}
