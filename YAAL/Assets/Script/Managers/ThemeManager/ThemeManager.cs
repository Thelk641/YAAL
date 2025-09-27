using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static Dictionary<Control, Grid> themeContainers = new Dictionary<Control, Grid>();
        public static string defaultTheme = "General Theme";

        public static Dictionary<string, Point> playCenters = new Dictionary<string, Point>();
        public static Dictionary<string, Point> editCenters = new Dictionary<string, Point>();
        public static List<Combo_Centers> centers = new List<Combo_Centers>();
        private static Vector2 slotSize;

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

                Grid layer0 = null;

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
            if(name == "")
            {
                return null;
            }

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

        public static string AddNewImage(string path)
        {
            string output = IOManager.CopyImageToDefaultFolder(path);
            if(output != "")
            {
                return output;
            } else
            {
                return path;
            }
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
            var root = new Window();
            root.Opacity = 0;
            root.IsHitTestVisible = false;
            Vector2 size = WindowManager.GetWindowSize();
            root.Width = size.X;
            root.Height = size.Y;


            playCenters = new Dictionary<string, Point>();
            editCenters = new Dictionary<string, Point>();

            ThemeSlot themeSlot = new ThemeSlot();
            root.Content = themeSlot;
            themeSlot.Measure(Size.Infinity);
            slotSize = WindowManager.GetSlotSize();
            Rect desiredSize = new Rect(0, 0, slotSize.X, slotSize.Y);
            themeSlot.Arrange(desiredSize);

            Dispatcher.UIThread.Post(() =>
            {
                Debug.WriteLine("themeSlot size : " + themeSlot.Bounds);
                playCenters["Play Button"] = ComputeCenter(themeSlot.FindControl<Button>("RealPlay")!, themeSlot);
                playCenters["Slot Name"] = ComputeCenter(themeSlot.FindControl<TextBlock>("_SlotName")!, themeSlot);
                playCenters["Tool Name"] = ComputeCenter(themeSlot.FindControl<ComboBox>("ToolSelect")!, themeSlot);
                playCenters["Start Tool"] = ComputeCenter(themeSlot.FindControl<Button>("StartTool")!, themeSlot);
                playCenters["Settings"] = ComputeCenter(themeSlot.FindControl<Button>("Edit")!, themeSlot);
                playCenters["Tracker"] = ComputeCenter(themeSlot.FindControl<ScrollViewer>("Viewer")!, themeSlot);

                themeSlot.SwitchMode();

                editCenters["Slot Selector"] = ComputeCenter(themeSlot.FindControl<ComboBox>("SlotSelector")!, themeSlot);
                editCenters["Download"] = ComputeCenter(themeSlot.FindControl<Button>("DownloadPatch")!, themeSlot);
                editCenters["Save"] = ComputeCenter(themeSlot.FindControl<Button>("DoneEditing")!, themeSlot);
                editCenters["Launcher Selector"] = ComputeCenter(themeSlot.FindControl<ComboBox>("SelectedLauncher")!, themeSlot);
                editCenters["Version Selector"] = ComputeCenter(themeSlot.FindControl<ComboBox>("SelectedVersion")!, themeSlot);
                editCenters["Auto/Manual"] = ComputeCenter(themeSlot.FindControl<Button>("AutomaticPatchButton")!, themeSlot);
                editCenters["Delete"] = ComputeCenter(themeSlot.FindControl<Button>("DeleteSlot")!, themeSlot);
                root.Close();
            }
            );
        }

        private static Point ComputeCenter(Control toFind, Control relativeTo)
        {
            // If they share the same parent, Bounds.Center is already relative to that parent
            if (toFind.GetVisualParent() == relativeTo)
            {
                return toFind.Bounds.Center;
            }
                

            var matrix = toFind.TransformToVisual(relativeTo);
            if (matrix is null)
            {
                return new Point(0, 0); // not attached yet or not transformable
            }

            // Use local coordinates: top-left of control local space is (0,0)
            var localCenter = new Point(toFind.Bounds.Width / 2.0, toFind.Bounds.Height / 2.0);
            return matrix.Value.Transform(localCenter);
        }

        public static void SetCenter(Control ctrl, string name, int topOffset)
        {
            Dispatcher.UIThread.Post(() =>
            {
                Point baseCenter = GetCenter(name);
                Point offsetCenter = new Point(baseCenter.X, baseCenter.Y + topOffset);
                double X = offsetCenter.X - (ctrl.Width / 2);
                double Y = offsetCenter.Y - (ctrl.Height / 2);

                Canvas.SetLeft(ctrl, X);
                Canvas.SetTop(ctrl, Y);
            });
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
