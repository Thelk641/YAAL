using Avalonia;
using Avalonia.Animation;
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
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YAAL.Assets.Scripts;
using static System.Net.Mime.MediaTypeNames;

namespace YAAL
{
    public static partial class ThemeManager 
    {
        public static Dictionary<string, Cache_CustomTheme> themes = new Dictionary<string, Cache_CustomTheme>();
        public static Dictionary<string, WeakReference<Bitmap>> images = new Dictionary<string, WeakReference<Bitmap>>();

        public static Dictionary<string, Point> playCenters = new Dictionary<string, Point>();
        public static Dictionary<string, Point> editCenters = new Dictionary<string, Point>();
        public static List<Combo_Centers> centers = new List<Combo_Centers>();
        private static Vector2 slotSize;

        private static ThemeHolder? slotBackground;
        private static ThemeHolder? slotForeground;

        public static Dictionary<string, Cache_RenderedTheme> renderedThemes = new Dictionary<string, Cache_RenderedTheme>();
        public static Dictionary<Cache_RenderedTheme, List<SlotHolder>> themedSlots = new Dictionary<Cache_RenderedTheme, List<SlotHolder>>();

        public static Cache_CustomTheme CreateNewTheme()
        {
            List<string> themeList = ThemeIOManager.GetThemeList();

            string temporaryName = "New Theme";
            int i = 1;
            while (themeList.Contains(temporaryName))
            {
                temporaryName = "New Theme (" + i + ")";
                ++i;
            }

            Cache_CustomTheme cache = new Cache_CustomTheme();
            cache.name = temporaryName;
            SaveTheme(cache);
            return cache;
        }

        public static void DeleteTheme(string name)
        {
            ThemeIOManager.DeleteTheme(name);
            themes.Remove(name);
        }

        public static string RenameTheme(Cache_CustomTheme cache, string newName)
        {
            if(cache.name == newName)
            {
                return newName;
            }
            themes.Remove(cache.name);
            string trueName = ThemeIOManager.RenameTheme(cache.name, newName);
            foreach (var item in themedSlots)
            {
                if(item.Key.themeName == cache.name)
                {
                    item.Key.themeName = trueName;
                }
            }

            cache.name = trueName;
            SaveTheme(cache);
            
            return trueName;
        }

        public static string DuplicateTheme(Cache_CustomTheme cache)
        {
            string trueName = ThemeIOManager.GetAvailableThemeName(cache.name);
            Cache_CustomTheme newCache = cache.Clone() as Cache_CustomTheme;
            newCache.name = trueName;
            SaveTheme(newCache);
            return trueName;
        }

        public static void SaveTheme(Cache_CustomTheme cache)
        {
            if(cache.name == null || cache.name == "")
            {
                return;
            }
            ThemeIOManager.SaveCustomTheme(cache);
            themes[cache.name] = cache;

            UpdateSlots(cache);
        }

        public static async Task UpdateSlots(Cache_CustomTheme cache)
        {
            List<SlotHolder> toUpdate = new List<SlotHolder>();

            foreach (var item in themedSlots)
            {
                if (item.Key.themeName == cache.name)
                {
                    toUpdate = item.Value;
                    break;
                }
            }

            if(toUpdate.Count > 0)
            {
                await RenderTheme(cache);
                foreach (var slot in toUpdate)
                {
                    slot.Resize();
                    await ApplyTheme(slot, cache.name);
                }
            }
        }

        public static async Task<Cache_RenderedTheme> LoadRenderedTheme(string name)
        {
            if (renderedThemes.ContainsKey(name))
            {
                return renderedThemes[name];
            }

            Cache_RenderedTheme output = new Cache_RenderedTheme();
            Cache_CustomTheme? loadedTheme = ThemeIOManager.LoadCustomTheme(name);
            
            if(loadedTheme == null)
            {
                //TODO : this should build a customTheme based on the default one
                loadedTheme = generalTheme.theme;
            }

            output.themeName = name;
            output.button = loadedTheme.buttonBackground;
            output.topOffset = loadedTheme.topOffset;
            output.bottomOffset = loadedTheme.bottomOffset;

            Bitmap? background = ThemeIOManager.GetRender(loadedTheme, ThemeSettings.backgroundColor);
            var slotsize = WindowManager.GetSlotSize();
            var backgroundSize = new Vector2(slotsize.X, slotsize.Y + output.topOffset + output.bottomOffset);


            Bitmap? foreground = ThemeIOManager.GetRender(loadedTheme, ThemeSettings.foregroundColor);
            var foregroundSize = WindowManager.GetSlotForegroundSize();

            if (background == null 
                || background.Size.Width != backgroundSize.X 
                || background.Size.Height != backgroundSize.Y
                || foreground == null
                || foreground.Size.Width != foregroundSize.X
                || foreground.Size.Height != foregroundSize.Y
                )
            {
                Dictionary<ThemeSettings, Bitmap> render = await RenderTheme(loadedTheme);
                if (!render.ContainsKey(ThemeSettings.backgroundColor))
                {
                    return output;
                }
                background = render[ThemeSettings.backgroundColor];
                background = render[ThemeSettings.foregroundColor];
            }

            output.background = new WeakReference<Bitmap>(background);
            output.foreground = new WeakReference<Bitmap>(foreground);

            renderedThemes[name] = output;

            return output;
        }

        public static Cache_CustomTheme LoadTheme(string name)
        {
            if (themes.TryGetValue(name, out var result))
            {
                return result;
            }

            Cache_CustomTheme? output = ThemeIOManager.LoadCustomTheme(name);
            if (output != null)
            {
                themes[name] = output;
                return output;
            }

            if(generalTheme != null)
            {
                return generalTheme.theme;
            }


            ErrorManager.ThrowError(
                "ThemeManager - Couldn't find themes",
                "Couldn't find theme " + name + ". Did you manually delete or rename them maybe ?"
                );

            return DefaultManager.launcherTheme;
        }

        public static async Task ApplyTheme(SlotHolder slot, string theme)
        {
            if(theme == "")
            {
                AutoTheme.SetTheme(slot.GetBackgrounds(), ThemeSettings.backgroundColor);
                
                foreach (var item in slot.GetForegrounds())
                {
                    AutoTheme.SetTheme(item, ThemeSettings.foregroundColor);
                }

                foreach (var item in slot.GetButtons())
                {
                    AutoTheme.SetTheme(item, ThemeSettings.buttonColor);
                }

                foreach (var item in slot.GetComboBox())
                {
                    AutoTheme.SetTheme(item, ThemeSettings.buttonColor);
                    if (item.GetVisualDescendants().OfType<ContentPresenter>().FirstOrDefault() is ContentPresenter presenter
                        && item.Background is SolidColorBrush brush)
                    {
                        presenter.Background = new SolidColorBrush(AutoColor.Darken(brush.Color));
                    }
                }

                return;
            }

            Cache_RenderedTheme cache = await LoadRenderedTheme(theme);

            if(cache.background.TryGetTarget(out Bitmap backgroundImage) && backgroundImage != null)
            {
                ImageBrush backgroundBrush = new ImageBrush(backgroundImage);
                backgroundBrush.AlignmentX = AlignmentX.Center;
                backgroundBrush.AlignmentY = AlignmentY.Center;
                backgroundBrush.Stretch = Stretch.Fill;
                var slotBackground = slot.GetBackgrounds();
                AutoTheme.SetTheme(slotBackground, ThemeSettings.off);
                slotBackground.Background = backgroundBrush;

            }

            if(cache.foreground.TryGetTarget(out Bitmap foregroundImage) && foregroundImage != null)
            {
                ImageBrush foregroundBrush = new ImageBrush(foregroundImage);
                foregroundBrush.AlignmentX = AlignmentX.Center;
                foregroundBrush.AlignmentY = AlignmentY.Center;
                foregroundBrush.Stretch = Stretch.Fill;

                foreach (var item in slot.GetForegrounds())
                {
                    //Trace.WriteLine(item.Name + " / " + foregroundImage.Size);
                    AutoTheme.SetTheme(item, ThemeSettings.off);
                    item.Background = foregroundBrush;
                }
            }


            foreach (var item in slot.GetButtons())
            {
                AutoTheme.SetTheme(item, ThemeSettings.off);
                item.Background = cache.button;
            }

            foreach (var item in slot.GetComboBox())
            {
                AutoTheme.SetTheme(item, ThemeSettings.off);
                item.Background = cache.button;
                if (item.GetVisualDescendants().OfType<ContentPresenter>().FirstOrDefault() is ContentPresenter presenter)
                {
                    presenter.Background = new SolidColorBrush(AutoColor.Darken(cache.button.Color));
                }
            }

            if (!themedSlots.ContainsKey(cache))
            {
                themedSlots[cache] = new List<SlotHolder>();
            }

            if (!themedSlots[cache].Contains(slot))
            {
                themedSlots[cache].Add(slot);
            }
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

            Bitmap? readBitmap = ThemeIOManager.ReadImage(name);
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
            string output = ThemeIOManager.CopyImageToDefaultFolder(path, themeName);
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
                string setting = SettingsManager.GetSetting(GeneralSettings.scaleModifier).TrimEnd('f');
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
                    Cache_CustomTheme? cacheTheme = ThemeIOManager.LoadCustomTheme(themeName);
                    float offset = 0f;
                    if(cacheTheme != null)
                    {
                        offset = cacheTheme.topOffset + cacheTheme.bottomOffset;
                    }

                    var slotSize = WindowManager.GetSlotSize();
                    defaultSize = new Vector2(slotSize.X, slotSize.Y + offset);
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
                ThemeIOManager.SaveImage(renderer, themeName, category);
            }

            return renderer;
        }

        public async static Task<Dictionary<ThemeSettings, Bitmap>> RenderTheme(Cache_CustomTheme cache)
        {
            Dictionary<ThemeSettings, Bitmap> output = new Dictionary<ThemeSettings, Bitmap>();
            if (themes.ContainsKey(cache.name))
            {
                themes.Remove(cache.name);
            }

            if (renderedThemes.ContainsKey(cache.name))
            {
                renderedThemes.Remove(cache.name);
            }

            if (slotBackground != null)
            {
                slotBackground.Close();
                slotBackground = null;
            }

            if(slotForeground != null)
            {
                slotForeground.Close();
                slotForeground = null;
            }

            Bitmap background = await UpdateTheme(cache.background, cache.name, ThemeSettings.backgroundColor, true);
            Bitmap foreground = await UpdateTheme(cache.foreground, cache.name, ThemeSettings.foregroundColor, true);

            output[ThemeSettings.backgroundColor] = background;
            output[ThemeSettings.foregroundColor] = foreground;

            return output;
        }

        public async static Task<Bitmap> UpdateTheme(Cache_LayeredBrush brush, string themeName, ThemeSettings setting, bool save = true)
        {
            return await UpdateTheme(brush, LoadTheme(themeName), setting, save);
        }

        public async static Task<Bitmap> UpdateTheme(Cache_LayeredBrush brush, Cache_CustomTheme theme, ThemeSettings setting, bool save = true)
        {
            Trace.WriteLine("Updating theme");
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
                        slotBackground.AdjustSize(false, theme.topOffset, theme.bottomOffset);
                    }

                    holder = slotBackground!.ExampleBackgroundContainer;
                    container = slotBackground.Back;
                    //temporarySize = WindowManager.GetSlotSize();
                    temporarySize = new Vector2((float)slotBackground.Width, (float)slotBackground.Height);
                    Trace.WriteLine(temporarySize);
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
                    float originalWidth = (float)layer.width;
                    float originalHeight = (float)layer.height;

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
                border.Height = size.Y;
                border.Width = size.X;
            }

            

            foreach (var item in centers)
            {
                await SetCenter(item.Key, item.Value, theme.topOffset, theme.bottomOffset, container, setting, sizes[item.Key]);

            }


            Bitmap output = Render(holder, theme.name, setting, save);
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
            }, DispatcherPriority.Render
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

        public static async Task SetCenter(Control ctrl, string centerName, int topOffset, int bottomOffset, Canvas container, ThemeSettings setting, Vector2 realSize)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Point baseCenter = GetCenter(centerName, topOffset, bottomOffset, setting);

                if(setting == ThemeSettings.foregroundColor)
                {
                    baseCenter = new Point(baseCenter.X, GetCenter("Tracker", topOffset, bottomOffset, setting).Y);
                }

                Point offsetCenter = new Point(baseCenter.X, baseCenter.Y);
                double X = offsetCenter.X - (realSize.X / 2);
                double Y = offsetCenter.Y - (realSize.Y / 2);
                Point transformedPoint = ComputePoint(new Point(X, Y), container, (ctrl.Parent as Canvas)!);
                Canvas.SetLeft(ctrl, transformedPoint.X);
                Canvas.SetTop(ctrl, transformedPoint.Y);
                Trace.WriteLine("Bounds : " 
                    + Canvas.GetLeft(ctrl) + "/"
                    + Canvas.GetRight(ctrl) + "/"
                    + Canvas.GetTop(ctrl) + "/"
                    + Canvas.GetBottom(ctrl));
            }, DispatcherPriority.Loaded);
        }

        public static Point GetCenter(string name, int topOffset, int bottomOffset, ThemeSettings setting)
        {
            if(name == "Default")
            {
                Vector2 slotSize;
                if (setting == ThemeSettings.foregroundColor)
                {
                    slotSize = WindowManager.GetSlotForegroundSize();
                } else
                {
                    slotSize = WindowManager.GetSlotSize();
                }

                Point output = new Point(slotSize.X / 2, (slotSize.Y + topOffset) / 2);
                
                return output;
            } else if (name == "True center")
            {
                Vector2 defaultSlotSize = WindowManager.GetSlotSize();
                Vector2 slotSize = new Vector2(defaultSlotSize.X, defaultSlotSize.Y + topOffset + bottomOffset);
                Point output = new Point(slotSize.X / 2, slotSize.Y / 2);
                return output;
            }

            if (playCenters.TryGetValue(name, out Point play))
            {
                return new Point(play.X, play.Y + topOffset);
            }

            if(editCenters.TryGetValue(name, out Point edit))
            {
                return new Point(edit.X, edit.Y + topOffset);
            }

            return new Point(0, 0);
        }

        public static List<Combo_Centers> GetCenterList()
        {
            if(centers.Count > 0)
            {
                return centers;
            }

            Combo_Centers defaultCenter = new Combo_Centers();
            defaultCenter.SetName("Default");
            centers.Add(defaultCenter);

            Combo_Centers center = new Combo_Centers();
            center.SetName("True center");
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
