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
        cache_download,
        userSettings,
        backupList,
        tools,
        previous_async,
        launcher,
        multiworld,
        launcherList,

        // upper case, directories
        ManagedApworlds,
        Async,
        Trash,
        Logs,
        MinimumWorlds
    };

    public static class FileSettingExtensions
    {
        public static string GetFileName(this FileSettings setting)
        {
            return $"{setting}.json";
        }

        public static string GetFolderName(this FileSettings setting)
        {
            return setting.ToString();
        }
    }
}
