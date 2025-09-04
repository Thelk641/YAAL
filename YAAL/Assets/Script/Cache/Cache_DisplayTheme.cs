using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;

namespace YAAL
{
    public class Cache_DisplayTheme
    {
        public bool isHeader { get; set; } = true;
        public Cache_Theme cache_theme;
        public string launcherName { get; set; }

        public void SetTheme(string name, Cache_Theme theme)
        {
            launcherName = name;
            cache_theme = theme;
            isHeader = false;
        }
    }
}
