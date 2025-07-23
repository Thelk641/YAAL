using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using YAAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YAAL
{

    public static partial class IOManager
    {
        public static async Task<string> PickFile(Window window)
        {
            if (window == null)
            {
                ErrorManager.ThrowError(
                    "IOManager_UI - Unable to find the parent window",
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
                    "IOManager_UI - Unable to find the parent window",
                    "The function PickFolder is unable to find its parent window. Please report this issue.");
                return "";
            }
            var tcs = new TaskCompletionSource<string>();
            FileFolderPicker picker = new FileFolderPicker(window);

            await picker.PickFolder(path => tcs.SetResult(path));

            return await tcs.Task;
        }
    }
}