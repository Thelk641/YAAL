using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;
using Avalonia;
using Avalonia.Media;
using System.ComponentModel;
using Avalonia.Controls;
using System.Numerics;
using Newtonsoft.Json;
using Avalonia.Layout;

namespace YAAL
{
    public class Cache_CustomBrushV2
    {
        [JsonProperty]
        private List<Cached_BrushV2> _cache = new List<Cached_BrushV2>();
        [JsonIgnore]
        public Dictionary<string, Border> cache { get
            {
                Dictionary<string, Border> output = new Dictionary<string, Border>();
                for (int i = 0; i < _cache.Count; i++)
                {
                    string name = "layer" + i;
                    output[name] = _cache[i].GetRawLayer();
                }
                return output;
            } 
        }
        [JsonIgnore]
        public Grid BackgroundHolder
        {
            get
            {
                Grid holder = new Grid();
                holder.Name = "Background Holder";
                for (int i = 0; i < _cache.Count; i++)
                {
                    Border newLayer = _cache[i].GetRawLayer();
                    holder.Children.Add(newLayer);
                }
                return holder;
            }
        }

        public void AddNewBrush(Cached_BrushV2 brush)
        {
            _cache.Add(brush);
        }

        public void RemoveBrush(Cached_BrushV2 brush)
        {
            _cache.Remove(brush);
        }
    }

    public abstract class Cached_BrushV2
    {
        public BrushType brushType { get; set; }
        public Border GetLayer()
        {
            Border output = GetRawLayer();
            CenterBorder(output);
            return output;
        }
        public abstract Border GetRawLayer();
        public bool widthAbsolute;
        public double width = 0;
        public bool heightAbsolute;
        public double height = 0;
        public bool xOffsetAbsolute;
        public double xOffset = 0;
        public bool yOffsetAbsolute;
        public double yOffset = 0;
        public string center = "Default";
        

        public void CenterBorder(Border toCenter)
        {
            Vector2 slotSize = WindowManager.GetSlotSize();
            toCenter.HorizontalAlignment = HorizontalAlignment.Center;
            toCenter.VerticalAlignment = VerticalAlignment.Center;
            if (center != "")
            {
                ThemeManager.SetCenter(toCenter, center);
            }

            TranslateTransform transform = new TranslateTransform();

            if (xOffsetAbsolute)
            {
                transform.X = xOffset;
            } else
            {
                transform.X = slotSize.X * xOffset / 100;
            }

            if (yOffsetAbsolute)
            {
                transform.Y = yOffset;
            }
            else
            {
                transform.Y = slotSize.X * yOffset / 100;
            }

            TransformGroup group = new TransformGroup();

            if(toCenter.RenderTransform is TransformGroup originalGroup)
            {
                originalGroup.Children.Add(transform);
            } else
            {
                if (toCenter.RenderTransform is Transform originalTransform)
                {
                    group.Children.Add(originalTransform);
                }
                group.Children.Add(transform);
                toCenter.RenderTransform = group;
            }

            if (width != 0)
            {
                if (widthAbsolute)
                {
                    toCenter.Width = width;
                }
                else
                {
                    toCenter.Width = slotSize.X * width / 100;
                }
            }

            if(height != 0)
            {
                if (heightAbsolute)
                {
                    toCenter.Height = height;
                } else
                {
                    toCenter.Height = slotSize.Y * height / 100;
                }
            }
            
        }
    }

    public class Cached_ImageBrushV2 : Cached_BrushV2
    {
        public string imageSource = "";
        public Stretch stretch;
        public TileMode tilemode;
        public double opacity;
        public FlipSettings flipSetting;

        public override Border GetRawLayer()
        {
            Border output = new Border();
            ImageBrush brush = new ImageBrush();
            brush.Source = ThemeManager.GetImage(imageSource);
            brush.Stretch = stretch;
            brush.TileMode = tilemode;
            brush.Opacity = opacity;
            brush.AlignmentX = AlignmentX.Center;
            brush.AlignmentY = AlignmentY.Center;

            ScaleTransform scale = new ScaleTransform { ScaleX = 1, ScaleY = 1 };

            if (flipSetting == FlipSettings.FlipX || flipSetting == FlipSettings.FlipXY) 
            {
                scale.ScaleX = -1;
            }

            if (flipSetting == FlipSettings.FlipY || flipSetting == FlipSettings.FlipXY)
            {
                scale.ScaleY = -1;
            }
            output.RenderTransform = scale;

            output.Background = brush;
            return output;
        }
    }

    public class Cached_SolidColorBrushV2 : Cached_BrushV2
    {
        public Color color;

        public override Border GetRawLayer()
        {
            Border output = new Border();
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = color;
            brush.Opacity = 1;
            output.Background = brush;
            return output;
        }
    }
}
