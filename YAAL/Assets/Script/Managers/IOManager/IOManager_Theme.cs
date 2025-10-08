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
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using ReactiveUI;
using System.Numerics;

namespace YAAL
{

    public static partial class IOManager
    {
        public static Cache_Theme GetGeneralTheme()
        {
            return settings.generalTheme;
        }

        public static void SetGeneralTheme(Cache_Theme newTheme)
        {
            settings.generalTheme = newTheme;
            App.Settings.SetTheme("General Theme", newTheme);
            SaveCache<Cache_UserSettings>(ProcessLocalPath(userSettings.GetFullPath()), settings);
        }

        public static Bitmap? ReadImage(string imageName)
        {
            if (File.Exists(imageName))
            {
                try
                {
                    return new Bitmap(imageName);
                } catch (Exception e)
                {
                    ErrorManager.ThrowError(
                        "IOManager - Failed to read image",
                        "Trying to parse file " + imageName + " raised the following exception : " + e.Message
                        );
                    return null;
                }
            }


            string path = Path.Combine(GetSaveLocation(Images), GetFileName(imageName));
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

        public static Cache_CustomTheme? LoadCustomTheme(string name)
        {
            string path = Path.Combine(GetSaveLocation(Themes), name, customTheme.GetFileName());
            if (File.Exists(path))
            {
                return LoadCache<Cache_CustomTheme>(path);
            }
            return null;
        }

        public static void SaveCustomTheme(Cache_CustomTheme cache)
        {
            string path = Path.Combine(GetSaveLocation(Themes), cache.name, customTheme.GetFileName());
            SaveCache<Cache_CustomTheme>(path, cache);
        }

        public static List<string> GetThemeList()
        {
            string path = GetSaveLocation(Themes);
            return Directory
                .EnumerateFiles(path)
                .Where(file => string.Equals(Path.GetExtension(file), ".json", StringComparison.OrdinalIgnoreCase))
                .Select(file => Path.GetFileNameWithoutExtension(file))
                .ToList();
        }

        public static string CopyImageToDefaultFolder(string path, string themeName)
        {
            if (!File.Exists(path))
            {
                return "";
            }

            string originalPath = ProcessLocalPath(path);
            string targetPath = Path.Combine(GetSaveLocation(Themes), themeName, GetFileName(path));
            Directory.CreateDirectory(Path.Combine(GetSaveLocation(Themes), themeName));

            if(path == targetPath || File.Exists(targetPath))
            {
                return ToLocalPath(targetPath);
            }

            if(CopyFile(path, targetPath))
            {
                return ToLocalPath(targetPath);
            } else
            {
                ErrorManager.AddNewError(
                    "IOManager - Failed to copy image to default folder",
                    "See other errors for more information"
                    );
                ErrorManager.ThrowError();
                return path;
            }
        }

        public static void SaveImage(Bitmap image, string themeName, ThemeSettings category)
        {
            // This version of this function is only there for debug
            string fullname = themeName + "_" + category.ToString() + "_" + image.Size.Width + "_" + image.Size.Height + ".png";
            string path = Path.Combine(GetSaveLocation(Images), fullname);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using var fileStream = File.Create(path);
            image.Save(fileStream);
        }

        public static void SaveImage(Bitmap image, string themeName)
        {
            Vector2 slotSize = WindowManager.GetSlotSize();
            string fullname = themeName + "_" + slotSize.X + "_" + slotSize.Y + ".png";
            string path = Path.Combine(GetSaveLocation(Images), fullname);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using var fileStream = File.Create(path);
            image.Save(fileStream);
        }

        public static Bitmap? GetRender(string themeName)
        {
            Vector2 slotSize = WindowManager.GetSlotSize();
            string fullname = themeName + "_" + slotSize.X + "_" + slotSize.Y + ".png";
            string path = Path.Combine(GetSaveLocation(Images), fullname);

            if (File.Exists(path))
            {
                try
                {
                    return new Bitmap(path);
                }
                catch (Exception e)
                {
                    ErrorManager.ThrowError(
                        "IOManager - Failed to read image",
                        "Trying to parse file " + path + " raised the following exception : " + e.Message
                        );
                    return null;
                }
            }

            return null;
        }
    }
}