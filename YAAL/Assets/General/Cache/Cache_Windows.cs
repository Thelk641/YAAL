using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace YAAL
{
    public class Cache_Windows
    {
        public Dictionary<WindowType, PixelPoint> positions = new Dictionary<WindowType, PixelPoint>();
        public Dictionary<WindowType, Vector2> size = new Dictionary<WindowType, Vector2>();
        public Dictionary<WindowType, bool> maximized = new Dictionary<WindowType, bool>();
    }
}
