using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;

namespace YAAL
{
    public class Cache_DisplaySlot
    {
        public string slotName { get; set; }
        public Cache_RoomSlot cache { get; set; }

        public bool isHeader { get; set; } = true;

        public void SetSlot(Cache_RoomSlot newCache)
        {
            cache = newCache;
            slotName = cache.slotName;
            isHeader = false;
        }
    }
}
