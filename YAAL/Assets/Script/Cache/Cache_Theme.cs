using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;
using Avalonia.Media;

namespace YAAL
{
    public class Cache_Theme
    {
        public Dictionary<ThemeSettings, Cache_Brush> categories = new Dictionary<ThemeSettings, Cache_Brush>();
    }
}
