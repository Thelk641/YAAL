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
using static YAAL.GeneralSettings;

namespace YAAL
{

    public static class BackupManager
    {
        public static bool CopyToDefault(string gameName, string path, out string output)
        {
            string defaultFolder = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), gameName, "Default");
            Directory.CreateDirectory(defaultFolder);
            output = Path.Combine(defaultFolder, IO_Tools.GetFileName(path));

            if (File.Exists(output))
            {
                return true;
            }

            if (path == output)
            {
                return true;
            }

            if (File.Exists(path))
            {
                return FileManager.CopyFile(path, output);
            }

            if (Directory.Exists(path))
            {
                return FileManager.CopyFolder(path, output);
            }

            ErrorManager.AddNewError(
                "IOManager_FileExtra - Target default doesn't exist",
                "Tried to copy " + path + "to the default directory, but this file or folder doesn't seem to exist."
                );
            return false;
        }

        public static string GetTemporaryFilePath(string originalFile)
        {
            Cache_BackupList cache = CacheManager.LoadCache<Cache_BackupList>(SettingsManager.GetSaveLocation(backupList));
            foreach (var item in cache.backupList)
            {
                if (item.originalFile == originalFile)
                {
                    return item.backedUpFile;
                }
            }
            return "";
        }

        public static void NoteBackup(Cache_Backup toAdd)
        {
            Cache_BackupList list = CacheManager.LoadCache<Cache_BackupList>(SettingsManager.GetSaveLocation(backupList));
            list.backupList.Add(toAdd);
            CacheManager.SaveCache<Cache_BackupList>(SettingsManager.GetSaveLocation(backupList), list);
        }

        public static void NoteRestore(Cache_Backup toRemove)
        {
            Cache_BackupList list = CacheManager.LoadCache<Cache_BackupList>(SettingsManager.GetSaveLocation(backupList));
            List<Cache_Backup> newList = new List<Cache_Backup>();

            foreach (var item in list.backupList)
            {
                if (item.originalFile != toRemove.originalFile)
                {
                    newList.Add(item);
                }
            }
            list.backupList = newList;
            CacheManager.SaveCache<Cache_BackupList>(SettingsManager.GetSaveLocation(backupList), list);
        }

        public static bool SetUpMinimumWorlds()
        {
            string targetFolder = SettingsManager.GetSaveLocation(MinimumWorlds);
            if (Directory.Exists(targetFolder))
            {
                return true;
            }

            Directory.CreateDirectory(targetFolder);

            DirectoryInfo dir = new DirectoryInfo(targetFolder);
            dir.Attributes |= FileAttributes.Hidden;

            string pathToAPFolder = SettingsManager.GetSetting(GeneralSettings.aplauncher);
            string archipelagoFolder = Path.GetDirectoryName(pathToAPFolder)!;

            if (!Directory.Exists(archipelagoFolder))
            {
                ErrorManager.AddNewError(
                    "IOManager - Archipelago folder doesn't exists",
                    "Your ArchipelagoLauncher.exe is at : " + pathToAPFolder + " sadly, the folder containing this file doesn't appear to exist."
                    );
                return false;
            }
            string worldsFolder = Path.Combine(archipelagoFolder, "lib", "worlds");
            return
                FileManager.CopyFolder(Path.Combine(worldsFolder, "_bizhawk"), Path.Combine(targetFolder, "_bizhawk"))
                && FileManager.CopyFolder(Path.Combine(worldsFolder, "generic"), Path.Combine(targetFolder, "generic"))
                && FileManager.CopyFile(Path.Combine(worldsFolder, "__init__.pyc"), Path.Combine(targetFolder, "__init__.pyc"))
                && FileManager.CopyFile(Path.Combine(worldsFolder, "AutoSNIClient.pyc"), Path.Combine(targetFolder, "AutoSNIClient.pyc"))
                && FileManager.CopyFile(Path.Combine(worldsFolder, "AutoWorld.pyc"), Path.Combine(targetFolder, "AutoWorld.pyc"))
                && FileManager.CopyFile(Path.Combine(worldsFolder, "Files.pyc"), Path.Combine(targetFolder, "Files.pyc"))
                && FileManager.CopyFile(Path.Combine(worldsFolder, "LauncherComponents.pyc"), Path.Combine(targetFolder, "LauncherComponents.pyc"))
                && FileManager.CopyFile(Path.Combine(worldsFolder, "smw.apworld"), Path.Combine(targetFolder, "smw.apworld"));
        }

        public static bool UpdateFileToVersion(string path, string launcherName, string version, string isNecessary)
        {
            if (path == "")
            {
                ErrorManager.AddNewError(
                    "IOManager_FileExtra - Empty path",
                    "Something tried to update a file version, but didn't set a path for said file."
                    );
                return false;
            }

            if (version == "")
            {
                ErrorManager.AddNewError(
                    "IOManager_FileExtra - No version selected",
                    "This particular slot doesn't have a version selected, please set one."
                    );
                return false;
            }

            string fileName;

            path = IO_Tools.CleanUpPath(path);
            if (path.EndsWith("\\"))
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                fileName = dir.Name;

            }
            else
            {
                fileName = Path.GetFileName(path);
            }

            if (fileName == "YAAL.apworld")
            {
                // This is a meta apworld, it's not supposed to be in this ManagedApworlds folder
                return true;
            }

            string versionFile = Path.Combine(SettingsManager.GetSaveLocation(ManagedApworlds), launcherName, version, fileName);

            if (!File.Exists(versionFile))
            {
                // Our download folder for this version doesn't contain filename
                // if we do need the file, error out
                // if we don't, just ignore this and move on
                if (isNecessary == true.ToString())
                {
                    ErrorManager.AddNewError(
                        "IOManager_FileExtra - Missing mandatory apworld",
                        "This launcher requires the following file, which doesn't seem to exists for the selected version : " + fileName
                        );
                    return false;
                }
                else
                {
                    return true;
                }
            }

            FileManager.HardDeleteFile(path);
            return FileManager.CopyFile(versionFile, path);
        }
    }
}