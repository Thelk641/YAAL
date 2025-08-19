using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace YAAL
{
    public static class AutoColor
    {
        public static Control FindNearestBackground(Control ctrl, out IBrush brush)
        {

            Control current = ctrl;

            while(current != null)
            {
                brush = GetBrush(current);
                if(brush != null)
                {
                    return current;
                }

                Control parent = current.GetVisualParent() as Control;
                if(parent != null)
                {
                    current = parent;
                } else
                {
                    current = current.Parent as Control;
                }
            }

            brush = null;
            return null;
        }

        public static IBrush? GetBrush(Control ctrl)
        {
            switch (ctrl)
            {
                case TemplatedControl templated when templated.Background != null:
                    return templated.Background;
                case Panel panel when panel.Background != null:
                    return panel.Background;
                case Border border when border.Background != null:
                    return border.Background;
            }
            return null;
        }
    }
}
