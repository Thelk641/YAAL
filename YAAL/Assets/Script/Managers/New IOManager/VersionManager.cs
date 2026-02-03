
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
    public static class VersionManager
    {
        public static bool AddDefaultVersion(string launcherName, string versionName, string apworldPath)
        {
            if (!File.Exists(apworldPath))
            {
                ErrorManager.AddNewError(
                    "VersionManager - Apworld doesn't exist",
                    "File at " + apworldPath + " doesn't exists"
                    );
                return false;
            }
            string directoryPath = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), launcherName, versionName);
            Directory.CreateDirectory(directoryPath);
            return FileManager.CopyFile(apworldPath, Path.Combine(directoryPath, IO_Tools.GetFileName(apworldPath)));
        }

        public static void AddDownloadedFilesToVersion(string gameName, string version, List<string> URLs, List<string> files)
        {
            string path = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), gameName, versions.GetFileName());
            Cache_Versions cache = CacheManager.LoadCache<Cache_Versions>(path);

            //TODO this is very wrong, it's noting down the URL for downloadedFiles (which is correct) AND for versions (which is not)

            if (cache.versions.ContainsKey(version))
            {

                List<string> alreadyDownloadedFiles = cache.downloaded[version];
                List<string> alreadyAddedFiles = cache.versions[version];
                foreach (var item in URLs)
                {
                    if (!alreadyDownloadedFiles.Contains(item))
                    {
                        alreadyDownloadedFiles.Add(item);
                    }
                }

                foreach (var item in files)
                {
                    if (!alreadyAddedFiles.Contains(item))
                    {
                        alreadyAddedFiles.Add(item);
                    }
                }
                CacheManager.SaveCache<Cache_Versions>(path, cache);
            } else
            {
                Cache_Versions newCache = new Cache_Versions();
                newCache.downloaded.Add(version, URLs);
                newCache.versions.Add(version, files);
                foreach (var item in cache.downloaded)
                {
                    newCache.downloaded.Add(item.Key, item.Value);
                }
                foreach (var item in cache.versions)
                {
                    newCache.versions.Add(item.Key, item.Value);
                }
                CacheManager.SaveCache<Cache_Versions>(path, newCache);
            }
        }

        public static void AddFilesToVersion(string gameName, string version, List<string> inputList)
        {
            string path = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), gameName, versions.GetFileName());
            Cache_Versions cache = CacheManager.LoadCache<Cache_Versions>(path);

            if (cache.versions.ContainsKey(version))
            {
                List<string> alreadyAddedFiles = cache.versions[version];
                foreach (var item in inputList)
                {
                    if (!alreadyAddedFiles.Contains(item))
                    {
                        alreadyAddedFiles.Add(item);
                    }
                }
                CopyVersionToWorkingDirectory(cache, gameName, version);
            }
            else
            {
                Cache_Versions newCache = new Cache_Versions();
                newCache.versions.Add(version, inputList);
                foreach (var item in cache.downloaded)
                {
                    newCache.downloaded.Add(item.Key, item.Value);
                }
                foreach (var item in cache.versions)
                {
                    newCache.versions.Add(item.Key, item.Value);
                }
                CopyVersionToWorkingDirectory(newCache, gameName, version);
            }
        }

        public static string CopyToVersionDirectory(string fileToCopy, string gameName, string version)
        {
            string workingName = IO_Tools.GetFileName(fileToCopy);


            string output = Path.Combine(
                SettingsManager.GetSaveLocation(ManagedApworlds),
                gameName,
                version,
                workingName
                );

            if (output == fileToCopy)
            {
                return fileToCopy;
            }

            if (FileManager.CopyFile(fileToCopy, output))
            {
                return output;
            }
            else
            {
                ErrorManager.ThrowError(
                "VersionManager - Failed to copy file to version directory",
                "Couldn't copy file " + fileToCopy + " to " + output
                );
                return fileToCopy;
            }
        }

        public static void CopyVersionToWorkingDirectory(Cache_Versions cache, string gameName, string version)
        {
            string path = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), gameName, versions.GetFileName());
            string versionDirectory = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), gameName, version);
            Directory.CreateDirectory(versionDirectory);
            List<string> filesToCopy = cache.versions[version];
            List<string> copiedFiles = new List<string>();
            foreach (var item in filesToCopy)
            {
                copiedFiles.Add(CopyToVersionDirectory(item, gameName, version));
            }
            cache.versions[version] = copiedFiles;
            CacheManager.SaveCache<Cache_Versions>(path, cache);
        }

        public static void CreateNewVersionCache(string gameName, string version)
        {
            string folder = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), gameName);
            Directory.CreateDirectory(folder);
            string fullPath = Path.Combine(folder, versions.GetFileName());
            Cache_Versions cache = new Cache_Versions();
            cache.downloaded.Add(version, new List<string>());
            cache.versions.Add(version, new List<string>());
            CacheManager.SaveCache<Cache_Versions>(fullPath, cache);
        }

        public static List<string> GetDownloadedVersions(string gameName)
        {
            string path = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), gameName, versions.GetFileName());
            Cache_Versions cache = CacheManager.LoadCache<Cache_Versions>(path);
            List<string> downloadedVersions = new List<string>();
            foreach (var item in cache.downloaded)
            {
                downloadedVersions.Add(item.Key);
            }
            return downloadedVersions;
        }

        public static List<string> GetFilesFromVersion(string gameName, string version)
        {
            string path = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), gameName, FileSettings.versions.GetFileName());
            Cache_Versions cache = CacheManager.LoadCache<Cache_Versions>(path);
            List<string> output = new List<string>();
            if (cache.versions.ContainsKey(version))
            {
                foreach (var item in cache.versions[version])
                {
                    output.Add(item);
                }
            }
            return output;
        }

        public static List<string> GetVersions(string gameName)
        {
            string path = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), gameName, FileSettings.versions.GetFileName());
            Cache_Versions cache = CacheManager.LoadCache<Cache_Versions>(path);
            List<string> versions = new List<string>();
            foreach (var item in cache.versions)
            {
                versions.Add(item.Key);
            }
            return versions;
        }

        public static bool HasThisBeenAlreadyDownloaded(string game, string version, string file)
        {
            string path = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), game, versions.GetFileName());
            Cache_Versions cache = CacheManager.LoadCache<Cache_Versions>(path);

            if (!cache.downloaded.ContainsKey(version))
            {
                return false;
            }

            return cache.downloaded[version].Contains(file);
        }

        public static void RemoveVersion(string game, string version)
        {
            string path = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), game, version);
            if (!Directory.Exists(path))
            {
                return;
            }
            FileManager.HardDeleteFile(path);
            string cacheJson = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), game, versions.GetFileName());
            if (File.Exists(cacheJson))
            {
                Cache_Versions cache = CacheManager.LoadCache<Cache_Versions>(cacheJson);
                if (cache.downloaded.ContainsKey(version))
                {
                    cache.downloaded.Remove(version);
                }
                if (cache.versions.ContainsKey(version))
                {
                    cache.versions.Remove(version);
                }
                CacheManager.SaveCache<Cache_Versions>(cacheJson, cache);
            }
        }

        public static void UpdateVersion(string gameName, string version, List<string> files)
        {
            string path = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), gameName, versions.GetFileName());
            Cache_Versions cache = CacheManager.LoadCache<Cache_Versions>(path);
            cache.versions[version] = files;
            CopyVersionToWorkingDirectory(cache, gameName, version);
        }

        public static void UpdateVersion(string gameName, string oldName, string newName, List<string> files)
        {
            string path = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), gameName, versions.GetFileName());
            Cache_Versions cache = CacheManager.LoadCache<Cache_Versions>(path);
            if (cache.versions.ContainsKey(newName))
            {
                ErrorManager.ThrowError(
                    "VersionManager - Version already exists",
                    "Tried to rename version " + oldName + " to " + newName + " but there's already a version of this name for this game. This is not allowed. Because of this, the version failed to save."
                    );
                return;
            }
            if (cache.downloaded.ContainsKey(oldName))
            {
                cache.downloaded[newName] = files;
                cache.downloaded.Remove(oldName);
            }

            cache.versions[newName] = files;
            cache.versions.Remove(oldName);
            CopyVersionToWorkingDirectory(cache, gameName, newName);
        }
    }
}
