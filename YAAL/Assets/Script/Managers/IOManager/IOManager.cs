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

    public static partial class IOManager
    {
        #if DEBUG
                private static string baseDirectory = "I:\\Emulators\\vba\\rom\\OOS rando\\YAAL - dev";
        #else
                private static string baseDirectory = AppContext.BaseDirectory;
        #endif

        //private static string baseDirectory = AppContext.BaseDirectory;
        public static Cache_UserSettings settings;
        public static List<string> games;
        static Dictionary<string, CustomLauncher> launcherCache = new Dictionary<string, CustomLauncher>();
        static Cache_LauncherList launcherList = new Cache_LauncherList();

        static IOManager()
        {
            settings = LoadCache<Cache_UserSettings>(userSettings.GetFullPath());

            if (settings.generalSettings.ContainsKey(zoom) && double.TryParse(settings.generalSettings[zoom], out double newZoom))
            {
                App.Settings.Zoom = newZoom;
            }

            if(settings.saveLocation.Count == 0)
            {
                settings.SetDefaultPath();
                SaveCache<Cache_UserSettings>(userSettings.GetFullPath(), settings);
            }
        }

        public static string GetSaveLocation(FileSettings key)
        {
            if (!settings.saveLocation.ContainsKey(key))
            {
                ErrorManager.ThrowError(
                    "IOManager - Save location doesn't exist",
                    "The code tried to get the save location named " + key.ToString() + " but it doesn't seem to exist ?"
                    );
            }

            string path = ProcessLocalPath(settings.saveLocation[key]);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            return path;
        }

        public static string GetApFolder()
        {
            return Path.GetDirectoryName(settings[aplauncher]);
        }

        public static bool SetUpMinimumWorlds()
        {
            string targetFolder = GetSaveLocation(MinimumWorlds);
            if (Directory.Exists(targetFolder))
            {
                return true;
            }

            Directory.CreateDirectory(targetFolder);

            DirectoryInfo dir = new DirectoryInfo(targetFolder);
            dir.Attributes |= FileAttributes.Hidden;

            string archipelagoFolder = GetApFolder();

            if (!Directory.Exists(archipelagoFolder))
            {
                ErrorManager.AddNewError(
                    "IOManager - Archipelago folder doesn't exists", 
                    "Your ArchipelagoLauncher.exe is at : " + settings[aplauncher] + " sadly, the folder containing this file doesn't appear to exist."
                    );
                return false;
            }
            string worldsFolder = Path.Combine(archipelagoFolder, "lib", "worlds");
            return
                CopyFolder(Path.Combine(worldsFolder, "_bizhawk"), Path.Combine(targetFolder, "_bizhawk"))
                && CopyFolder(Path.Combine(worldsFolder, "generic"), Path.Combine(targetFolder, "generic"))
                && CopyFile(Path.Combine(worldsFolder, "__init__.pyc"), Path.Combine(targetFolder, "__init__.pyc"))
                && CopyFile(Path.Combine(worldsFolder, "AutoSNIClient.pyc"), Path.Combine(targetFolder, "AutoSNIClient.pyc"))
                && CopyFile(Path.Combine(worldsFolder, "AutoWorld.pyc"), Path.Combine(targetFolder, "AutoWorld.pyc"))
                && CopyFile(Path.Combine(worldsFolder, "Files.pyc"), Path.Combine(targetFolder, "Files.pyc"))
                && CopyFile(Path.Combine(worldsFolder, "LauncherComponents.pyc"), Path.Combine(targetFolder, "LauncherComponents.pyc"))
                && CopyFile(Path.Combine(worldsFolder, "smw.apworld"), Path.Combine(targetFolder, "smw.apworld"));
        }

        public static string CleanUpPath(string originalPath)
        {
            try
            {
                string output = originalPath.Trim().Trim('"').Trim();
                FileInfo fi = new FileInfo(output);
                Path.GetFullPath(output);
                return output;
            } catch (Exception e)
            {
                ErrorManager.AddNewError(
                "IOManager - Couldn't clean path",
                "Tried to clean path : " + originalPath + ", ended with the following exception :" + e.Message
                );

                return originalPath;
            }
        }

        public static List<string> SplitPathList(string originalPath)
        {
            List<string> output = new List<string>();
            string[] list = originalPath.Split(';');
            foreach (var item in list)
            {
                // If we've had something finishing in ;\" we might have an
                // item of length 2, let's just ignore them
                if (item.Length > 2) {
                    output.Add(item);
                }
            }
            return output;
        }

        public static void SetSetting(GeneralSettings name, string value)
        {
            settings.generalSettings[name] = value;
            if(name == backgroundColor
                || name == foregroundColor
                || name == buttonColor)
            {
                ThemeManager.UpdateGeneralTheme();
            }
        }

        public static string GetSetting(GeneralSettings name)
        {
            try
            {
                if (settings.generalSettings.ContainsKey(name))
                {
                    return settings[name];
                }
                return "";
            }
            catch (Exception)
            {
                Trace.WriteLine(name.ToString());
                throw;
            }    
        }

        public static string ProcessLocalPath(string originalPath)
        {
            try
            {
                if (originalPath.StartsWith("./"))
                {
                    string relativePath = originalPath.Substring(2);
                    string fullPath = Path.GetFullPath(relativePath, baseDirectory);
                    return fullPath;
                }
                else
                {
                    return originalPath;
                }
            }
            catch (Exception e)
            {
                ErrorManager.ThrowError(
                        "IOManager - Failed to parse local path",
                        "Trying to parse local path raised the following exception : " + e.Message);
                return originalPath;
            }
        }

        public static string ToLocalPath(string originalPath)
        {
            string relativePath = Path.GetRelativePath(baseDirectory, Path.GetFullPath(originalPath));

            if(Path.IsPathRooted(relativePath) && !relativePath.StartsWith(".")){
                return originalPath;
            }

            return "./" + relativePath.Replace("\\", "/");
        }

        public static string ToDebug(string archipelago)
        {
            string? directory = Path.GetDirectoryName(archipelago);
            if (directory != null)
            {
                return Path.Combine(directory, "ArchipelagoLauncherDebug.exe");
            } else
            {
                return archipelago;
            }
        }
    }
}