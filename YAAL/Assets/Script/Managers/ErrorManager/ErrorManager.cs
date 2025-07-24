using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YAAL
{
    public static class ErrorManager
    {
        private static Cache_ErrorList cache = new Cache_ErrorList();

        public static void AddNewError(string name, string content)
        {
            AddNewError(name, content, Environment.StackTrace);
        }

        public static void AddNewError(string name, string content, string stackTrace)
        {
            Cache_Error cache_Error = new Cache_Error();
            cache_Error.name = name;
            cache_Error.content = content;
            cache_Error.stackTrace = stackTrace;

            cache.errors.Add(cache_Error);
        }

        public static void ThrowError()
        {
            if(cache.errors.Count == 0)
            {
                return;
            }

            string path = IOManager.SaveCacheError(cache);
            string args = (" --error " + "\"" + path + "\"");
            ProcessManager.StartProcess(Environment.ProcessPath, ("--error " + "\"" + path + "\""), true);
            cache = new Cache_ErrorList();
        }

        public static void ThrowError(string name, string content)
        {
            AddNewError(name, content);
            ThrowError();
        }


        public static void ReadError(string path, IClassicDesktopStyleApplicationLifetime desktop)
        {
            IOManager.ReadCacheError(path);
            ShowError(desktop);
        }

        public static void ShowError(IClassicDesktopStyleApplicationLifetime desktop)
        {
            ErrorWindow errorWindow = new ErrorWindow();
            desktop.MainWindow = errorWindow;
            errorWindow.AddError(cache);
            errorWindow.ShowError();
        }
    }
}
