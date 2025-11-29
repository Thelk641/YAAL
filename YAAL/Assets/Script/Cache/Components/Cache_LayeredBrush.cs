using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static YAAL.AsyncSettings;

namespace YAAL
{
    public class Cache_LayeredBrush
    {
        [JsonProperty]
        private List<Cached_Layer> _cache = new List<Cached_Layer>();
        [JsonIgnore]
        public Dictionary<string, Border> layers { get
            {
                Dictionary<string, Border> output = new Dictionary<string, Border>();
                for (int i = 0; i < _cache.Count; i++)
                {
                    string name = "layer" + i;
                    output[name] = _cache[i].GetLayer();
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
                    Border newLayer = _cache[i].GetLayer();
                    holder.Children.Add(newLayer);
                }
                return holder;
            }
        }

        public void AddNewBrush(Cached_Layer brush, int index = -1)
        {
            if(index == -1)
            {
                _cache.Add(brush);
            } else
            {
                _cache.Insert(index, brush);
            }
        }

        public void UpdateBrush(Cached_Layer oldBrush, Cached_Layer newBrush)
        {
            int index = _cache.IndexOf(oldBrush);
            if(index == -1)
            {
                Debug.WriteLine(_cache.Contains(oldBrush));
            }
            _cache.Remove(oldBrush);
            _cache.Insert(index, newBrush);
        }

        public void RemoveBrush(Cached_Layer brush)
        {
            _cache.Remove(brush);
        }

        public bool MoveBrushUp(Cached_Layer brush)
        {
            int index = _cache.IndexOf(brush);
            if(index > 0)
            {
                _cache.Remove(brush);
                _cache.Insert(index - 1, brush);
                return true;
            }
            return false;
        }

        public bool MoveBrushDown(Cached_Layer brush)
        {
            int index = _cache.IndexOf(brush);
            if (index < _cache.Count - 1)
            {
                _cache.Remove(brush);
                _cache.Insert(index + 1, brush);
                return true;
            }
            return false;
        }

        public List<Cached_Layer> GetLayers()
        {
            return _cache;
        }
    }

    public abstract class Cached_Layer
    {
        [JsonProperty]
        public BrushType brushType { get {
                if(this is Cached_ImageLayer)
                {
                    return BrushType.Image;
                } else
                {
                    return BrushType.Color;
                }
            } }


        public bool isForeground;
        public Border GetLayer()
        {
            Border output = GetRawLayer();
            CenterBorder(output);
            return output;
        }

        public abstract Border GetRawLayer();
        public bool widthAbsolute = false;
        public double width = 100;
        public bool heightAbsolute = false;
        public double height = 100;
        public bool xOffsetAbsolute = false;
        public double xOffset = 0;
        public bool yOffsetAbsolute = false;
        public double yOffset = 0;
        public string center = "Default";

        public event PropertyChangedEventHandler? PropertyChanged;

        public void CenterBorder(Border toCenter)
        {
            Vector2 slotSize;

            if (isForeground)
            {
                slotSize = WindowManager.GetSlotForegroundSize();
            } else
            {
                slotSize = WindowManager.GetSlotSize();
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

            if (height != 0)
            {
                if (heightAbsolute)
                {
                    toCenter.Height = height;
                }
                else
                {
                    toCenter.Height = slotSize.Y * height / 100;
                }
            }

            toCenter.HorizontalAlignment = HorizontalAlignment.Center;
            toCenter.VerticalAlignment = VerticalAlignment.Center;

            TranslateTransform transform = new TranslateTransform();
            double trueXOffset = xOffset;
            double trueYOffset = yOffset;

            if (!xOffsetAbsolute)
            {
                trueXOffset *= slotSize.X / 100;
            }

            if (!yOffsetAbsolute)
            {
                trueYOffset *= slotSize.Y / 100;
            }

            if (this is Cached_ImageLayer cache && cache.stretch == Stretch.None && toCenter.Background is ImageBrush image)
            {
                double dx = image.DestinationRect.Rect.Width / 4;
                double dy = image.DestinationRect.Rect.Height / 4;
                switch (image.AlignmentX)
                {
                    case AlignmentX.Left:
                        trueXOffset += image.DestinationRect.Rect.Width / 4;
                        break;
                    case AlignmentX.Right:
                        trueXOffset -= image.DestinationRect.Rect.Width / 4;
                        break;
                }
                

                switch (image.AlignmentY)
                {
                    case AlignmentY.Top:
                        trueYOffset += image.DestinationRect.Rect.Height / 4;
                        break;
                    case AlignmentY.Bottom:
                        trueYOffset += image.DestinationRect.Rect.Height / 4;
                        break;
                }
            }

            transform.X = trueXOffset;
            transform.Y = trueYOffset;

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
            
        }
    }

    public class Cached_ImageLayer : Cached_Layer
    {
        public string imageSource = "";
        public Stretch stretch;
        public TileMode tilemode;
        public double opacity = 1;
        public FlipSettings flipSetting;
        public float imageWidth = 100;
        public float imageHeight = 100;
        public bool absoluteImageWidth;
        public bool absoluteImageHeight;

        public override Border GetRawLayer()
        {
            float trueWidth = imageWidth;
            float trueHeight = imageHeight;

            Vector2 slotSize;

            if (isForeground)
            {
                slotSize = WindowManager.GetSlotForegroundSize();
            }
            else
            {
                slotSize = WindowManager.GetSlotSize();
            }

            if (!absoluteImageHeight)
            {
                trueHeight = (int)Math.Round(trueHeight * slotSize.Y / 100);
            }

            if (!absoluteImageWidth)
            {
                trueWidth = (int)Math.Round(trueWidth * slotSize.X / 100);
            }



            Border output = new Border();
            AutoTheme.SetTheme(output, ThemeSettings.off);
            ImageBrush brush = new ImageBrush();
            brush.Source = ThemeManager.GetImage(imageSource, (int)trueWidth, (int)trueHeight);
            brush.Stretch = stretch;
            brush.TileMode = tilemode;
            brush.Opacity = opacity;
            
            brush.DestinationRect = new RelativeRect(0, 0, (int)trueWidth, (int)trueHeight, RelativeUnit.Absolute);

            if(tilemode == TileMode.None)
            {
                brush.AlignmentX = AlignmentX.Center;
                brush.AlignmentY = AlignmentY.Center;
            } else
            {
                brush.AlignmentX = AlignmentX.Left;
                brush.AlignmentY = AlignmentY.Top;
            }

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

    public class Cached_ColorLayer : Cached_Layer
    {
        public Color color;

        public override Border GetRawLayer()
        {
            Border output = new Border();
            AutoTheme.SetTheme(output, ThemeSettings.off);
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = color;
            brush.Opacity = 1;
            output.Background = brush;
            return output;
        }
    }
}
