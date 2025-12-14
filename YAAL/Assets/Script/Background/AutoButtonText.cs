using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Metsys.Bson;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using static YAAL.AutoColor;

namespace YAAL
{
    public static class AutoButtonText
    {
        public static readonly AttachedProperty<IBrush> AutoButtonTextProperty =
        AvaloniaProperty.RegisterAttached<Control, IBrush>("AutoButtonText", typeof(AutoButtonText), defaultValue:Brushes.Black);

        public static void SetAutoButtonText(AvaloniaObject element, IBrush value) =>
            element.SetValue(AutoButtonTextProperty, value);

        public static IBrush GetAutoButtonText(AvaloniaObject element) =>
            element.GetValue(AutoButtonTextProperty);
    }
}
