using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Numerics;

namespace YAAL
{
    public class Cache_RenderedTheme
    {
        public string themeName = "";
        public Vector2 windowSize;
        public WeakReference<Bitmap> background;
        public WeakReference<Bitmap> foreground;
        public SolidColorBrush button;
        public int topOffset;
        public int bottomOffset;
    }
}
