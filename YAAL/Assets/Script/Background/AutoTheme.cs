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

            if (themeInfo == null || themeInfo == ThemeSettings.off || ctrl.TemplatedParent != null)
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
            if (themeInfo == ThemeSettings.transparent)
            {
                IBrush brush = new SolidColorBrush(Avalonia.Media.Colors.Transparent);

                switch (ctrl)
                {
                    case Window window:
                        window.Background = brush;
                        break;
                    case Button button:
                        button.Background = brush;
                        break;
                    case ComboBox comboBox:
                        comboBox.Background = brush;
                        if (comboBox.GetVisualDescendants().OfType<ContentPresenter>().FirstOrDefault() is ContentPresenter presenter)
                        {
                            if (brush is SolidColorBrush solid)
                            {
                                presenter.Background = new SolidColorBrush(AutoColor.Darken(solid.Color));
                            } else
                            {
                                presenter.Background = new SolidColorBrush(Colors.Transparent);
                            }
                        }
                        break;
                    case Border border:
                        border.Background = brush;
                        break;
                    case TemplatedControl templated:
                        templated.Background = brush;
                        break;
                }
            }
            else
            {
                if(ctrl is TemplatedControl templated)
                {
                    Color color = ThemeManager.GetGeneralTheme(themeInfo);
                    templated.Background = new SolidColorBrush(color);
                    if(templated is ComboBox combo)
                    {
                        if (combo.GetVisualDescendants().OfType<ContentPresenter>().FirstOrDefault() is ContentPresenter presenter)
                        {
                            presenter.Background = new SolidColorBrush(AutoColor.Darken(color));
                        }
                    }
                } else
                {
                    Debug.WriteLine("ctrl is not templated !? " + ctrl.Name);
                }
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
