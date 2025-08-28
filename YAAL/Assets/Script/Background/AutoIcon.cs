using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Svg.Skia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static YAAL.AutoColor;

namespace YAAL
{
    public static class AutoIcon
    {
        static Dictionary<int, Cache_Background> backgrounds = new Dictionary<int, Cache_Background>();
        static List<int> subscribed = new List<int>();

        public static readonly AttachedProperty<Icons> AutoIconProperty =
        AvaloniaProperty.RegisterAttached<Button, Icons>("Icon", typeof(AutoIcon), Icons.None);

        public static void SetIcon(AvaloniaObject element, Icons value) =>
            element.SetValue(AutoIconProperty, value);

        public static Icons GetIcon(AvaloniaObject element) =>
            element.GetValue(AutoIconProperty);

        static AutoIcon()
        {
            AutoIconProperty.Changed.AddClassHandler<Button>(
                (button, e) =>
                {
                    if(button.Background == null)
                    {
                        if (!subscribed.Contains(button.GetHashCode()))
                        {
                            button.GetObservable(Button.BackgroundProperty).Subscribe(_ => EvaluateBackground(button));
                            subscribed.Add(button.GetHashCode());
                            UpdateDictionary(button, (Icons)e.NewValue!);
                        }
                        else
                        {
                            return;
                        }
                    }
                });
        }

        private static void EvaluateBackground(Button button)
        {
            if(button.Background == null)
            {
                return;
            }

            int hash = button.GetHashCode();

            if (!backgrounds.ContainsKey(hash)){
                return;
            }

            
            IBrush brush = GetBrush(button);
            Icons icon = backgrounds[hash].icon;

            EvaluateBackground(button, brush, icon);
        }

        private static void EvaluateBackground(Button button, IBrush backgroundColor, Icons icon)
        {
            int hash = button.GetHashCode();

            if (backgrounds.ContainsKey(hash) && backgrounds[hash].previousColor == (backgroundColor as ISolidColorBrush).Color)
            {
                return;
            }


            bool darkMode = true;

            if (backgroundColor is ISolidColorBrush solid)
            {
                darkMode = !NeedsWhite(solid.Color);
            }

            var holder = button.FindDescendantOfType<Avalonia.Svg.Skia.Svg>();

            if (holder != null)
            {
                SetIcon(button, darkMode, icon);
            } else
            {
                button.AttachedToVisualTree += async (_, _) =>
                {
                    await Dispatcher.UIThread.InvokeAsync(() => 
                    {
                        SetIcon(button, darkMode, icon);
                    });
                };
            }
        }

        private static void SetIcon(Button button, bool darkmode, Icons icon)
        {
            int hash = button.GetHashCode();

            var holder = button.FindDescendantOfType<Avalonia.Svg.Skia.Svg>();

            if(holder == null)
            {
                return;
            }

            if (darkmode)
            {
                holder.Path = icon.Dark();
                //holder.Path = "avares://YAAL/Assets/Icons/File_dark.svg";
            }
            else
            {
                holder.Path = icon.White();
                //holder.Path = "avares://YAAL/Assets/Icons/File_white.svg";
            }

            if (backgrounds.ContainsKey(hash))
            {
                backgrounds[hash].previousColor = (button.Background as ISolidColorBrush).Color;
            }
        }

        private static void UpdateDictionary(Button button, Icons icon)
        {
            int hash = button.GetHashCode();

            Cache_Background cache = new Cache_Background();
            cache.button = button;
            cache.icon = icon;
            if(button.Background != null)
            {
                cache.previousColor = (GetBrush(button) as ISolidColorBrush).Color;
            }
            backgrounds[hash] = cache;
        }
    }
}
