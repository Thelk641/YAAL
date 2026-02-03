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

    public static partial class FileManager
    {
#if DEBUG
                private static string baseDirectory = "I:\\Emulators\\vba\\rom\\OOS rando\\YAAL - dev";
#else
                private static string baseDirectory = AppContext.BaseDirectory;
#endif

        public static bool CopyFile(string originalFile, string destinationPath)
        {
            //destinationPath must include file name !
            if (File.Exists(destinationPath))
            {
                // we are never, ever, overriding a file in this function
                ErrorManager.AddNewError(
                    "FileManager - Copy can't overwrite",
                    "CopyFile was asked to copy a file to " + destinationPath + " but that file already exists. CopyFile is banned from overwriting for security reasons.");
                return false;
            }

            try
            {
                File.Copy(originalFile, destinationPath);

                return true;
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                    "FileManager - Copy threw an exception",
                    "Tried to copy a file to " + destinationPath + " but it raised the following exception : " + e.Message
                    );
                return false;
            }

        }

        public static bool CopyFolder(string sourceDir, string destinationDir, bool recursive = true)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
            {
                ErrorManager.AddNewError(
                    "FileManager - Folder doesn't exist",
                    "Tried to copy the following folder : " + dir + " but it doesn't seem to exist."
                    );
                return false;
            }

            // Create the destination directory if it doesn't exist
            Directory.CreateDirectory(destinationDir);

            // Copy all the files
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, overwrite: true);
            }

            // Copy all subdirectories
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    if (!CopyFolder(subDir.FullName, newDestinationDir, true))
                    {
                        ErrorManager.AddNewError(
                            "FileManager - SubFolder copy failed",
                            "Tried to copy subfolder " + subDir.FullName + " to target " + newDestinationDir + " but it failed for some reasons."
                            );
                        return false;
                    }
                }
            }
            return true;
        }

        public static string GetBaseDirectory()
        {
            return baseDirectory;
        }

        public static bool HardDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }

                return true;
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                    "FileManager - Hard delete threw an exception",
                    "Tried to delete file or folder at " + path + "but it raised the following exception : " + e.Message
                    );
                return false;
            }
        }

        public static string LoadFile(string path)
        {
            if (!File.Exists(path))
            {
                return "";
            }
            else return File.ReadAllText(path);
        }

        public static bool MoveFile(string origin, string target)
        {
            //tries to move origin to target, returns how successfull it was
            try
            {
                if (origin == "" || target == "")
                {
                    ErrorManager.AddNewError(
                        "FileManager - Origin or Target is empty",
                        "Something just tried to move nothing somewhere, something nowhere, or nothing nowhere. If your settings appear fine, please report this issue.");
                    return false;
                }


                if (Directory.Exists(origin))
                {
                    // we're attempting to move an entire folder
                    return SafeMove(origin, target);
                }

                if (File.Exists(origin))
                {
                    // we're attempting to move a single file
                    if (Directory.Exists(target))
                    {
                        // target is the folder that will contain the file
                        string finalTarget = Path.Combine(target, Path.GetFileName(origin));
                        return SafeMove(origin, finalTarget);
                    }
                    else
                    {
                        // target is the complete path, with the file name included
                        return SafeMove(origin, target);
                    }
                }
                ErrorManager.AddNewError(
                    "FileManager - File doesn't exists",
                    "MoveFile tried to move this : " + origin + " sadly, this file or folder doesn't appear to exist.");
                return false;
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                    "FileManager - MoveFile threw an exception",
                    "MoveFile tried to move this : " + origin + " but this raised the following exception : " + e.Message);
                return false;
            }
        }

        private static bool SafeMove(string origin, string destination)
        {
            try
            {
                if (Path.GetPathRoot(origin).Equals(Path.GetPathRoot(destination), StringComparison.OrdinalIgnoreCase))
                {
                    if (File.Exists(origin))
                    {
                        File.Move(origin, destination);
                    }
                    else
                    {
                        Directory.Move(origin, destination);
                    }

                }
                else
                {
                    if (File.Exists(origin))
                    {
                        File.Copy(origin, destination);
                        File.Delete(origin);
                    }
                    else
                    {
                        CopyFolder(origin, destination);
                        Directory.Delete(origin, true);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                    "FileManager - MoveFile threw an exception",
                    "MoveFile tried to move this : " + origin + " but this raised the following exception : " + e.Message);
                return false;
            }
        }

        public static bool SaveFile(string path, string file)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, file);
                return true;
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                    "FileManager - Saving threw an exception",
                    "Trying to save file " + path + " threw the following exception : " + e.Message
                    );
                return false;
            }
        }

        public static bool SaveFile(string path, byte[] file)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, file);
                return true;
            }
            catch (Exception e)
            {
                ErrorManager.AddNewError(
                    "FileManager - Saving threw an exception",
                    "Trying to save file " + path + " threw the following exception : " + e.Message
                    );
                return false;
            }
        }

        public static bool SoftDeleteFile(string path)
        {
            string fileName;
            string originalFileName = Path.GetFileName(path);
            string binDirectory = Path.Combine(SettingsManager.GetSaveLocation(Trash), DateTime.Now.ToString("dd-MM-yyyy-HH-mm"));
            Directory.CreateDirectory(binDirectory);
            if (File.Exists(path))
            {
                fileName = IO_Tools.FindAvailableFileName(binDirectory, originalFileName);
            }
            else
            {
                fileName = IO_Tools.FindAvailableDirectoryName(binDirectory, originalFileName);
            }

            string bin = Path.Combine(binDirectory, fileName);
            return MoveFile(path, bin);
        }

        public static bool SoftDeleteFolder(string path)
        {
            string fileName = Path.GetFileName(path);
            string binDirectory = Path.Combine(SettingsManager.GetSaveLocation(Trash), DateTime.Now.ToString("dd-MM-yyyy-HH-mm"));
            Directory.CreateDirectory(binDirectory);
            if (Directory.Exists(path))
            {
                string trueName = IO_Tools.FindAvailableDirectoryName(binDirectory, fileName);
                return MoveFile(path, Path.Combine(binDirectory, trueName));
            }
            else
            {
                return false;
            }
        }
    }
}