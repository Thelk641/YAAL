using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;
using Avalonia.Media;
using System.ComponentModel;
using Newtonsoft.Json;

namespace YAAL
{
    public class Cache_CustomTheme
    {
        public string name;
        public int topOffset { get; set; } = 0;
        public int bottomOffset { get; set; } = 0;
        public Cache_CustomBrush background { get; set; }
        public Cache_CustomBrush foreground { get; set; }

        public bool transparentButton { get; set; }

        [JsonIgnore]
        public SolidColorBrush buttonBackground
        {
            get
            {
                if (transparentButton)
                {
                    return new SolidColorBrush(Colors.Transparent);
                }
                return new SolidColorBrush(color, opacity);
            }
        }

        public Color color;
        public double opacity;
    }
}
