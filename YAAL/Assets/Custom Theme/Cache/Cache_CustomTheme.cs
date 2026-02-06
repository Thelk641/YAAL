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
    public class Cache_CustomTheme : ICloneable
    {
        public string name { get; set; }
        public int topOffset { get; set; } = 0;
        public int bottomOffset { get; set; } = 0;
        public Cache_LayeredBrush background { get; set; } = new Cache_LayeredBrush();
        public Cache_LayeredBrush foreground { get; set; } = new Cache_LayeredBrush();

        [JsonIgnore]
        public SolidColorBrush buttonBackground
        {
            get
            {
                return new SolidColorBrush(buttonColor, buttonOpacity);
            }
            set
            {
                buttonColor = value.Color;
                buttonOpacity = value.Opacity;
            }
        }

        public Color buttonColor = Colors.Transparent;
        public double buttonOpacity = 1;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
