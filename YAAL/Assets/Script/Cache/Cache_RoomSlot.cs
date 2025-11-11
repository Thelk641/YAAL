using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;

namespace YAAL
{
    public class Cache_RoomSlot
    {
        public string slotName;
        public string gameName;
        public string trackerURL;
        public string patchURL;

        public override bool Equals(object? obj)
        {
            bool output = false;
            if(obj is Cache_RoomSlot toCompare)
            {
                if(this.slotName == toCompare.slotName
                    && this.gameName == toCompare.gameName
                    && this.trackerURL == toCompare.trackerURL
                    && this.patchURL == toCompare.patchURL)
                {
                    output = true;
                }
            }
            return output;
        }
    }
}
