using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAAL
{
    public static class ThemeManager 
    {
        public static Dictionary<string, Cache_CustomTheme> themes = new Dictionary<string, Cache_CustomTheme>();
        public static Dictionary<string, WeakReference<Bitmap>> images = new Dictionary<string, WeakReference<Bitmap>>();
        public static Dictionary<Control, Border> themedControl = new Dictionary<Control, Border>();
        public static string defaultTheme = "General Theme";

        public static void ApplyTheme(Control container, string theme = "")
        {
            if(theme == "")
            {
                theme = defaultTheme;
            }

            ThemeSettings category = ThemeSettings.foregroundColor;

            ApplyTheme(container, category, theme);
        }

        public static void ApplyTheme(Control container, ThemeSettings category, string theme = "")
        {
            if (theme == "")
            {
                theme = defaultTheme;
            }
            Cache_CustomTheme cache = GetTheme(theme);

            if(container is Button button)
            {
                button.Background = cache.buttonBackground;
                return;
            }

            if(container is ComboBox comboBox)
            {
                // TODO : there is more to comboBox theme then background, see AutoTheme for more stuff to do
                comboBox.Background = cache.buttonBackground;
                return;
            }


            if(container is Panel panel)
            {
                if (themedControl.ContainsKey(container))
                {
                    panel.Children.Remove(themedControl[container]);
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
                themedControl[container] = layer0;
            }
        }

        public static Cache_CustomTheme GetTheme(string name)
        {
            if(themes.TryGetValue(name, out var result))
            {
                return result;
            }

            if(themes.TryGetValue(defaultTheme, out var defaultResult))
            {
                return defaultResult;
            }

            ErrorManager.ThrowError(
                "ThemeManager - Couldn't find themes",
                "Couldn't find theme " + name + " nor default theme named " + defaultTheme + ". Did you manually delete or rename them maybe ?"
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
    }
}
