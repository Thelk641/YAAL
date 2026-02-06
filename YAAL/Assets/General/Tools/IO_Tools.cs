using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using YAAL;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static YAAL.FileSettings;
using static YAAL.GeneralSettings;

namespace YAAL
{

    public static class IO_Tools
    {
        public static string CleanUpPath(string originalPath)
        {
            try
            {
                string output = originalPath.Trim().Trim('"').Trim();
                FileInfo fi = new FileInfo(output);
                Path.GetFullPath(output);
                return output;
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                "IO_Tools - Couldn't clean path",
                "Tried to clean path : " + originalPath + ", ended with the following exception :" + e.Message
                );

                return originalPath;
            }
        }

        public static string FindApworld(string apfolder, string fileName)
        {
            string custom_world = Path.Combine(apfolder, "custom_worlds", fileName);
            string lib_world = Path.Combine(apfolder, "lib", "worlds", fileName);

            if (File.Exists(custom_world))
            {
                return custom_world;
            }
            else if (File.Exists(lib_world))
            {
                return lib_world;
            }

            return fileName;
        }

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
            } while (Directory.Exists(Path.Combine(origin, output)));

            return output;
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

        public static string GetArchipelagoVersion()
        {
            string output = "";
            string apLauncher = SettingsManager.GetSetting(aplauncher);
            if (File.Exists(apLauncher))
            {
                string manifestPath = Path.Combine(Path.GetDirectoryName(apLauncher), "manifest.json");
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

        public static string GetFileName(string path)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                return dir.Name;
            }
            catch (Exception)
            {
                ErrorManager.AddNewError(
                    "IO_Tools - Tried to find name of path null",
                    "Something sent a null value to GetFileName(path), this shouldn't ever happen. Please report the issue."
                    );
                return "";
            }
        }

        public static string? GetGameNameFromApworld(string apworldPath)
        {
            using ZipArchive archive = ZipFile.OpenRead(apworldPath);

            // Look for any file that ends with __init__.py
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.Contains("__init__.py"))
                {
                    using StreamReader reader = new StreamReader(entry.Open());
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var match = Regex.Match(line, @"^\s*game\s*=\s*[""'](?<game>[^""']+)[""']\s*$");
                        if (match.Success)
                        {
                            return match.Groups["game"].Value;
                        }
                    }
                }
            }

            return ""; // Not found
        }

        public static string GetTime()
        {
            return DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
        }

        public static async Task<string> PickFile(Window window)
        {
            if (window == null)
            {
                ErrorManager.ThrowError(
                    "IO_Tools - Unable to find the parent window",
                    "The function PickFile is unable to find the main window. Please report this issue.");
                return "";
            }
            var tcs = new TaskCompletionSource<string>();
            FileFolderPicker picker = new FileFolderPicker(window);

            await picker.PickFile(path => tcs.SetResult(path));

            return await tcs.Task;
        }

        public static async Task<string> PickFolder(Window window)
        {
            if (window == null)
            {
                ErrorManager.ThrowError(
                    "IO_Tools - Unable to find the parent window",
                    "The function PickFolder is unable to find its parent window. Please report this issue.");
                return "";
            }
            var tcs = new TaskCompletionSource<string>();
            FileFolderPicker picker = new FileFolderPicker(window);

            await picker.PickFolder(path => tcs.SetResult(path));

            return await tcs.Task;
        }

        public static string ProcessLocalPath(string originalPath)
        {
            try
            {
                if (originalPath.StartsWith("./"))
                {
                    string relativePath = originalPath.Substring(2);
                    string fullPath = Path.GetFullPath(relativePath, FileManager.GetBaseDirectory());
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
                    "IO_Tools - Failed to parse local path",
                    "Trying to parse local path raised the following exception : " + e.Message);
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
                if (item.Length > 2)
                {
                    output.Add(item);
                }
            }
            return output;
        }

        public static string ToLocalPath(string originalPath)
        {
            string relativePath = Path.GetRelativePath(FileManager.GetBaseDirectory(), Path.GetFullPath(originalPath));

            if (Path.IsPathRooted(relativePath) && !relativePath.StartsWith("."))
            {
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
            }
            else
            {
                return archipelago;
            }
        }
    }
}