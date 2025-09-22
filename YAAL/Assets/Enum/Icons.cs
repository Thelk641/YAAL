using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAAL
{
    public enum Icons
    {
        File,
        Folder,
        Play,
        ToolPlay,
        Settings,
        Save,
        Trash,
        Up,
        Down,
        Plus,
        Minus,
        Percent,
        Pin,
        None,
    };

    public static class IconsExtension
    {
        public static string Dark(this Icons icon)
        {
            return "avares://YAAL/Assets/Icons/" + icon.ToString() + "_dark.svg";
        }

        public static string White(this Icons icon)
        {
            return "avares://YAAL/Assets/Icons/" + icon.ToString() + "_white.svg";
        }
    }
}
