using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;

namespace YAAL
{
    public class Cache_Background
    {
        public GeneralSettings group;
        public Dictionary<Avalonia.Svg.Skia.Svg, Icons> icons = new Dictionary<Avalonia.Svg.Skia.Svg, Icons>();
    }
}
