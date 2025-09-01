using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;
using Avalonia.Media;
using System.ComponentModel;

namespace YAAL
{
    public class Cache_Theme : INotifyPropertyChanged
    {
        public string name;
        public Dictionary<ThemeSettings, Cache_Brush> categories = new Dictionary<ThemeSettings, Cache_Brush>()
        {
            {ThemeSettings.backgroundColor, Cache_Brush.DefaultBackground() },
            {ThemeSettings.foregroundColor, Cache_Brush.DefaultForeground() },
            {ThemeSettings.dropdownColor, Cache_Brush.DefaultComboBox() },
            {ThemeSettings.buttonColor, Cache_Brush.DefaultButton() },
        };

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
