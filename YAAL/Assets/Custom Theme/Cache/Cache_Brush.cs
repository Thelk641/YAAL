using Avalonia.Media;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;

namespace YAAL
{
    public class Cache_Brush
    {
        public bool isImage = false;
        [JsonIgnore]
        public SolidColorBrush colorBrush
        {
            get 
            {
                if(isTransparent)
                {
                    return new SolidColorBrush(Colors.Transparent);
                }
                SolidColorBrush output = new SolidColorBrush();
                output.Color = color;
                output.Opacity = opacity;
                return output;
            } 
            set
            {
                color = value.Color;
                opacity = value.Opacity;
            }
        }
        public Color color;
        public double opacity;
        public string? imageSource;
        public Stretch stretch;
        public TileMode tilemode;
        public bool isTransparent = false;

        public IBrush GetBrush()
        {
            if (isImage && imageSource != null && File.Exists(imageSource))
            {
                ImageBrush output = new ImageBrush();
                output.Source = new Bitmap(imageSource);
                output.Stretch = stretch;
                output.TileMode = tilemode;

                return output;
            }
            else
            {
                return colorBrush;
            }
        }

        public static Cache_Brush DefaultBackground()
        {
            Cache_Brush output = new Cache_Brush();
            output.colorBrush = new SolidColorBrush(AutoColor.HexToColor("#FF000000"));
            return output;
        }

        public static Cache_Brush DefaultForeground()
        {
            Cache_Brush output = new Cache_Brush();
            output.colorBrush = new SolidColorBrush(AutoColor.HexToColor("#FF313131"));
            return output;
        }

        public static Cache_Brush DefaultComboBox()
        {
            Cache_Brush output = new Cache_Brush();
            output.colorBrush = new SolidColorBrush(AutoColor.HexToColor("#FF1D1D1D"));
            return output;
        }

        public static Cache_Brush DefaultButton()
        {
            Cache_Brush output = new Cache_Brush();
            output.colorBrush = new SolidColorBrush(AutoColor.HexToColor("#FF5A5A5A"));
            return output;
        }

    }
}
