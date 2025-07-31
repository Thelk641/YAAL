using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
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
    public static class BackgroundSetter
    {
        public static void SetBackground(List<Border> toSet)
        {
            var theme = Application.Current.ActualThemeVariant;
            if (theme == ThemeVariant.Dark)
            {
                foreach (var item in toSet)
                {
                    item.Background = new SolidColorBrush(Color.Parse("#454545"));
                }
            }
            else
            {
                foreach (var item in toSet)
                {
                    item.Background = new SolidColorBrush(Color.Parse("#AAA"));
                }
            }
        }

        public static void SetBackground(Border toSet)
        {
            var theme = Application.Current.ActualThemeVariant;
            if (theme == ThemeVariant.Dark)
            {
                toSet.Background = new SolidColorBrush(Color.Parse("#454545"));

            }
            else
            {
                toSet.Background = new SolidColorBrush(Color.Parse("#AAA"));
            }
        }
    }
}
