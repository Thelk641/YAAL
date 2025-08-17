using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace YAAL
{
    public static class BackgroundSetter
    {
        static Dictionary<Border, Cache_Background> backgrounds = new Dictionary<Border, Cache_Background>();

        public static void UpdateBackground(GeneralSettings group, string newHex)
        {
            Color color = ColorSelector.HexToColor(newHex);

            foreach (var item in backgrounds)
            {
                if(item.Value.group == group)
                {
                    SetBackground(item.Key, color, item.Value.icons);
                }
            }
        }

        public static void SetBackground(Border toSet, Color color, Dictionary<Avalonia.Svg.Skia.Svg, Icons> svgs)
        {
            if(toSet != null)
            {
                if (!backgrounds.ContainsKey(toSet))
                {
                    backgrounds[toSet] = new Cache_Background();
                }

                toSet.Background = new SolidColorBrush(color);
            }
            

            foreach (var item in svgs)
            {
                if (!backgrounds[toSet].icons.ContainsKey(item.Key))
                {
                    backgrounds[toSet].icons[item.Key] = item.Value;
                }
                SetIcon(color, item.Key, item.Value);
            }
        }

        public static void SetBackground(Border toSet, GeneralSettings group, Dictionary<Avalonia.Svg.Skia.Svg, Icons> svgs)
        {
            if (toSet!= null && !backgrounds.ContainsKey(toSet))
            {
                backgrounds[toSet] = new Cache_Background();
                backgrounds[toSet].group = group;
            }

            string setting = IOManager.GetSetting(group);
            Color backgroundColor = ColorSelector.HexToColor(setting);

            SetBackground(toSet, backgroundColor, svgs);
        }

        public static void SetBackground(Border toSet, GeneralSettings group, Avalonia.Svg.Skia.Svg svg, Icons icon)
        {
            Dictionary<Avalonia.Svg.Skia.Svg, Icons> dic = new Dictionary<Avalonia.Svg.Skia.Svg, Icons> { { svg, icon } };
            SetBackground(toSet, group, dic);
        }

        public static void SetBackground(Border toSet, Color color, Avalonia.Svg.Skia.Svg svg, Icons icon)
        {
            Dictionary<Avalonia.Svg.Skia.Svg, Icons> dic = new Dictionary<Avalonia.Svg.Skia.Svg, Icons> { { svg, icon } };
            SetBackground(toSet, color, dic);
        }

        public static void SetBackground(Border toSet, GeneralSettings group)
        {
            SetBackground(toSet, group, new Dictionary<Avalonia.Svg.Skia.Svg, Icons>());
        }

        public static void SetBackground(Border toSet, string setting)
        {
            if(Color.TryParse(setting, out Color color))
            {
                SetBackground(toSet, color, new Dictionary<Avalonia.Svg.Skia.Svg, Icons>());
            } else
            {
                SetBackground(toSet, GeneralSettings.foregroundColor);
            }
        }

        public static void SetIcon(GeneralSettings group, Dictionary<Avalonia.Svg.Skia.Svg, Icons> svgs)
        {
            string setting = IOManager.GetSetting(group);
            Color backgroundColor = ColorSelector.HexToColor(setting);
            SetIcon(backgroundColor, svgs);
        }

        public static void SetIcon(GeneralSettings group, Avalonia.Svg.Skia.Svg icon, Icons toUse)
        {
            string setting = IOManager.GetSetting(group);
            Color backgroundColor = ColorSelector.HexToColor(setting);
            SetIcon(backgroundColor, icon, toUse);
        }

        public static void SetIcon(Color color, Dictionary<Avalonia.Svg.Skia.Svg, Icons> svgs)
        {
            foreach (var item in svgs)
            {
                SetIcon(color, item.Key, item.Value);
            }
        }

        public static void SetIcon(Color color, Avalonia.Svg.Skia.Svg icon, Icons toUse)
        {
            if (NeedsWhite(color))
            {
                icon.Path = toUse.White();
            } else
            {
                icon.Path = toUse.Dark();
            }
        }

        private static bool NeedsWhite(Color color)
        {
            double R = color.R / 255;
            double G = color.G / 255;
            double B = color.B / 255;

            double luminance = 0.299 * R + 0.587 * G + 0.114 * B;

            return luminance < 0.5;
        }
    }
}
