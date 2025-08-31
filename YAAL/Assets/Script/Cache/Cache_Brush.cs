using Avalonia.Media;
using Avalonia.Media.Imaging;
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
        public SolidColorBrush? colorBrush;
        public string? imageSource;
        public Stretch stretch;
        public TileMode tilemode;

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
    }
}
