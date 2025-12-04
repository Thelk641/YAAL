using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.SlotSettings;

namespace YAAL
{
    public class Cache_Slot : ICloneable
    {
        public Dictionary<SlotSettings, string> settings = new Dictionary<SlotSettings, string>()
        {
            {patch, ""},
            {slotName, ""},
            {slotInfo, "${slotName}:${password}@${roomIP}:${roomPort}"},
            {rom, "" },
            {version, "" },
            {baseLauncher, "" },
            {slotTrackerURL, "" }
        };

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
