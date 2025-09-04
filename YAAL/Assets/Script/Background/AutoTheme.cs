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
        public static ConditionalWeakTable<Control, EventHandler<string>> alreadyThemed = new ConditionalWeakTable<Control, EventHandler<string>>();

        public static readonly AttachedProperty<Cache_ThemeInfo> AutoThemeProperty =
        AvaloniaProperty.RegisterAttached<Control, Cache_ThemeInfo>("AutoTheme", typeof(AutoTheme), inherits: false);

        public static void SetAutoTheme(AvaloniaObject element, Cache_ThemeInfo value) =>
            element.SetValue(AutoThemeProperty, value);

        public static Cache_ThemeInfo GetAutoTheme(AvaloniaObject element) =>
            element.GetValue(AutoThemeProperty);

        static AutoTheme()
        {
            AutoThemeProperty.Changed.AddClassHandler<Control>(
                (ctrl, e) =>
                {
                    EnableAutoTheme(ctrl, e.NewValue as Cache_ThemeInfo);
                });
        }

        private static void EnableAutoTheme(Control ctrl, Cache_ThemeInfo themeInfo)
        {
            if (themeInfo == null || !themeInfo.isThemed || ctrl.TemplatedParent != null)
            {
                return;
            }

            EventHandler<string> handler = (_, updatedTheme) =>
            {
                if (updatedTheme == themeInfo.theme)
                {
                    ApplyTheme(ctrl, App.Settings.GetTheme(themeInfo.theme), themeInfo);
                }
                else if (updatedTheme == "General Theme" && App.Settings.GetTheme(themeInfo.theme) == null)
                {
                    ApplyTheme(ctrl, App.Settings.GetTheme("General Theme"), themeInfo);
                }
            };

            if (alreadyThemed.TryGetValue(ctrl, out var oldhandler))
            {
                alreadyThemed.Remove(ctrl);
                App.Settings.ThemeChanged -= oldhandler;
            }

            Cache_Theme theme = App.Settings.GetTheme(themeInfo.theme);
            if (theme == null)
            {
                theme = App.Settings.GetTheme("General Theme");
            }

            App.Settings.ThemeChanged += handler;
            if (themeInfo.theme != "General Theme")
            {
                alreadyThemed.Add(ctrl, handler);
            }
            ApplyTheme(ctrl, theme, themeInfo);
        }

        private static void ApplyTheme(Control ctrl, Cache_Theme theme, Cache_ThemeInfo themeInfo)
        {
            if (themeInfo.category == ThemeSettings.transparent)
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
                switch (ctrl)
                {
                    case Window window:
                        window.Background = theme.categories[themeInfo.category ?? ThemeSettings.backgroundColor].GetBrush();
                        break;
                    case Button button:
                        button.Background = theme.categories[themeInfo.category ?? ThemeSettings.buttonColor].GetBrush();
                        break;
                    case ComboBox comboBox:
                        comboBox.Background = theme.categories[themeInfo.category ?? ThemeSettings.dropdownColor].GetBrush();
                        break;
                    case Border border:
                        border.Background = theme.categories[themeInfo.category ?? ThemeSettings.foregroundColor].GetBrush();
                        break;
                    case TemplatedControl templated:
                        templated.Background = theme.categories[themeInfo.category ?? ThemeSettings.foregroundColor].GetBrush();
                        break;
                }
            }
        }

        public static void SetTheme(Control ctr, ThemeSettings group, string theme = "General Theme")
        {
            Cache_ThemeInfo themeInfo = new Cache_ThemeInfo();
            themeInfo.category = group;
            themeInfo.theme = theme;
            SetAutoTheme(ctr, themeInfo);
        }

        public static void SetTheme(Control ctrl, string theme)
        {
            ThemeSettings group;

            switch (ctrl)
            {
                case Window window:
                    group = ThemeSettings.backgroundColor;
                    break;
                case Button button:
                    group = ThemeSettings.buttonColor;
                    break;
                case ComboBox comboBox:
                    group = ThemeSettings.dropdownColor;
                    break;
                default:
                    group = ThemeSettings.foregroundColor;
                    break;
            }

            SetTheme(ctrl, group, theme);
        }

        public static void SetScrollbarTheme(ScrollViewer scrollViewer)
        {
            ApplyScrollbarTheme(scrollViewer);
            App.Settings.ThemeChanged += (_, updatedTheme) =>
            {
                if(updatedTheme == "General Theme")
                {
                    ApplyScrollbarTheme(scrollViewer);
                }
            };
        }

        private static void ApplyScrollbarTheme(ScrollViewer scrollViewer)
        {
            IBrush brush = App.Settings.GetTheme("General Theme").categories[ThemeSettings.foregroundColor].GetBrush();
            var rectangle = scrollViewer.GetVisualDescendants().OfType<Rectangle>().FirstOrDefault();
            if(rectangle != null)
            {
                rectangle.Fill = brush;
            }
        }
    }
}
