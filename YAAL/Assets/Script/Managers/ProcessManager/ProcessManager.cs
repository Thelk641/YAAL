using Mono.Unix;
using ShellLink;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace YAAL
{
    public static class ProcessManager
    {
        public static List<Cache_Process> keyedProcesses = new List<Cache_Process>();
        public static Process StartProcess(string path, string args, bool autoStart = true, bool redirectOutput = true)
        {
            Process process = new Process();

            // .lnk issue, problem is, needs to be resolved for linux and OSX as well
            string truePath = path;

            if (Path.GetExtension(path).ToLowerInvariant() == ".lnk")
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    truePath = ResolveShortcut(path);
                } else
                {
                    ErrorManager.AddNewError(
                        "ProcessManager - .lnk files on non-Windows OS",
                        "StartProcess was given a file with a .lnk extension, but those are Windows-only. I couldn't find an easy way to process them, please don't use them on non-Windows OS."
                        );
                    return null;
                }
                
            }

            try
            {
                if (IsExecutable(truePath))
                {
                    // This is an executable, we need to start it and 
                    // read the output
                    process.StartInfo.FileName = truePath;
                    process.StartInfo.Arguments = args;
                    process.StartInfo.UseShellExecute = !redirectOutput;
                    process.StartInfo.RedirectStandardOutput = redirectOutput;
                    process.StartInfo.RedirectStandardError = redirectOutput;
                    process.EnableRaisingEvents = true;
                }
                else
                {
                    // This is not an executable, let's let the OS
                    // deal with it
                    process.StartInfo.FileName = truePath;
                    process.StartInfo.Arguments = args;
                    process.StartInfo.UseShellExecute = true;
                }

                Trace.WriteLine($"Running: {truePath} {args}");

                // We might want to setup the process, but start it at a later time,
                // when we're sure everything is setup (see KeyedProcess)
                if (autoStart)
                {
                    process.Start();
                }

                return process;
            }
            catch (Exception e)
            {
                ErrorManager.ThrowError(
                    "ProcessManager - Failed to create process",
                    "Trying to create a process raised the following exception : " + e.Message);
                return null;
            }
        }

        public static Cache_Process StartKeyedProcess(string path, string args, bool redirectOutput)
        {
            Cache_Process process = new Cache_Process(StartProcess(path, args, false, redirectOutput));
            process.redirectOutput = redirectOutput;
            if(process == null || process.GetProcess() == null)
            {
                ErrorManager.AddNewError(
                    "ProcessManager - Failed to start keyed process",
                    "Tried to start process " + path + " with args " + args + " but it failed. See other errors for more information."
                    );
                return null;
            }

            keyedProcesses.Add(process);
            return process;
        }

        public static string? ResolveShortcut(string path)
        {
            // this is pure chatgpt, no idea what it does
            try
            {
                Shortcut lnk = Shortcut.ReadFromFile(path);

                string? basePath = Path.GetDirectoryName(path);
                string? target = lnk.LinkInfo?.LocalBasePath;

                // Fallback if LocalBasePath is missing
                if (string.IsNullOrWhiteSpace(target))
                    target = lnk.StringData?.RelativePath;

                // If still empty, try WorkingDir
                if (string.IsNullOrWhiteSpace(target))
                    target = lnk.StringData?.WorkingDir;

                // Expand any environment variables
                target = Environment.ExpandEnvironmentVariables(target ?? "");

                // Convert relative to full path if needed
                if (!Path.IsPathRooted(target) && basePath != null)
                    target = Path.GetFullPath(Path.Combine(basePath, target));

                return File.Exists(target) || Directory.Exists(target) ? target : null;
            }
            catch (Exception ex)
            {
                ErrorManager.AddNewError(
                    "ProcessManager - Failed to process .lnk",
                    "The function supposed to deal with .lnk files raised the following exception : " + ex.Message
                    );
                return path;
            }
        }

        public static bool IsExecutable(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string ext = Path.GetExtension(path).ToLowerInvariant();
                return ext == ".exe" || ext == ".bat" || ext == ".cmd" || ext == ".com";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                try
                {
                    var fileInfo = new UnixFileInfo(path);
                    return fileInfo.FileAccessPermissions.HasFlag(FileAccessPermissions.UserExecute);
                }
                catch
                {
                    return false;
                }
            } else
            {
                ErrorManager.AddNewError(
                    "ProcessManager - Non supported platform",
                    "The platform was detected as neither Windows, Linux or OSX. Good luck."
                    );
                return false;
            }
        }

        public static string GetProcessUniqueId()
        {
            var process = Process.GetCurrentProcess();
            return $"{process.Id}-{process.StartTime.ToFileTimeUtc()}";
        }
    }
}
