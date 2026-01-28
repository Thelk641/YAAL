using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;

namespace YAAL
{
    public class Cache_Game: ICloneable
    {
        public Object Clone()
        {
            return this.MemberwiseClone();
        }

        public string GetSetting(Enum key)
        {
            return "";
        }
    }
}
