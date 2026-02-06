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
    public class Cache_GeneralTheme
    {
        public Color background;
        public Color foreground;
        public Color button;
        public Cache_CustomTheme theme { 
            get {
                Cache_CustomTheme output = new Cache_CustomTheme();

                Cache_LayeredBrush backgroundLayer = new Cache_LayeredBrush();
                Cache_Brush backgroundBrush = new Cache_Brush();
                backgroundBrush.colorBrush = new SolidColorBrush(background);
                output.background = backgroundLayer;

                Cache_LayeredBrush foregroundLayer = new Cache_LayeredBrush();
                Cache_Brush foregroundBrush = new Cache_Brush();
                foregroundBrush.colorBrush = new SolidColorBrush(foreground);
                output.foreground = foregroundLayer;

                output.topOffset = 0;
                output.bottomOffset = 0;
                output.buttonColor = button;
                output.name = "General Theme";

                return output;
            } 
        
        }
    }
}
