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
    public class Theme
    {
        public SolidColorBrush color;
        public ImageBrush image;
        public Dictionary<string, bool> darkmode = new Dictionary<string, bool>();
    }
}
