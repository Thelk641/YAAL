using YAAL.Assets.Script.Cache;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static YAAL.FileSettings;
using static YAAL.GeneralSettings;

namespace YAAL
{
    public static partial class IOManager
    {
        public static string FindAvailableDirectoryName(string origin, string baseName)
        {
            if (!Directory.Exists(Path.Combine(origin, baseName)))
            {
                return baseName;
            }
            int i = 0;
            string output;

            do
            {
                ++i;
                output = baseName + "(" + i + ")";
            } while (Directory.Exists(Path.Combine(origin,output)));

            return output;
        }

        public static string FindAvailableLauncherName(string gameName)
        {
            return FindAvailableDirectoryName(GetSaveLocation(ManagedApworlds), gameName);
        }

        public static string FindAvailableFileName(string directory, string baseName)
        {
            string output = Path.Combine(directory, baseName);
            int i = 0;

            while (File.Exists(output))
            {
                ++i;
                output = Path.Combine(directory, ("(" + i + ")" + baseName));
            }

            return output;

        }

        public static bool CopyToDefault(string gameName, string path, out string output)
        {
            string defaultFolder = Path.Combine(GetSaveLocation(ManagedApworlds), gameName, "Default");
            Directory.CreateDirectory(defaultFolder);
            output = Path.Combine(defaultFolder, GetFileName(path));

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
                return CopyFile(path, output);
            }

            if (Directory.Exists(path))
            {
                return CopyFolder(path, output);
            }

            ErrorManager.AddNewError(
                "IOManager_FileExtra - Target default doesn't exist",
                "Tried to copy " + path + "to the default directory, but this file or folder doesn't seem to exist."
                );
            return false;
        }

        public static string GetArchipelagoVersion()
        {
            string output = "";
            if (File.Exists(settings[aplauncher]))
            {
                string manifestPath = Path.Combine(GetApFolder(), "manifest.json");
                //open manifest.json, grab the version, put it in output

                if (File.Exists(manifestPath))
                {
                    // Read and parse JSON
                    string json = File.ReadAllText(manifestPath);
                    var manifest = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                    if (manifest != null && manifest.TryGetValue("version", out object versionObj) && versionObj is JArray versionArray)
                    {
                        output = string.Join(".", versionArray.Select(v => v.ToString()));
                    }
                }
            }
            return output;
        }

        public static bool UpdateFileToVersion(string path, string launcherName, string version, string isNecessary)
        {
            if(path == "")
            {
                ErrorManager.AddNewError(
                    "IOManager_FileExtra - Empty path",
                    "Something tried to update a file version, but didn't set a path for said file."
                    );
                return false;
            }

            if(version == "")
            {
                ErrorManager.AddNewError(
                    "IOManager_FileExtra - No version selected",
                    "This particular slot doesn't have a version selected, please set one."
                    );
                return false;
            }

            string fileName;
            
            path = CleanUpPath(path);
            if (path.EndsWith("\\"))
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                fileName = dir.Name;
                
            } else
            {
                fileName = Path.GetFileName(path);
            }

            if(fileName == "YAAL.apworld")
            {
                // This is a meta apworld, it's not supposed to be in this ManagedApworlds folder
                return true;
            }

            string versionFile = Path.Combine(GetSaveLocation(ManagedApworlds), launcherName, version, fileName);

            if(!File.Exists(versionFile))
            {
                // Our download folder for this version doesn't contain filename
                // if we do need the file, error out
                // if we don't, just ignore this and move on
                if(isNecessary == true.ToString())
                {
                    ErrorManager.AddNewError(
                        "IOManager_FileExtra - Missing mandatory apworld",
                        "This launcher requires the following file, which doesn't seem to exists for the selected version : " + fileName
                        );
                    return false;
                } else
                {
                    return true;
                }
            }

            HardDeleteFile(path);
            return CopyFile(versionFile, path);
        }

        public static bool UpdatePatch(string launcherName, string path, Cache_PreviousSlot newSlot)
        {
            string cachePath = Path.Combine(GetSaveLocation(ManagedApworlds), launcherName, previous_async.GetFileName());
            Cache_PreviousSlot oldSlot = LoadCache<Cache_PreviousSlot>(cachePath);

            if (oldSlot.previousAsync == newSlot.previousAsync 
                && oldSlot.previousSlot == newSlot.previousSlot 
                && oldSlot.previousPatch == newSlot.previousPatch)
            {
                // We've started the same slot as last time, nothing else to do
                return true;
            }

            if(oldSlot.previousPatch != null && oldSlot.previousPatch != "")
            {
                string oldPatch = Path.Combine(path, GetFileName(oldSlot.previousPatch));

                if (File.Exists(oldPatch))
                {
                    HardDeleteFile(oldPatch);
                }
            }

            if (CopyFile(newSlot.previousPatch, Path.Combine(path, GetFileName(newSlot.previousPatch))))
            {
                return true;
            } else
            {
                ErrorManager.AddNewError(
                    "IOManager_FileExtra - Failed to update patch",
                    "Failed to copy " + newSlot.previousPatch + " to " + cachePath
                    );
                return false;
            }
        }

        public static string MoveToSlotDirectory(string fileToMove, string asyncName, string slotName, string newName)
        {
            string workingName = "";

            if (newName == "")
            {
                workingName = GetFileName(fileToMove);
                
            } else
            {
                workingName = newName + Path.GetExtension(GetFileName(fileToMove));
            }


            string output = Path.Combine(
                GetSaveLocation(Async),
                asyncName,
                slotName,
                workingName
                );

            if(output == fileToMove || File.Exists(output))
            {
                // We've already moved this file to the right directory in the past
                return fileToMove;
            }

            if (MoveFile(fileToMove, output))
            {
                return output;
            } else
            {
                ErrorManager.AddNewError(
                "IOManager_FileExtra - Failed to move file to working directory",
                "Couldn't move file " + fileToMove + " to " + output
                );
                return "";
            }

            
        }
    }
}
