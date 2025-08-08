using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;

namespace YAAL
{
    public class Cache_Async : ICloneable
    {
        public Dictionary<AsyncSettings, string> settings = new Dictionary<AsyncSettings, string>()
        {
            {asyncName, "" },
            {roomURL, ""},
            {password, "None" },
            {roomIP, "" },
            {roomPort, "" }
        };

        public Dictionary<string, string> toolVersions = new Dictionary<string, string>();

        public List<Cache_Slot> slots = new List<Cache_Slot>();

        public Cache_Room room = new Cache_Room();

        public Object Clone()
        {
            return this.MemberwiseClone();
        }

        public void ParseRoomInfo()
        {
            string room = settings[AsyncSettings.roomURL].Trim();

            var match = Regex.Match(room, @"^(?<host>.+):(?<port>\d{5})\s*$");

            if (match.Success)
            {
                settings[roomIP] = match.Groups["host"].Value;
                settings[roomPort] = match.Groups["port"].Value;
            }
            else
            {
                settings[roomIP] = room;
                settings[roomPort] = "";
            }

            if (settings[roomIP] == "localhost")
            {
                // this is the default port
                settings[roomPort] = "38281";
            }
        }
    }
}
