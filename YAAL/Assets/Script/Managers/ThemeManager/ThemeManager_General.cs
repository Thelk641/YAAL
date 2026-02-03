using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Metsys.Bson;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YAAL.Assets.Scripts;
using static System.Net.Mime.MediaTypeNames;

namespace YAAL
{
    public static partial class ThemeManager 
    {
        static Cache_GeneralTheme generalTheme;
        public static Action? GeneralThemeUpdated;
        static ThemeManager()
        {
            Dispatcher.UIThread.Post(
                () => {
                    if(WindowManager.mainWindow != null)
                    {
                        UpdateCenters();
                    }
            });
        }

        public static void UpdateGeneralTheme()
        {
            generalTheme = new Cache_GeneralTheme();
            string background = SettingsManager.GetSetting(GeneralSettings.backgroundColor);
            string foreground = SettingsManager.GetSetting(GeneralSettings.foregroundColor);
            string button = SettingsManager.GetSetting(GeneralSettings.buttonColor);

            if(background == "" || foreground == "" || button == "")
            {
                generalTheme = DefaultManager.generalTheme;
                return;
            }
            generalTheme.background = AutoColor.HexToColor(background);
            generalTheme.foreground = AutoColor.HexToColor(foreground);
            generalTheme.button = AutoColor.HexToColor(button);

            GeneralThemeUpdated?.Invoke();
        }

        public static Color GetGeneralTheme(ThemeSettings setting)
        {
            Color output = new Color();
            switch (setting)
            {
                case ThemeSettings.backgroundColor:
                    output = generalTheme.background;
                    break;
                case ThemeSettings.foregroundColor:
                    output = generalTheme.foreground;
                    break;
                case ThemeSettings.buttonColor:
                    output = generalTheme.button;
                    break;
            }

            return output;
        }
    }
}
