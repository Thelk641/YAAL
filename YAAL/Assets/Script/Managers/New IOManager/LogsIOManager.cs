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
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YAAL;
using YAAL.Assets.Script.Cache;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static YAAL.FileSettings;
using static YAAL.GeneralSettings;
using static YAAL.LauncherSettings;

namespace YAAL
{

    public static class LogsIOManager
    {
        public static void SaveCacheLogs(string toSave)
        {
            string logDirectory = SettingsManager.GetSaveLocation(Logs);
            Directory.CreateDirectory(logDirectory);
            string savePath = Path.Combine(logDirectory, ("Debug log " + IO_Tools.GetTime() + ".txt"));
            File.WriteAllText(savePath, toSave);
        }

        public static string SaveCacheError(Cache_ErrorList error)
        {
            string logDirectory = SettingsManager.GetSaveLocation(Logs);
            Directory.CreateDirectory(logDirectory);
            string savePath = Path.Combine(logDirectory, ("Error log " + IO_Tools.GetTime() + ".txt"));

            string readableLog = "";

            foreach (var item in error.errors)
            {
                readableLog += $@"
                === ERROR: {item.name} ===

                {item.content}

                Stack Trace:
                {item.stackTrace}

                ===========================

                ";
            }

            readableLog = string.Join(Environment.NewLine,
                            readableLog.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                                       .Select(line => line.TrimEnd())       // remove trailing space
                                       .Select(line => line.TrimStart())     // remove leading space
                                       .Select(line => line.StartsWith("at ") ? "\t" + line : line)
                            );


            FileManager.SaveFile(savePath, readableLog);
            return Path.GetFullPath(savePath);
        }

        public static void ReadCacheError(string path)
        {
            string json = "";
            try
            {
                json = FileManager.LoadFile(path.Replace("\\\\", "\\").Trim('\"'));
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                    "Failed to find error file",
                    "Trying to display error threw the following exception : " + e.Message);
                return;
            }

            Cache_ErrorList output = new Cache_ErrorList();
            var pattern = @"=== ERROR: (?<name>.+?) ===\r?\n\r?\n(?<content>.*?)\r?\n\r?\nStack Trace:\r?\n(?<stack>.*?)\r?\n\r?\n=+";
            var matches = Regex.Matches(json, pattern, RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                ErrorManager.AddNewError(
                    match.Groups["name"].Value.Trim(),
                    match.Groups["content"].Value.Trim(),
                    match.Groups["stack"].Value.Trim());
            }
        }
    }
}