using YAAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using static YAAL.FileSettings;
using static YAAL.GeneralSettings;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace YAAL
{

    public static partial class IOManager
    {
        public static Cache_UserSettings settings;
        public static List<string> games;
        static Dictionary<string, CustomLauncher> launcherCache = new Dictionary<string, CustomLauncher>();
        static Cache_LauncherList launcherList = new Cache_LauncherList();

        static IOManager()
        {
            string baseDirectory = AppContext.BaseDirectory;
            string file = Path.Combine(baseDirectory, userSettings.GetFileName());
            settings = new Cache_UserSettings();
            settings = LoadCache<Cache_UserSettings>(Path.Combine(AppContext.BaseDirectory, userSettings.GetFileName()));
            settings.SetDefaultPath();
            settings.SetDefaultSettings();
            UpdateLauncherList();
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

            string path = settings.saveLocation[key];

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            return path;
        }

        public static string GetApFolder()
        {
            return Path.GetDirectoryName(settings[aplauncher]);
        }

        public static bool SetUpMinimumWorlds()
        {
            string targetFolder = Path.Combine(GetSaveLocation(ManagedApworlds), MinimumWorlds.GetFolderName());
            if (Directory.Exists(targetFolder))
            {
                return true;
            }

            Directory.CreateDirectory(targetFolder);

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
                && CopyFile(Path.Combine(worldsFolder, "LauncherComponents.pyc"), Path.Combine(targetFolder, "LauncherComponents.pyc"));
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
                Debug.WriteLine(name.ToString());
                throw;
            }    
        }

        public static Cache_Theme GetGeneralTheme()
        {
            return settings.generalTheme;
        }

        public static void SetGeneralTheme(Cache_Theme newTheme)
        {
            settings.generalTheme = newTheme;
            App.Settings.SetTheme("General Theme", newTheme);
            SaveCache<Cache_UserSettings>(Path.Combine(AppContext.BaseDirectory, userSettings.GetFileName()), settings);
        }
    }
}