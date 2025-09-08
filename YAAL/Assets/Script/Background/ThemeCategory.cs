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
using ReactiveUI;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace YAAL
{
    public static class ThemeCategory
    {
        public static readonly AttachedProperty<ThemeSettings> Category =
            AvaloniaProperty.RegisterAttached<Control, ThemeSettings>(
                "ThemeCategory", 
                typeof(ThemeCategory), 
                defaultValue:ThemeSettings.foregroundColor
            );

        public static void SetThemeCategory(AvaloniaObject element, bool value) =>
            element.SetValue(Category, value);

        public static ThemeSettings GetThemeCategory(AvaloniaObject element)
        {
            return element.GetValue(Category);
        }
    }
}
