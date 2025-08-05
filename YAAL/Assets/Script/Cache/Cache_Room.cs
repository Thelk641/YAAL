using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;

namespace YAAL
{
    public class Cache_Room
    {
        public string URL;
        public string IP;
        public string port;
        public string cheeseTrackerURL;
        public string trackerPageURL;
        public Dictionary<string, Cache_RoomSlot> slots = new Dictionary<string, Cache_RoomSlot>();

        public void UpdatePort()
        {

        }

        public void SetURLs(string newURL)
        {
            URL = newURL;
        }
    }
}
