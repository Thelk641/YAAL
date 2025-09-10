using Avalonia;

using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData.Experimental;
using ReactiveUI;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace YAAL
{
    public static class VisibilityHolder
    {
        public static readonly AttachedProperty<string?> Holder =
            AvaloniaProperty.RegisterAttached<Control, string?>(
                "VisibilityHolder", 
                typeof(VisibilityHolder), 
                defaultValue:null
            );

        public static void SetVisibilityHolder(AvaloniaObject element, string? value) =>
            element.SetValue(Holder, value);

        public static Control? GetVisibilityHolder(AvaloniaObject element)
        {
            string? name = element.GetValue(Holder);
            Control ctrl = element as Control;

            if(name == null)
            {
                return ctrl;
            }
            Control holder = null;

            Visual? parent = ctrl.GetVisualParent();
            while (parent != null)
            {
                if (parent is Control c && c.Name == name)
                {
                    return c;
                }

                parent = parent.GetVisualParent();
            }

            return null;
        }
    }
}
