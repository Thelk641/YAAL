using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using static YAAL.AutoColor;

namespace YAAL
{
    public static class AutoTheme
    {
        public static ConditionalWeakTable<Control, Action> alreadyThemed = new ConditionalWeakTable<Control, Action>();

        public static readonly AttachedProperty<ThemeSettings> AutoThemeProperty =
        AvaloniaProperty.RegisterAttached<Control, ThemeSettings>("AutoTheme", typeof(AutoTheme), inherits: false);

        public static void SetAutoTheme(AvaloniaObject element, ThemeSettings value) =>
            element.SetValue(AutoThemeProperty, value);

        public static ThemeSettings GetAutoTheme(AvaloniaObject element) =>
            element.GetValue(AutoThemeProperty);

        static AutoTheme()
        {
            AutoThemeProperty.Changed.AddClassHandler<Control>(
                (ctrl, e) =>
                {
                    if(e != null && e.NewValue is ThemeSettings cache)
                    {
                        EnableAutoTheme(ctrl, cache);
                    }
                });
        }

        private static void EnableAutoTheme(Control ctrl, ThemeSettings themeInfo)
        {
            if (alreadyThemed.TryGetValue(ctrl, out var oldhandler))
            {
                alreadyThemed.Remove(ctrl);
                ThemeManager.GeneralThemeUpdated -= oldhandler;
            }

            if (themeInfo == null || themeInfo == ThemeSettings.off)
            {
                return;
            }

            Action handler = () =>
            {
                ApplyTheme(ctrl, themeInfo);
            };

            ThemeManager.GeneralThemeUpdated += handler;
            alreadyThemed.Add(ctrl, handler);

            ApplyTheme(ctrl, themeInfo);
        }

        private static void ApplyTheme(Control ctrl, ThemeSettings themeInfo)
        {
            Color color;

            if(themeInfo == ThemeSettings.transparent)
            {
                color = Colors.Transparent;
            } else
            {
                color = ThemeManager.GetGeneralTheme(themeInfo);
            }

            IBrush brush = new SolidColorBrush(color);

            var background = ctrl.GetType().GetProperty("Background", typeof(IBrush));

            if (background != null && background.CanWrite)
            {
                background.SetValue(ctrl, brush);
            } else
            {
                Trace.WriteLine("No background property for : " + ctrl.Name + " of type " + ctrl.GetType());
            }
        }

        public static void SetTheme(Control ctrl, ThemeSettings group)
        {
            SetAutoTheme(ctrl, group);
        }

        public static void SetScrollbarTheme(ScrollViewer scrollViewer)
        {
            ApplyScrollbarTheme(scrollViewer);
            ThemeManager.GeneralThemeUpdated += () =>
            {
                ApplyScrollbarTheme(scrollViewer);
            };
        }

        private static void ApplyScrollbarTheme(ScrollViewer scrollViewer)
        {
            IBrush brush = new SolidColorBrush(ThemeManager.GetGeneralTheme(ThemeSettings.foregroundColor));
            var rectangle = scrollViewer.GetVisualDescendants().OfType<Rectangle>().FirstOrDefault();
            if(rectangle != null)
            {
                rectangle.Fill = brush;
            }
        }
    }
}
