using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAAL
{
    [DefaultValue(null)]
    public enum ThemeSettings
    {
        backgroundColor,
        foregroundColor,
        dropdownColor,
        buttonColor,
        transparent,
        rendered,
        off
    };
}
