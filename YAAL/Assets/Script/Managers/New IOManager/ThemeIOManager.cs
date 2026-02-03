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
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using YAAL;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static YAAL.AsyncSettings;
using static YAAL.FileSettings;
using static YAAL.SlotSettings;

namespace YAAL
{

    public static class ThemeIOManager
    {
        public static string CopyImageToDefaultFolder(string path, string themeName)
        {
            if (!File.Exists(path))
            {
                return "";
            }

            string originalPath = IO_Tools.ProcessLocalPath(path);
            string targetPath = Path.Combine(SettingsManager.GetSaveLocation(Themes), themeName, IO_Tools.GetFileName(path));
            Directory.CreateDirectory(Path.Combine(SettingsManager.GetSaveLocation(Themes), themeName));

            if (path == targetPath || File.Exists(targetPath))
            {
                return IO_Tools.ToLocalPath(targetPath);
            }

            if (FileManager.CopyFile(path, targetPath))
            {
                return IO_Tools.ToLocalPath(targetPath);
            }
            else
            {
                ErrorManager.AddNewError(
                    "ThemeIOManager - Failed to copy image to default folder",
                    "See other errors for more information"
                    );
                ErrorManager.ThrowError();
                return path;
            }
        }

        public static void DeleteTheme(string name)
        {
            string path = Path.Combine(SettingsManager.GetSaveLocation(Themes), name);
            if (Directory.Exists(path))
            {
                FileManager.SoftDeleteFolder(path);
            }
        }

        public static string GetAvailableThemeName(string name)
        {
            return IO_Tools.FindAvailableDirectoryName(SettingsManager.GetSaveLocation(Themes), name);
        }

        public static List<string> GetThemeList()
        {
            string path = SettingsManager.GetSaveLocation(Themes);
            List<string> output = new List<string>();
            foreach (var item in Directory.GetDirectories(path))
            {
                if (File.Exists(Path.Combine(item, "customTheme.json")))
                {
                    output.Add(Path.GetFileName(item));
                }
            }
            return output;
        }

        public static Bitmap? GetRender(Cache_CustomTheme theme, ThemeSettings setting)
        {
            Vector2 slotSize = new Vector2();
            float offset = theme.topOffset + theme.bottomOffset;
            string fullname = "";

            switch (setting)
            {
                case ThemeSettings.backgroundColor:
                    slotSize = WindowManager.GetSlotSize();
                    fullname = theme.name + "_" + "backgroundColor" + "_" + slotSize.X + "_" + (slotSize.Y + offset) + ".png";
                    break;
                case ThemeSettings.foregroundColor:
                    slotSize = WindowManager.GetSlotForegroundSize();
                    fullname = theme.name + "_" + "foregroundColor" + "_" + slotSize.X + "_" + slotSize.Y + ".png";
                    break;
            }

            string path = Path.Combine(SettingsManager.GetSaveLocation(Images), fullname);

            if (File.Exists(path))
            {
                try
                {
                    return new Bitmap(path);
                }
                catch (Exception e)
                {
                    ErrorManager.ThrowError(
                        "ThemeIOManager - Failed to read image",
                        "Trying to parse file " + path + " raised the following exception : " + e.Message
                        );
                    return null;
                }
            }

            return null;
        }

        public static Cache_CustomTheme? LoadCustomTheme(string name)
        {
            string path = Path.Combine(SettingsManager.GetSaveLocation(Themes), name, customTheme.GetFileName());
            if (File.Exists(path))
            {
                return CacheManager.LoadCache<Cache_CustomTheme>(path);
            }
            return null;
        }

        public static Bitmap? ReadImage(string imageName)
        {
            if (File.Exists(imageName))
            {
                try
                {
                    return new Bitmap(imageName);
                }
                catch (Exception e)
                {
                    ErrorManager.ThrowError(
                        "ThemeIOManager - Failed to read image",
                        "Trying to parse file " + imageName + " raised the following exception : " + e.Message
                        );
                    return null;
                }
            }


            string path = Path.Combine(SettingsManager.GetSaveLocation(Images), IO_Tools.GetFileName(imageName));
            if (File.Exists(path))
            {
                try
                {
                    return new Bitmap(path);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

        public static string RenameTheme(string oldName, string newName)
        {
            string oldPath = Path.Combine(SettingsManager.GetSaveLocation(Themes), oldName);
            string newPath = Path.Combine(SettingsManager.GetSaveLocation(Themes), newName);
            if (Directory.Exists(oldPath))
            {
                string trueName = newName;
                if (Directory.Exists(newPath))
                {
                    trueName = IO_Tools.FindAvailableDirectoryName(SettingsManager.GetSaveLocation(Themes), newName);
                }
                try
                {
                    Directory.Move(oldPath, newPath);
                }
                catch (Exception e)
                {
                    ErrorManager.ThrowError(
                        "ThemeIOManager - Failed to move folder",
                        "Trying to move folder at " + oldPath + " to " + newPath + " raised the following exception : " + e.Message
                        );
                    return oldPath;
                }

                return trueName;
            }
            return oldPath;
        }

        public static void SaveCustomTheme(Cache_CustomTheme cache)
        {
            string path = Path.Combine(SettingsManager.GetSaveLocation(Themes), cache.name, customTheme.GetFileName());
            CacheManager.SaveCache<Cache_CustomTheme>(path, cache);
        }

        public static void SaveImage(Bitmap image, string themeName, ThemeSettings category)
        {
            // This version of this function is only there for debug
            string fullname = themeName + "_" + category.ToString() + "_" + image.Size.Width + "_" + image.Size.Height + ".png";
            string path = Path.Combine(SettingsManager.GetSaveLocation(Images), fullname);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Directory.CreateDirectory(SettingsManager.GetSaveLocation(Images));

            using var fileStream = File.Create(path);
            image.Save(fileStream);
        }

        public static void SaveImage(Bitmap image, string themeName)
        {
            Vector2 slotSize = WindowManager.GetSlotSize();
            string fullname = themeName + "_" + slotSize.X + "_" + slotSize.Y + ".png";
            string path = Path.Combine(SettingsManager.GetSaveLocation(Images), fullname);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using var fileStream = File.Create(path);
            image.Save(fileStream);
        }
    }
}