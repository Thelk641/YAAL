using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAAL
{
    public enum FileSettings
    {
        // lower case, files
        versions,
        userSettings,
        backupList,
        tools,
        previous_async,
        launcher,
        multiworld,
        launcherList,
        customTheme,
        windows,

        // upper case, directories
        ManagedApworlds,
        Async,
        Trash,
        Logs,
        MinimumWorlds,
        Themes,
        Images,
        Rendered,
        Assets,
    };

    public static class FileSettingExtensions
    {
        public static string GetFileName(this FileSettings setting)
        {
            return $"{setting}.json";
        }

        public static string GetFolderName(this FileSettings setting)
        {
            return "./" + setting.ToString();
        }

        public static string GetFullPath(this FileSettings setting)
        {
            string relativePath = "./" + setting.GetFileName();
            return IOManager.ProcessLocalPath(relativePath);
        }
    }
}
