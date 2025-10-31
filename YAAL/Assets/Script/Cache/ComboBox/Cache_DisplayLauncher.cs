using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;

namespace YAAL
{
    public class Cache_DisplayLauncher : ICloneable
    {
        public string name { get; set; }
        public Cache_CustomLauncher cache { get; set; }

        public bool isHeader { get; set; } = false;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
