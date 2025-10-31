using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAAL
{
    public class Cache_CustomLauncher
    {
        public Dictionary<string, Dictionary<string, string>> instructions = new();
        public Dictionary<LauncherSettings, string> settings = new();
        public Dictionary<string, string> customSettings = new();
        public bool isGame = true;
        public bool requiresPatch = false;
        public bool requiresVersion = false;
    }
}
