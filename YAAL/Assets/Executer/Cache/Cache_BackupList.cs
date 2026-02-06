using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAAL
{
    public class Cache_BackupList
    {
        public List<Cache_Backup> backupList = new List<Cache_Backup>();
        public Dictionary<string, List<string>> apworldList = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> launcherToApworldList = new Dictionary<string, List<string>>();
    }
}
