using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
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
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YAAL.Assets.Scripts;
using static System.Net.Mime.MediaTypeNames;

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

        private static ThemeHolder? slotBackground;
        private static ThemeHolder? slotForeground;

        static ThemeManager()
        {
            // TODO : this shouldn't be hardcoded
            themes["General Theme"] = DefaultManager.theme;
        }

        public static void SaveTheme(Cache_CustomTheme cache)
        {
            IOManager.SaveCustomTheme(cache);
        }

        public static Cache_CustomTheme LoadTheme(string name)
        {
            if (themes.TryGetValue(name, out var result))
            {
                return result;
            }

            Cache_CustomTheme? output = IOManager.LoadCustomTheme(name);
            if (output != null)
            {
                themes[name] = output;
                return output;
            }

            if (themes.TryGetValue(defaultTheme, out var defaultResult))
            {
                return defaultResult;
            }

            output = IOManager.LoadCustomTheme(defaultTheme);
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


        public static void ApplyTheme(Control container, string theme)
        {
            ThemeSettings category = ThemeCategory.GetThemeCategory(container);
            Cache_CustomTheme cache = LoadTheme(theme);

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


            if(container is Border border)
            {
                Bitmap? fetchBackground = GetBackgroundImage(theme);
                if(fetchBackground is Bitmap newBackground)
                {
                    ImageBrush backgroundBrush = new ImageBrush(newBackground);
                    backgroundBrush.AlignmentX = AlignmentX.Center;
                    backgroundBrush.AlignmentY = AlignmentY.Center;
                    backgroundBrush.Stretch = Stretch.Fill;
                    border.Background = backgroundBrush;
                }
            }

            ErrorManager.ThrowError(
                "ThemeManager - Tried to apply theme to invalid object type",
                "ApplyTheme was called on object " + container + "which isn't Button, ComboBox or Panel. Please report this."
                );
        }

        public static Bitmap? GetBackgroundImage(string themeName)
        {
            if (!themes.ContainsKey(themeName))
            {
                return null;
            }

            // Have we tried to access this recently ?
            if (images.TryGetValue(themeName, out var oldRef) && oldRef.TryGetTarget(out var oldRender))
            {
                Vector2 slotSize = WindowManager.GetSlotSize();
                if(oldRender.Size.Width == slotSize.X && oldRender.Size.Height == slotSize.Y)
                {
                    return oldRender;
                }
            }

            // Have we already rendered this theme at these dimensions ?
            Bitmap? preRender = IOManager.GetRender(themeName);
            if(preRender is Bitmap toOutput)
            {
                return toOutput;
            }

            // Let's render it at these new dimensions !
            RenderTheme(themeName);
            if(images.TryGetValue(themeName, out var weakRef) && weakRef.TryGetTarget(out var output))
            {
                return output;
            }

            // Something went wrong, nope out of there
            return null;
        }

        public static Cache_CustomTheme GetDefaultTheme()
        {
            if (themes.TryGetValue(defaultTheme, out var defaultResult))
            {
                return defaultResult;
            }

            Cache_CustomTheme? output = IOManager.LoadCustomTheme(defaultTheme);
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

        public static Bitmap? GetImage(string name, int imageWidth = 0, int imageHeight = 0)
        {
            if(name == "")
            {
                return null;
            }

            if(images.TryGetValue(name, out var weakRef))
            {
                if(weakRef.TryGetTarget(out var image))
                {
                    if(imageWidth != 0 && imageHeight != 0)
                    {
                        return image.CreateScaledBitmap(new PixelSize(imageWidth, imageHeight));
                    }
                    return image;
                }

                images.Remove(name);
            }

            Bitmap? readBitmap = IOManager.ReadImage(name);
            if (readBitmap != null)
            {
                images[name] = new WeakReference<Bitmap>(readBitmap);
                if (imageWidth != 0 && imageHeight != 0)
                {
                    return readBitmap.CreateScaledBitmap(new PixelSize(imageWidth, imageHeight));
                }
                return readBitmap;
            }
            return null;
        }

        public static string AddNewImage(string path, string themeName)
        {
            string output = IOManager.CopyImageToDefaultFolder(path, themeName);
            if(output != "")
            {
                return output;
            } else
            {
                return path;
            }
        }

        public static Bitmap Render(Control ctrl, string themeName, ThemeSettings category, bool save = true)
        {
            float sharpnessMultiplier = 2f;
            try
            {
                string setting = IOManager.GetSetting(GeneralSettings.scaleModifier).TrimEnd('f');
                float parsed = float.Parse(setting, CultureInfo.InvariantCulture.NumberFormat);
                if (parsed != 0)
                {
                    sharpnessMultiplier = parsed;
                }
            }
            catch (Exception e)
            {
                ErrorManager.ThrowError(
                    "ThemeManager - Invalid sharpness multiplier",
                    "Trying to parse your sharpness multiplier setting raised the following exception : " + e.Message);
            }
            
            Vector2 dpi = new Vector2(96 * sharpnessMultiplier, 96 * sharpnessMultiplier);
            Vector2 defaultSize;

            switch (category)
            {
                case ThemeSettings.rendered:
                case ThemeSettings.backgroundColor:
                    defaultSize = WindowManager.GetSlotSize();
                    break;
                case ThemeSettings.foregroundColor:
                    defaultSize = WindowManager.GetSlotForegroundSize();
                    break;
                default:
                    defaultSize = WindowManager.GetWindowSize();
                    break;
            }
            Size renderSize = new Size(defaultSize);

            int pixelWidth = (int)Math.Ceiling(renderSize.Width * dpi.X / 96.0);
            int pixelHeight = (int)Math.Ceiling(renderSize.Height * dpi.Y / 96.0);
            var pixelSize = new PixelSize(pixelWidth, pixelHeight);

            var renderer = new RenderTargetBitmap(pixelSize, dpi);
            var finalRect = new Rect(renderSize);

            ctrl.Measure(renderSize);
            ctrl.Arrange(finalRect);
            renderer.Render(ctrl);

            if (save)
            {
                IOManager.SaveImage(renderer, themeName, category);
            }

            return renderer;
        }

        public async static void RenderTheme(string themeName)
        {
            if (!themes.ContainsKey(themeName))
            {
                ErrorManager.ThrowError(
                    "ThemeManager - Couldn't find a theme",
                    "Tried to render theme " + themeName + " but it doesn't appear to exist."
                    );
                return;
            }


            if(slotBackground != null)
            {
                slotBackground.Close();
                slotBackground = null;
            }

            if(slotForeground != null)
            {
                slotForeground.Close();
                slotForeground = null;
            }

            Cache_CustomTheme cache = themes[themeName];
            Bitmap background = await UpdateTheme(cache.background, themeName, ThemeSettings.backgroundColor, false);
            Bitmap foreground = await UpdateTheme(cache.foreground, themeName, ThemeSettings.foregroundColor, false);

            EmptySlot holder = new EmptySlot();
            Vector2 slotSize = WindowManager.GetSlotSize();
            holder.Width = slotSize.X;
            holder.Height = slotSize.Y;

            ImageBrush backgroundBrush = new ImageBrush(background);
            backgroundBrush.AlignmentX = AlignmentX.Center;
            backgroundBrush.AlignmentY = AlignmentY.Center;
            backgroundBrush.Stretch = Stretch.Fill;

            ImageBrush foregroundBrush = new ImageBrush(foreground);
            foregroundBrush.AlignmentX = AlignmentX.Center;
            foregroundBrush.AlignmentY = AlignmentY.Center;
            foregroundBrush.Stretch = Stretch.Fill;

            holder.Back.Background = backgroundBrush;
            holder.Foreground.Background = foregroundBrush;

            Bitmap output = Render(holder.Back, themeName, ThemeSettings.rendered, true);
            images[themeName] = new WeakReference<Bitmap>(output);
        }

        public async static Task<Bitmap> UpdateTheme(Cache_LayeredBrush brush, string themeName, ThemeSettings setting, bool save = true)
        {
            Cache_CustomTheme theme = LoadTheme(themeName);
            Canvas holder;
            Canvas container;
            Vector2 temporarySize;
            switch (setting)
            {
                case ThemeSettings.backgroundColor:
                    if (slotBackground == null)
                    {
                        slotBackground = new ThemeHolder(false, theme.topOffset, theme.bottomOffset);
                        slotBackground.Opacity = 0;
                        slotBackground.IsHitTestVisible = false;

                        Debouncer.Debounce(() => { slotBackground.Close(); slotBackground = null; }, 60f);
                    }
                    else
                    {
                        slotBackground.ExampleBackgroundContainer.Children.Clear();
                    }
                    holder = slotBackground!.ExampleBackgroundContainer;
                    container = slotBackground.Back;
                    temporarySize = WindowManager.GetSlotSize();
                    break;
                case ThemeSettings.foregroundColor:
                    if (slotForeground == null)
                    {
                        slotForeground = new ThemeHolder(true, theme.topOffset, theme.bottomOffset);
                        slotForeground.Opacity = 0;
                        slotForeground.IsHitTestVisible = false;

                        Debouncer.Debounce(() => { slotForeground.Close(); slotForeground = null; }, 60f);
                    }
                    else
                    {
                        slotForeground.ExampleBackgroundContainer.Children.Clear();
                    }
                    holder = slotForeground.ExampleBackgroundContainer;
                    container = slotForeground.Back;
                    temporarySize = WindowManager.GetSlotForegroundSize();
                    break;
                default:
                    return null;
            }

            Dictionary<Border, string> centers = new Dictionary<Border, string>();
            Dictionary<Border, Vector2> sizes = new Dictionary<Border, Vector2>();

            foreach (var item in brush.GetLayers())
            {
                Cached_Layer layer = item;
                var border = layer.GetLayer();
                border.IsVisible = true;
                centers[border] = layer.center;
                holder.Children.Add(border);

                Vector2 size;
                if(layer is Cached_ImageLayer image)
                {
                    float width = image.imageWidth;
                    if (!image.absoluteImageWidth)
                    {
                        width *= temporarySize.X / 100;
                    }
                    float height = image.imageHeight;
                    if (!image.absoluteImageHeight)
                    {
                        height *= temporarySize.Y / 100;
                    }
                    size = new Vector2(width, height);
                } else
                {
                    float width = (float)layer.width;
                    if (!layer.widthAbsolute)
                    {
                        width *= temporarySize.X / 100;
                    }
                    float height = (float)layer.height;
                    if (!layer.heightAbsolute)
                    {
                        height *= temporarySize.Y / 100;
                    }
                    
                    size = new Vector2((float)Math.Round(width), (float)Math.Round(height));
                }
                sizes[border] = size;
            }

            foreach (var item in centers)
            {
                await SetCenter(item.Key, item.Value, theme.topOffset, container, setting, sizes[item.Key]);
            }

            Bitmap output = Render(holder, themeName, setting, save);
            return output;
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
                playCenters["Play Button"] = ComputeCenter(themeSlot.FindControl<Button>("RealPlay")!, themeSlot);
                playCenters["Slot Name"] = ComputeCenter(themeSlot.FindControl<TextBlock>("_SlotName")!, themeSlot);
                playCenters["Tool Name"] = ComputeCenter(themeSlot.FindControl<ComboBox>("ToolSelect")!, themeSlot);
                playCenters["Start Tool"] = ComputeCenter(themeSlot.FindControl<Button>("StartTool")!, themeSlot);
                playCenters["Settings"] = ComputeCenter(themeSlot.FindControl<Button>("Edit")!, themeSlot);
                playCenters["Tracker"] = ComputeCenter(themeSlot.FindControl<ScrollViewer>("Viewer")!, themeSlot);
                playCenters["Update"] = ComputeCenter(themeSlot.FindControl<Button>("UpdateItems")!, themeSlot);

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
            Point output = matrix.Value.Transform(localCenter);

            if(toFind.Name == "Viewer")
            {
                output = new Point(output.X, WindowManager.GetSlotForegroundSize().Y / 2);
            }
            return output;
        }

        private static Point ComputePoint(Point toCompute, Control origin, Control relativeTo)
        {
            var matrix = origin.TransformToVisual(relativeTo);
            if (matrix is null)
            {
                return new Point(0, 0); // not attached yet or not transformable
            }

            Point output = matrix.Value.Transform(toCompute);
            return output;
        }

        public static Task SetCenter(Control ctrl, string centerName, int topOffset, Canvas container, ThemeSettings setting, Vector2 realSize)
        {
            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            Dispatcher.UIThread.Post(() =>
            {
                Point baseCenter = GetCenter(centerName);

                if(setting == ThemeSettings.foregroundColor)
                {
                    baseCenter = new Point(baseCenter.X, GetCenter("Tracker").Y);
                }

                Point offsetCenter = new Point(baseCenter.X, baseCenter.Y + topOffset);
                double X = offsetCenter.X - (realSize.X / 2);
                double Y = offsetCenter.Y - (realSize.Y / 2);
                Point transformedPoint = ComputePoint(new Point(X, Y), container, (ctrl.Parent as Canvas)!);
                Canvas.SetLeft(ctrl, transformedPoint.X);
                Canvas.SetTop(ctrl, transformedPoint.Y);

                tcs.SetResult(null);
            }, DispatcherPriority.Render);

            return tcs.Task;
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
