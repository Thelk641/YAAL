using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace YAAL
{
    public class ScalableWindow : Avalonia.Controls.Window
    {
        public double baseMinHeight;
        public double baseMaxHeight;
        public double baseMinWidth;
        public double baseMaxWidth;
        public double baseHeight;
        public double baseWidth;
        public double previousZoom = 1;

        private ScaleTransform scale = new ScaleTransform();

        public ScalableWindow()
        {
            this.Opened += (_, _) =>
            {
                if (this.FindControl<LayoutTransformControl>("Controller") is var Controller)
                {
                    Controller.LayoutTransform = scale;
                }
                baseHeight = base.Height;
                baseWidth = base.Width;
                baseMinHeight = base.MinHeight;
                baseMaxHeight = base.MaxHeight;
                baseMinWidth = base.MinWidth;
                baseMaxWidth = base.MaxWidth;
                AdjustZoom();
            };

            App.Settings.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(App.Settings.Zoom))
                {
                    AdjustZoom();
                }
            };
        }

        public void AdjustZoom()
        {
            double newZoom = App.Settings.Zoom;
            scale.ScaleX = newZoom;
            scale.ScaleY = newZoom;

            if (baseMinHeight != 0)
            {
                base.MinHeight = baseMinHeight * newZoom;
            }

            if (baseMaxHeight != double.PositiveInfinity)
            {
                base.MaxHeight = baseMaxHeight * newZoom;
            }

            if (baseMinWidth != 0)
            {
                base.MinWidth = baseMinWidth * newZoom;
            }

            if (baseMaxWidth != double.PositiveInfinity)
            {
                base.MaxWidth = baseMaxWidth * newZoom;
            }

            base.Height = base.Height / previousZoom * newZoom;
            base.Width = base.Width / previousZoom * newZoom;
            base.InvalidateMeasure();
            base.InvalidateArrange();
            previousZoom = newZoom;
        }
    }
}
