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
using static YAAL.AutoColor;

namespace YAAL
{
    public static class AutoText
    {

        static Dictionary<int, Control> backgrounds = new Dictionary<int, Control>();
        static Dictionary<int, Color> previousValue = new Dictionary<int, Color>();

        public static readonly AttachedProperty<bool> AutoTextProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("AutoText", typeof(BackgroundSetter));

        public static void SetAutoText(AvaloniaObject element, bool value) =>
            element.SetValue(AutoTextProperty, value);

        public static bool GetAutoText(AvaloniaObject element) =>
            element.GetValue(AutoTextProperty);

        static AutoText()
        {
            AutoTextProperty.Changed.AddClassHandler<Control>(
                (ctrl, e) =>
                {
                    if ((bool)e.NewValue!)
                    {
                        EnableAutoText(ctrl);
                    }
                });
        }

        private static void EnableAutoText(Control ctrl)
        {
           

            var backgroundColor = FindNearestBackground(ctrl, out IBrush brush);

            if (ctrl is TextBlock text && text.Text != null && text.Text == "aplauncher")
            {
                //Debug.Write("For the text, the background is on : " + backgroundColor);
            }

            if (backgroundColor == null)
            {
                return;
            }

            EvaluateBackground(ctrl, brush);
            UpdateDictionary(ctrl, backgroundColor);
        }

        private static void EvaluateBackground(Control ctrl, IBrush backgroundColor)
        {
            if(backgroundColor == null)
            {
                return;
            }

            int hash = ctrl.GetHashCode();

            if(previousValue.ContainsKey(hash) && previousValue[hash] == (backgroundColor as ISolidColorBrush).Color)
            {
                var holder = backgrounds[hash];
                return;
            }


            bool darkMode = true;

            if (backgroundColor is ISolidColorBrush solid)
            {
                darkMode = !NeedsWhite(solid.Color);
            }

            if (ctrl is TextBlock textblock)
            {
                if (darkMode)
                {
                    textblock.Foreground = Brushes.Black;
                }
                else
                {
                    textblock.Foreground = Brushes.White;
                }

            }
            else if (ctrl is Button button)
            {
                if (darkMode)
                {
                    button.Foreground = Brushes.Black;
                }
                else
                {
                    button.Foreground = Brushes.White;
                }
            }

            previousValue[hash] = (backgroundColor as ISolidColorBrush).Color;
        }

        private static void UpdateDictionary(Control text, Control background)
        {
            int hash = text.GetHashCode();

            if (!backgrounds.ContainsKey(hash))
            {
                backgrounds.Add(hash, background);
                switch (background)
                {
                    case TemplatedControl templated:
                        var observer = templated.GetObservable(TemplatedControl.BackgroundProperty);
                        templated.GetObservable(TemplatedControl.BackgroundProperty).Subscribe(_ => EvaluateBackground(text, GetBrush(background)));
                        break;
                    case Panel panel:
                        panel.GetObservable(TemplatedControl.BackgroundProperty).Subscribe(_ => EvaluateBackground(text, GetBrush(background)));
                        break;
                    case Border border:
                        border.GetObservable(TemplatedControl.BackgroundProperty).Subscribe(_ => EvaluateBackground(text, GetBrush(background)));
                        break;
                }
            }
        }
    }
}
