using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShellLink.Structures;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using YAAL.Assets.Script.Cache;
using static YAAL.FileSettings;
using static YAAL.GeneralSettings;

namespace YAAL
{
    public static partial class IOManager
    {
        public static bool IsAlreadyBackedUp(string path)
        {
            Cache_BackupList cache = LoadCache<Cache_BackupList>(GetSaveLocation(backupList));
            foreach (var item in cache.backupList)
            {
                if (item.originalFile == path && item.movedToBackup == true)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Backup(string path, string defaultFile, string asyncName, string slotName, bool noteIt)
        {
            if (IsAlreadyBackedUp(path))
            {
                ErrorManager.AddNewError(
                    "IOManager_FileCore - File already backed up",
                    "File " + path + " has already been backed up by a launcher that is waiting to auto-restore it. This is not allowed. This can also be caused by a failed auto-restore, please check 'YAAL/ManagedApworlds/backupList.json' for a list of files waiting to be successfully auto-restored."
                    );

                return false;
            }

            string slotDir = Path.Combine(GetSaveLocation(Async), asyncName, slotName);
            string tempBackupDir = Path.Combine(slotDir, "Temporary Backup");
            Directory.CreateDirectory(tempBackupDir);
            string backupDir = Path.Combine(slotDir, "Backup");
            Directory.CreateDirectory(backupDir);

            string pathName = GetFileName(path);
            string backedupFile = Path.Combine(backupDir, pathName);
            string tempBackedupFile = Path.Combine(tempBackupDir, pathName);
            if (File.Exists(tempBackedupFile))
            {
                tempBackedupFile = FindAvailableFileName(tempBackupDir, pathName);
            }
            if (Directory.Exists(tempBackedupFile))
            {
                tempBackedupFile = Path.Combine(tempBackupDir, FindAvailableDirectoryName(tempBackupDir, pathName));
            }

            // putting file or folder at "path" away in the Temporary Backup folder
            if (File.Exists(path) || Directory.Exists(path))
            {
                Cache_Backup fileToTemporary = new Cache_Backup();
                fileToTemporary.originalFile = path;
                fileToTemporary.backedUpFile = tempBackedupFile;
                fileToTemporary.asyncName = asyncName;
                fileToTemporary.slotName = slotName;
                fileToTemporary.movedToBackup = true;

                if (!MoveFile(path, tempBackedupFile))
                {
                    ErrorManager.AddNewError(
                        "IOManager_FileCore - Moving file to temporary backup failed",
                        "Trying to move " + path + " to " + tempBackedupFile + " failed."
                        );
                    return false;
                }
                NoteBackup(fileToTemporary);
            }

            string actualBackedUpFile = defaultFile;

            Cache_Backup backupToTarget = new Cache_Backup();
            backupToTarget.originalFile = backedupFile;
            backupToTarget.backedUpFile = path;
            backupToTarget.asyncName = asyncName;
            backupToTarget.slotName = slotName;
            backupToTarget.movedToBackup = false;



            // if we've got a backup for this async/slot, let's put it in there
            if (File.Exists(backedupFile))
            {
                if (CopyFile(backedupFile, path))
                {
                    NoteBackup(backupToTarget);
                    return true;
                }
            }
            else if (Directory.Exists(backedupFile))
            {
                if (CopyFolder(backedupFile, path))
                {
                    NoteBackup(backupToTarget);
                    return true;
                }
            }
            else
            {
                if (File.Exists(defaultFile))
                {
                    if (CopyFile(defaultFile, path))
                    {
                        NoteBackup(backupToTarget);
                        return true;
                    }
                    ErrorManager.AddNewError(
                        "IOManager_FileCore - Copying default file failed",
                        "Trying to copy " + defaultFile + " to " + path + " failed."
                        );
                    return false;
                } else if (Directory.Exists(defaultFile))
                {
                    if (CopyFolder(defaultFile, path))
                    {
                        NoteBackup(backupToTarget);
                        return true;
                    }
                    ErrorManager.AddNewError(
                        "IOManager_FileCore - Copying default folder failed",
                        "Trying to copy " + defaultFile + " to " + path + " failed."
                        );
                    return false;
                } else
                {
                    NoteBackup(backupToTarget);
                    return true;
                }
            }

            // Either MoveFile or CopyFile failed
            ErrorManager.AddNewError(
                "IOManager_FileCore - Unknown error",
                "Backup returned false for unknown reasons. Please check other errors for more information."
                );
            return false;
        }

        public static bool Restore(string path, string asyncName, string slotName)
        {
            Debug.WriteLine("FileCore, trying to restore : " + path);

            string slotDir = Path.Combine(GetSaveLocation(Async), asyncName, slotName);
            string tempBackupDir = Path.Combine(slotDir, "Temporary Backup");
            Directory.CreateDirectory(tempBackupDir);
            string backupDir = Path.Combine(slotDir, "Backup");
            Directory.CreateDirectory(backupDir);

            string pathName = GetFileName(path);
            string backedupFile = Path.Combine(backupDir, pathName);
            string tempBackedupFile = GetTemporaryFilePath(path);

            if(IsAlreadyBackedUp(path) && (!File.Exists(tempBackedupFile) && !Directory.Exists(tempBackedupFile)))
            {
                ErrorManager.AddNewError(
                    "IOManager_FileCore - Backedup file doesn't appear to exists",
                    "File " + path + " has been backed up to " + tempBackedupFile + " but it doesn't appear to exist anymore. If you've not done that yourself, please report this issue."
                    );
            }

            // putting file or folder at target into Backup for the next time we open this slot
            if (File.Exists(path) || Directory.Exists(path))
            {
                // These are not the info of what we're doing right now
                // but of the backup we're undoing, hence they appear backward
                Cache_Backup pathToBackup = new Cache_Backup();
                pathToBackup.originalFile = backedupFile;
                pathToBackup.backedUpFile = path;
                pathToBackup.asyncName = asyncName;
                pathToBackup.slotName = slotName;
                pathToBackup.movedToBackup = false;

                string oldBackupDir = Path.Combine(slotDir, "Backup.old", GetTime());

                if (File.Exists(backedupFile) || Directory.Exists(backedupFile))
                {
                    Directory.CreateDirectory(oldBackupDir);
                    if (!MoveFile(backedupFile, Path.Combine(oldBackupDir, pathName)))
                    {
                        ErrorManager.AddNewError(
                            "IOManager_FileCore - Updating backup failed",
                            "While restoring, trying to update backup " + backedupFile + " by moving the old one to " + Path.Combine(oldBackupDir, pathName) + " failed."
                            );
                        return false;
                    }
                }

                if (!MoveFile(path, backedupFile))
                {
                    ErrorManager.AddNewError(
                        "IOManager_FileCore - Updating backup failed",
                        "While restoring, trying to backup " + path + " to " + backedupFile + " failed."
                        );
                    return false;
                }
                NoteRestore(pathToBackup);
            }

            if (!File.Exists(tempBackedupFile) && !Directory.Exists(tempBackedupFile))
            {
                // If we don't use a default file and don't have a pre-existing file,
                // we only need to restore our own slot's backup, and then we're done
                return true;
            }


            Cache_Backup temporaryToPath = new Cache_Backup();
            temporaryToPath.originalFile = path;
            temporaryToPath.backedUpFile = tempBackedupFile;
            temporaryToPath.asyncName = asyncName;
            temporaryToPath.slotName = slotName;
            temporaryToPath.movedToBackup = true;

            if (!MoveFile(tempBackedupFile, path))
            {
                ErrorManager.AddNewError(
                    "IOManager_FileCore - Restore failed",
                    "Trying to restore " + tempBackedupFile + " to " + path + " failed."
                    );
                return false;
            }
            NoteRestore(temporaryToPath);
            return true;
        }

        public static bool RestoreBackups()
        {
            Cache_BackupList backupList = LoadCache<Cache_BackupList>(GetSaveLocation(FileSettings.backupList));
            if(backupList.backupList.Count == 0)
            {
                return true;
            }

            List<Cache_Backup> toRestore = new List<Cache_Backup>();
            foreach (var item in backupList.backupList)
            {
                // backupList also contains movement from Backup to origin
                // (restoring a slot's files when opening it)
                // but our restore function already accounts for that
                if (item.movedToBackup)
                {
                    toRestore.Add(item);
                }
            }

            foreach (var item in toRestore)
            {
                if(!Restore(item.originalFile, item.asyncName, item.slotName))
                {
                    return false;
                }
            }
            return true;
        }

        public static void ResetBackupList()
        {
            Cache_BackupList cache = new Cache_BackupList();
            SaveCache<Cache_BackupList>(GetSaveLocation(FileSettings.backupList), cache);
        }

        public static void RestoreAll()
        {
            if (!RestoreApworlds())
            {
                ErrorManager.ThrowError(
                        "App - Failed to restore apworlds",
                        "Something went wrong while trying to restore apworlds directly."
                        );
                return;
            }

            if (!RestoreBackups())
            {
                ErrorManager.ThrowError(
                        "App - Failed to restore backups",
                        "Something went wrong while trying to restore backups directly."
                        );
                return;
            }

            ResetBackupList();
        }

        private static bool StartIsolating(string archipelagoFolder, string customWorlds, string old_customWorlds, string worlds, string old_worlds, out Cache_BackupList cache)
        {
            cache = LoadCache<Cache_BackupList>(GetSaveLocation(FileSettings.backupList));
            if (cache.apworldList.ContainsKey(archipelagoFolder))
            {
                return true;
            }

            cache.apworldList[archipelagoFolder] = new List<string>();

            if (!SetUpMinimumWorlds())
            {
                return false;
            }

            if (!Directory.Exists(customWorlds) || !Directory.Exists(worlds))
            {
                ErrorManager.AddNewError(
                    "IOManager_FileExtra - IsolateApworlds failed to create directories",
                    "While trying to isolate the relevant apworlds, IOManager somehow failed to create customWorlds and/or lib/worlds. Please report this issue, you also should clean up the folder yourself (the old, pre-isolate folders are just renamed as old.name)"
                    );
                return false;
            }

            if (Directory.Exists(old_customWorlds) || Directory.Exists(old_worlds))
            {
                ErrorManager.AddNewError(
                    "IOManager_FileExtra - IsolateApworlds already running",
                    "Another launcher is waiting to automatically restore the apworld directories. This is not allowed. Please wait for the auto-restore, or if this is caused by another bug, restore them manually."
                    );
                return false;
            }

            MoveFile(customWorlds, old_customWorlds);
            MoveFile(worlds, old_worlds);

            Directory.CreateDirectory(customWorlds);

            CopyFolder(Path.Combine(GetSaveLocation(MinimumWorlds)), worlds);
            return true;
        }

        public static bool IsolateApworlds(string archipelago, List<string> apworlds)
        {
            string ArchipelagoFolder;
            string customWorlds;
            string old_customWorlds;
            string worlds;
            string old_worlds;
            try
            {
                ArchipelagoFolder = Path.GetDirectoryName(archipelago);

                customWorlds = Path.Combine(ArchipelagoFolder, "custom_worlds");
                old_customWorlds = Path.Combine(ArchipelagoFolder, "custom_worlds_old");

                worlds = Path.Combine(ArchipelagoFolder, "lib", "worlds");
                old_worlds = Path.Combine(ArchipelagoFolder, "lib", "worlds_old");
            }
            catch (System.Exception e)
            {
                ErrorManager.AddNewError(
                    "IOManager_FileExtra - IsolateApworlds threw an exception",
                    "Trying to get the Archipelago folder raised the following exception : " + e.Message
                    );
                return false;
            }

            Cache_BackupList cache;
            StartIsolating(ArchipelagoFolder, customWorlds, old_customWorlds, worlds, old_worlds, out cache);

            string ID = ProcessManager.GetProcessUniqueId();
            cache.launcherToApworldList[ID] = new List<string>();
            foreach (var item in apworlds)
            {
                if (!cache.launcherToApworldList[ID].Contains(item))
                {
                    cache.launcherToApworldList[ID].Add(item);
                }
                
                string fileName = Path.GetFileName(item);
                if (cache.apworldList[ArchipelagoFolder].Contains(item))
                {
                    continue;
                }


                if(File.Exists(Path.Combine(old_customWorlds, fileName)))
                {
                    if(!File.Exists(Path.Combine(customWorlds, fileName)))
                    {
                        CopyFile(Path.Combine(old_customWorlds, fileName), Path.Combine(customWorlds, fileName));
                    }
                    cache.apworldList[ArchipelagoFolder].Add(item);
                    continue;
                }

                if (File.Exists(Path.Combine(old_worlds, fileName)))
                {
                    if (!File.Exists(Path.Combine(worlds, fileName)))
                    {
                        CopyFile(Path.Combine(old_worlds, fileName), Path.Combine(worlds, fileName));
                    }
                    cache.apworldList[ArchipelagoFolder].Add(item);
                    continue;
                }

                if (Directory.Exists(Path.Combine(old_customWorlds, fileName)))
                {
                    if (!Directory.Exists(Path.Combine(customWorlds, fileName)))
                    {
                        CopyFolder(Path.Combine(old_customWorlds, fileName), Path.Combine(customWorlds, fileName));
                    }
                    cache.apworldList[ArchipelagoFolder].Add(item);
                    continue;
                }

                if (Directory.Exists(Path.Combine(old_worlds, fileName)))
                {
                    if (!Directory.Exists(Path.Combine(worlds, fileName)))
                    {
                        CopyFolder(Path.Combine(old_worlds, fileName), Path.Combine(worlds, fileName));
                    }
                    cache.apworldList[ArchipelagoFolder].Add(item);
                    continue;
                }

                ErrorManager.AddNewError(
                    "IOManager_FileExtra - Apworld not found",
                    "You've asked this launcher to isolate " + fileName + " but this file doesn't appear to exist in either customWorlds or lib/world"
                    );
                SaveCache<Cache_BackupList>(GetSaveLocation(FileSettings.backupList), cache);
                return false;
            }
            SaveCache<Cache_BackupList>(GetSaveLocation(FileSettings.backupList), cache);
            return true;
        }

        public static bool RestoreApworlds()
        {
            Cache_BackupList cache = LoadCache<Cache_BackupList>(GetSaveLocation(FileSettings.backupList));

            foreach (var item in cache.apworldList)
            {
                if (!RestoreApworlds(item.Key))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool RestoreApworlds(string archipelago, List<string> apworlds)
        {
            Cache_BackupList cache = LoadCache<Cache_BackupList>(GetSaveLocation(FileSettings.backupList));
            string archipelagoFolder = Path.GetDirectoryName(archipelago);
            if (!cache.apworldList.ContainsKey(archipelagoFolder))
            {
                ErrorManager.AddNewError(
                    "IOManager_Backup - Tried to restore apworlds in an instant that hasn't isolated them",
                    "Some Isolate command tried to restore apworlds for the archipelago instance at " + archipelago + " but the list of apworlds backed up for this installation is null. This shouldn't ever happen, please report this."
                    );
                return false;
            }
            string ID = ProcessManager.GetProcessUniqueId();

            foreach (var item in apworlds)
            {
                bool canBeRemoved = true;
                foreach (var list in cache.launcherToApworldList)
                {
                    if (list.Key != ID && list.Value.Contains(item))
                    {
                        canBeRemoved = false;
                        break;
                    }
                }
                if (canBeRemoved)
                {
                    cache.apworldList[archipelagoFolder].Remove(item);
                }
            }

            cache.launcherToApworldList.Remove(ID);
            SaveCache<Cache_BackupList>(GetSaveLocation(FileSettings.backupList), cache);

            if (cache.apworldList[archipelagoFolder].Count == 0)
            {
                return RestoreApworlds(archipelagoFolder);
            }

            return true;
        }

        public static bool RestoreApworlds(string archipelagoFolder)
        {
            Cache_BackupList cache = LoadCache<Cache_BackupList>(GetSaveLocation(FileSettings.backupList));
            string customWorlds;
            string old_customWorlds;
            string worlds;
            string old_worlds;
            try
            {
                customWorlds = Path.Combine(archipelagoFolder, "custom_worlds");
                old_customWorlds = Path.Combine(archipelagoFolder, "custom_worlds_old");

                worlds = Path.Combine(archipelagoFolder, "lib", "worlds");
                old_worlds = Path.Combine(archipelagoFolder, "lib", "worlds_old");
            }
            catch (System.Exception e)
            {
                ErrorManager.AddNewError(
                    "IOManager_FileExtra - IsolateApworlds threw an exception",
                    "Trying to get the Archipelago folder raised the following exception : " + e.Message
                    );
                return false;
            }

            if (!Directory.Exists(customWorlds) || !Directory.Exists(worlds))
            {
                ErrorManager.AddNewError(
                    "IOManager_FileExtra - Restore apworlds couldn't find apworld directories",
                    "The temporary customWorlds or lib/worlds created by the isolate apworld function doesn't appear to exist, this shouldn't ever happen. If you've not caused this yourself, please report this issue."
                    );
                return false;
            }

            if (!Directory.Exists(old_customWorlds) || !Directory.Exists(old_worlds))
            {
                ErrorManager.AddNewError(
                    "IOManager_FileExtra - Restore apworlds couldn't find apworld backups",
                    "The renamed customWorlds or lib/worlds created by the isolate apworld function doesn't appear to exist, this shouldn't ever happen. If you've not caused this yourself, please report this issue."
                    );
                return false;
            }

            bool hasClearedCustom = false;

            if (!Directory.Exists(customWorlds) || HardDeleteFile(customWorlds)) 
            {
                hasClearedCustom = true;
            }

            bool hasClearedWorlds = false;

            if (!Directory.Exists(worlds) || HardDeleteFile(worlds))
            {
                hasClearedWorlds = true;
            }

            if(hasClearedCustom && hasClearedWorlds)
            {
                // both temporary folders have been taken out, we can now restore the old ones
                if(MoveFile(old_worlds, worlds) && MoveFile(old_customWorlds, customWorlds))
                {
                    cache.apworldList.Remove(archipelagoFolder);
                    SaveCache<Cache_BackupList>(GetSaveLocation(FileSettings.backupList), cache);
                    return true;
                }
            }

            ErrorManager.AddNewError(
                "IOManager_FileExtra - Couldn't restore apworlds",
                "Trying to restore custom_worlds and/or lib/worlds failed. Please see other errors for more informations. The original ones are still there, they've just been renamed, please restore them manually."
                );

            return false;
        }
    }
}
