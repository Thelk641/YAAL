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
        static Dictionary<GeneralSettings, Dictionary<int, Visual>> backgrounds = new Dictionary<GeneralSettings, Dictionary<int, Visual>>
        {
            { GeneralSettings.backgroundColor, new Dictionary<int, Visual>()},
            { GeneralSettings.foregroundColor, new Dictionary<int, Visual>()},
            { GeneralSettings.dropdownColor, new Dictionary<int, Visual>()},
            { GeneralSettings.buttonColor, new Dictionary<int, Visual>()},
        };
        static Dictionary<string, Dictionary<GeneralSettings, Dictionary<int, Visual>>> custom = new Dictionary<string, Dictionary<GeneralSettings, Dictionary<int, Visual>>>();

        public static void UpdateBackground(GeneralSettings group, string newHex)
        {
            Color color = AutoColor.HexToColor(newHex);

            if (!backgrounds.ContainsKey(group))
            {
                return;
            }


            foreach (var item in backgrounds[group])
            {
                Set(item.Value, color);
            }
        }

        public static void UpdateBackground(string launcherName, GeneralSettings group, string newHex)
        {
            Color color = AutoColor.HexToColor(newHex);

            if (!custom.ContainsKey(launcherName) || !custom[launcherName].ContainsKey(group))
            {
                return;
            }

            foreach (var item in custom[launcherName][group])
            {
                Set(item.Value, color);
            }
        }

        public static void Set(Visual visual, GeneralSettings group)
        {
            return;
            int hash = visual.GetHashCode();
            backgrounds[group][hash] = visual;
            Color color = AutoColor.HexToColor(IOManager.GetSetting(group));
            Set(visual, color);
        }

        public static void Set(Visual visual, Color color)
        {
            return;
            switch (visual)
            {
                case Border border:
                    border.Background = new SolidColorBrush(color);
                    break;
                case Window window:
                    window.Background = new SolidColorBrush(color);
                    break;
                case ComboBox box:
                    box.Background = new SolidColorBrush(color);
                    break;
                case Button button:
                    button.Background = new SolidColorBrush(color);
                    break;
            }
        }

        public static void Set(Visual visual, string setting)
        {
            Set(visual, AutoColor.HexToColor(setting));
        }

        public static void Set(Visual visual)
        {
            return;
            switch (visual)
            {
                case Border border:
                    Set(visual, GeneralSettings.foregroundColor);
                    break;
                case Window window:
                    Set(visual, GeneralSettings.backgroundColor);
                    break;
                case ComboBox box:
                    Set(visual, GeneralSettings.dropdownColor);
                    break;
                case Button button:
                    Set(visual, GeneralSettings.buttonColor);
                    break;
            }
        }

        public static void SetCustom(Visual visual, string launcherName)
        {
            return;
            int hash = visual.GetHashCode();
            CustomLauncher cache = IOManager.LoadLauncher(launcherName);
            GeneralSettings group = GeneralSettings.foregroundColor;
            switch (visual)
            {
                case ComboBox box:
                    group = GeneralSettings.dropdownColor;
                    break;
                case Button button:
                    group = GeneralSettings.buttonColor;
                    break;
            }

            Color color;

            if (cache.selfsettings.ContainsKey(LauncherSettings.useCustomColor) && cache.selfsettings[LauncherSettings.useCustomColor] == true.ToString() && cache.customSettings.ContainsKey(group.ToString()))
            {
                color = AutoColor.HexToColor(cache.customSettings[group.ToString()]);
            } else
            {
                color = AutoColor.HexToColor(IOManager.GetSetting(group));
            }

            Set(visual, color);

            foreach (var launcher in custom)
            {
                foreach (var settingGroup in launcher.Value)
                {
                    if (settingGroup.Value.ContainsKey(hash))
                    {
                        settingGroup.Value.Remove(hash);
                    }
                }
            }

            if (!custom.ContainsKey(launcherName))
            {
                custom[launcherName] = new Dictionary<GeneralSettings, Dictionary<int, Visual>>();
                custom[launcherName][group] = new Dictionary<int, Visual>();
                custom[launcherName][group][hash] = visual;
            } else
            {
                if (custom[launcherName].ContainsKey(group))
                {
                    custom[launcherName][group][hash] = visual;
                } else
                {
                    custom[launcherName][group] = new Dictionary<int, Visual>();
                    custom[launcherName][group][hash] = visual;
                }
            }
        }
    }
}
