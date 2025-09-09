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

namespace YAAL
{
    public class Cache_CustomBrush
    {
        [JsonProperty]
        private List<Cached_Brush> _cache = new List<Cached_Brush>();
        [JsonIgnore]
        public Dictionary<string, Border> cache { get
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
        public Border BackgroundHolder
        {
            get
            {
                Border firstLayer = _cache[0].GetLayer();
                Border previousLayer = firstLayer;
                for (int i = 1; i < _cache.Count; i++)
                {
                    Border newLayer = _cache[i].GetLayer();
                    previousLayer.Child = newLayer;
                    previousLayer = newLayer;
                }
                return firstLayer;
            }
        }

        public void AddNewBrush(Cached_Brush brush)
        {
            _cache.Add(brush);
        }

        public void RemoveBrush(Cached_Brush brush)
        {
            _cache.Remove(brush);
        }
    }

    public abstract class Cached_Brush
    {
        public string brushType { get; set; } = "";
        public abstract Border GetLayer();
    }

    public class Cached_ImageBrush : Cached_Brush
    {
        public string imageSource = "";
        public Stretch stretch;
        public TileMode tilemode;
        public double opacity;
        public bool originIsRelative;
        public Vector2 absoluteOrigin;
        public Vector2 relativeOrigin;
        public ImageSettings flipSetting;

        public override Border GetLayer()
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

            if (flipSetting == ImageSettings.FlipX || flipSetting == ImageSettings.FlipXY) 
            {
                scale.ScaleX = -1;
            }

            if (flipSetting == ImageSettings.FlipY || flipSetting == ImageSettings.FlipXY)
            {
                scale.ScaleY = -1;
            }

            if (originIsRelative)
            {
                brush.Transform = scale;
                brush.SourceRect = new RelativeRect(
                    new Rect(relativeOrigin.X, relativeOrigin.Y, 1.0, 1.0),
                    RelativeUnit.Relative
                );
            } else
            {
               TranslateTransform translate = new TranslateTransform { X = absoluteOrigin.X, Y = absoluteOrigin.Y };
               brush.Transform = new TransformGroup { Children = { scale, translate } };
            }

            output.Background = brush;
            return output;
        }
    }

    public class Cached_SolidColorBrush : Cached_Brush
    {
        public Color color;

        public override Border GetLayer()
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
