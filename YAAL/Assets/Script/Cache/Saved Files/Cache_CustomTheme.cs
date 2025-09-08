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
        public Cache_CustomBrush background { get; set; } = new Cache_CustomBrush();
        public Cache_CustomBrush foreground { get; set; } = new Cache_CustomBrush();

        public bool transparentButton { get; set; } = false;

        [JsonIgnore]
        public SolidColorBrush buttonBackground
        {
            get
            {
                if (transparentButton)
                {
                    return new SolidColorBrush(Colors.Transparent);
                }
                return new SolidColorBrush(buttonColor, buttonOpacity);
            }
            set
            {
                buttonColor = value.Color;
                buttonOpacity = value.Opacity;
            }
        }

        public Color buttonColor;
        public double buttonOpacity;
    }
}
