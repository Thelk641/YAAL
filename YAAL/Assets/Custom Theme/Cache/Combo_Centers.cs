using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;

namespace YAAL
{
    public class Combo_Centers
    {
        public string centerName { get; set; }

        public bool isHeader { get; set; } = true;

        public void SetName(string name)
        {
            centerName = name;
            isHeader = false;
        }
    }
}
