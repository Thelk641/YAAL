using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YAAL
{
    public static class ThemeManager 
    {
        public static Dictionary<string, Cache_CustomTheme> themes = new Dictionary<string, Cache_CustomTheme>();
        public static Dictionary<string, WeakReference<Bitmap>> images = new Dictionary<string, WeakReference<Bitmap>>();
        public static Dictionary<string, List<Control>> themedControl = new Dictionary<string, List<Control>>();
        public static Dictionary<Control, Border> themeContainers = new Dictionary<Control, Border>();
        public static string defaultTheme = "General Theme";

        static ThemeManager()
        {
            // TODO : this shouldn't be hardcoded
            themes["General Theme"] = DefaultManager.theme;
        }

        public static void ApplyTheme(Control container, string theme = "")
        {
            if (theme == "")
            {
                theme = defaultTheme;
            }
            ThemeSettings category = ThemeCategory.GetThemeCategory(container);
            Cache_CustomTheme cache = GetTheme(theme);

            if(container is Button button)
            {
                button.Background = cache.buttonBackground;
                return;
            }

            if(container is ComboBox comboBox)
            {
                SolidColorBrush brush = cache.buttonBackground;
                comboBox.Background = cache.buttonBackground;
                if (comboBox.GetVisualDescendants().OfType<ContentPresenter>().FirstOrDefault() is ContentPresenter presenter)
                {
                    presenter.Background = new SolidColorBrush(AutoColor.Darken(brush.Color));
                }
                return;
            }


            if(container is Panel panel)
            {
                if (themeContainers.ContainsKey(container))
                {
                    panel.Children.Remove(themeContainers[container]);
                    foreach (var item in themedControl)
                    {
                        if (item.Value.Contains(container))
                        {
                            item.Value.Remove(container);
                        }
                    }
                }

                Border layer0 = null;

                if(category == ThemeSettings.backgroundColor)
                {
                    layer0 = cache.background.BackgroundHolder;
                } else
                {
                    layer0 = cache.foreground.BackgroundHolder;
                }

                panel.Children.Add(layer0);
                themeContainers[container] = layer0;
                if (!themedControl.ContainsKey(theme))
                {
                    themedControl[theme] = new List<Control>();
                }
                themedControl[theme].Add(container);
                return;
            }

            ErrorManager.ThrowError(
                "ThemeManager - Tried to apply theme to invalid object type",
                "ApplyTheme was called on object " + container + "which isn't Button, ComboBox or Panel. Please report this."
                );
        }

        public static Cache_CustomTheme GetTheme(string name)
        {
            if(themes.TryGetValue(name, out var result))
            {
                return result;
            }

            Cache_CustomTheme? output = IOManager.GetCustomTheme(name);
            if(output != null)
            {
                themes[name] = output;
                return output;
            }

            if(themes.TryGetValue(defaultTheme, out var defaultResult))
            {
                return defaultResult;
            }

            output = IOManager.GetCustomTheme(defaultTheme);
            if (output != null)
            {
                themes[defaultTheme] = output;
                return output;
            }


            ErrorManager.ThrowError(
                "ThemeManager - Couldn't find themes",
                "Couldn't find theme " + name + " nor default theme named " + defaultTheme + ". Did you manually delete or rename them maybe ?"
                );

            return DefaultManager.theme;
        } 

        public static Cache_CustomTheme GetDefaultTheme()
        {
            if (themes.TryGetValue(defaultTheme, out var defaultResult))
            {
                return defaultResult;
            }

            Cache_CustomTheme? output = IOManager.GetCustomTheme(defaultTheme);
            if (output != null)
            {
                themes[defaultTheme] = output;
                return output;
            }

            ErrorManager.ThrowError(
                "ThemeManager - Couldn't find default theme",
                "Couldn't find default theme named" + defaultTheme + ". Did you manually delete or rename it maybe ?"
                );

            return new Cache_CustomTheme();
        }

        public static Bitmap? GetImage(string name)
        {
            if(images.TryGetValue(name, out var weakRef))
            {
                if(weakRef.TryGetTarget(out var image))
                {
                    return image;
                }

                images.Remove(name);
            }

            Bitmap? readBitmap = IOManager.ReadImage(name);
            if (readBitmap != null)
            {
                images[name] = new WeakReference<Bitmap>(readBitmap);
            }
            return null;
        }

        public static void UpdateTheme(string name, Cache_CustomTheme newTheme)
        {
            if (themes.ContainsKey(name))
            {
                themes.Remove(name);
            }
            themes[name] = newTheme;

            if (themedControl.ContainsKey(name))
            {
                foreach (var item in themedControl[name])
                {
                    ApplyTheme(item, name);
                }
            }
        }
    }
}
