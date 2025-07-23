using Avalonia.Animation;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace YAAL.Assets.Scripts
{
    public static class Cleaner
    {
        public static string Clean(string rawInput)
        {
            if(rawInput == "\"")
            {
                return "";
            }

            return rawInput.Replace("\"", "").Replace("\"", "\""); ;
        }
    }
}
