using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace YAAL.Assets.Scripts
{
    public class UISettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? ThemeUpdated;


        // Zoom setting
        private double _zoom = 1.0;
        public double Zoom
        {
            get => _zoom;
            set
            {
                if (_zoom != value)
                {
                    _zoom = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Zoom)));
                }
            }
        }

        // Better Theme
        private Dictionary<string, Cache_Theme> themes = new Dictionary<string, Cache_Theme>();

        public event EventHandler<string>? ThemeChanged;

        public void LoadThemes()
        {
            themes["General Theme"] = IOManager.GetGeneralTheme();
            foreach (var item in IOManager.GetLauncherList())
            {
                Cache_CustomLauncher cache = IOManager.LoadCacheLauncher(item);
                if(cache.customTheme != null)
                {
                    themes[item] = cache.customTheme;
                }
            }
        }

        public void SetTheme(string key, Cache_Theme theme)
        {
            themes[key] = theme;
            ThemeChanged?.Invoke(this, key);
        }

        public Cache_Theme? GetTheme(string key)
        {
            if (themes.ContainsKey(key))
            {
                return themes[key];
            }
            else
            {
                return null;
            }
        }
    }
}
