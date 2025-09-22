using Avalonia;
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
using System.Numerics;
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

        public static Dictionary<string, Point> playCenters = new Dictionary<string, Point>();
        public static Dictionary<string, Point> editCenters = new Dictionary<string, Point>();
        public static List<Combo_Centers> centers = new List<Combo_Centers>();
        private static Vector2 slotSize;

        static ThemeManager()
        {
            // TODO : this shouldn't be hardcoded
            themes["General Theme"] = DefaultManager.theme;
            UpdateCenters();
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

        public static Bitmap Render(Control ctrl, string themeName, ThemeSettings category)
        {
            Vector2 dpi = new Vector2(96, 96);
            Size renderSize = ctrl.Bounds.Size;
            if(renderSize.Width <=0 || renderSize.Height <= 0)
            {
                ctrl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                renderSize = ctrl.DesiredSize;
            }

            int pixelWidth = (int)Math.Ceiling(renderSize.Width * dpi.X / 96.0);
            int pixelHeight = (int)Math.Ceiling(renderSize.Height * dpi.Y / 96.0);

            var pixelSize = new PixelSize(pixelWidth, pixelHeight);

            var renderer = new RenderTargetBitmap(pixelSize, dpi);
            var finalRect = new Rect(renderSize);

            ctrl.Measure(renderSize);
            ctrl.Arrange(finalRect);

            renderer.Render(ctrl);

            IOManager.SaveImage(renderer, themeName, category);
            return renderer;
        }

        public static void UpdateCenters()
        {
            playCenters = new Dictionary<string, Point>();
            editCenters = new Dictionary<string, Point>();

            ThemeSlotV2 themeSlot = new ThemeSlotV2();
            themeSlot.Measure(Size.Infinity);
            slotSize = WindowManager.GetSlotSize();
            Rect desiredSize = new Rect(0, 0, slotSize.X, slotSize.Y);
            themeSlot.Arrange(desiredSize);

            playCenters["Play Button"] = themeSlot.FindControl<Button>("RealPlay")!.Bounds.Center;
            playCenters["Slot Name"] = themeSlot.FindControl<TextBlock>("_SlotName")!.Bounds.Center;
            playCenters["Tool Name"] = themeSlot.FindControl<ComboBox>("ToolSelect")!.Bounds.Center;
            playCenters["Start Tool"] = themeSlot.FindControl<Button>("StartTool")!.Bounds.Center;
            playCenters["Settings"] = themeSlot.FindControl<Button>("Edit")!.Bounds.Center;

            themeSlot.SwitchMode();

            editCenters["Play Button"] = themeSlot.FindControl<Button>("FakePlay")!.Bounds.Center;
            editCenters["Slot Selector"] = themeSlot.FindControl<ComboBox>("SlotSelector")!.Bounds.Center;
            editCenters["Download"] = themeSlot.FindControl<Button>("DownloadPatch")!.Bounds.Center;
            editCenters["Save"] = themeSlot.FindControl<Button>("DoneEditing")!.Bounds.Center;
            editCenters["Launcher Selector"] = themeSlot.FindControl<ComboBox>("SelectedLauncher")!.Bounds.Center;
            editCenters["Version Selector"] = themeSlot.FindControl<ComboBox>("SelectedVersion")!.Bounds.Center;
            editCenters["Auto/Manual"] = themeSlot.FindControl<Button>("AutomaticPatchButton")!.Bounds.Center;
            editCenters["Delete"] = themeSlot.FindControl<Button>("DeleteSlot")!.Bounds.Center;
        }

        public static void SetCenter(Control ctrl, string name)
        {
            string trueName = name.Substring(9);
            bool playMode = name.Contains("PlayMode_");

            Point center = new Point(0, 0);
            if (playMode)
            {
                if (playCenters.TryGetValue(trueName, out Point truePlayPoint))
                {
                    center = truePlayPoint;
                }
            }
            else
            {
                if (editCenters.TryGetValue(trueName, out Point trueEditPoint))
                {
                    center = trueEditPoint;
                }
            }

            double X = center.X - (slotSize.X / 2);
            double Y = center.Y - (slotSize.Y / 2);
            ctrl.RenderTransform = new TranslateTransform(X, Y);
        }

        public static Point GetCenter(string name)
        {
            if(name == "Default")
            {
                Vector2 slotSize = WindowManager.GetSlotSize();
                Point output = new Point(slotSize.X / 2, slotSize.Y / 2);
                return output;
            }

            if(playCenters.TryGetValue(name, out Point play))
            {
                return play;
            }

            if(editCenters.TryGetValue(name, out Point edit))
            {
                return edit;
            }

            return new Point(0, 0);
        }

        public static List<Combo_Centers> GetCenterList()
        {
            if(centers.Count > 0)
            {
                return centers;
            }

            Combo_Centers center = new Combo_Centers();
            center.SetName("Default");
            centers.Add(center);

            Combo_Centers playHeader = new Combo_Centers();
            playHeader.centerName = "-- Play Mode";
            centers.Add(playHeader);

            Combo_Centers toAdd;

            if (playCenters.Count == 0)
            {
                UpdateCenters();
            }

            foreach (var item in playCenters)
            {
                toAdd = new Combo_Centers();
                toAdd.SetName(item.Key);
                centers.Add(toAdd);
            }

            Combo_Centers editHeader = new Combo_Centers();
            editHeader.centerName = "-- Edit Mode";
            centers.Add(editHeader);

            foreach (var item in editCenters)
            {
                toAdd = new Combo_Centers();
                toAdd.SetName(item.Key);
                centers.Add(toAdd);
            }

            return centers;
        }
    }
}
