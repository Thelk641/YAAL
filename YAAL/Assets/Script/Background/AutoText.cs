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
    public static class AutoText
    {
        private static Dictionary<ComboBox, EventHandler?> boxDictionary = new Dictionary<ComboBox, EventHandler?>();
        static Dictionary<int, Control> backgrounds = new Dictionary<int, Control>();
        static Dictionary<int, IBrush> previousValue = new Dictionary<int, IBrush>();

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

            if (ctrl is TextBlock text && text.Text != null && text.Text.Contains("Masaru"))
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

            if(previousValue.ContainsKey(hash) && previousValue[hash] == backgroundColor)
            {
                return;
            }


            bool darkMode = !NeedsWhite(ctrl);

            /*if (backgroundColor is ISolidColorBrush solid)
            {
                darkMode = !NeedsWhite(solid.Color);
            }*/

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
            } else if (ctrl is ComboBox comboBox)
            {
                AutoComboBox(comboBox);
            }

            previousValue[hash] = backgroundColor;
        }



        private static void UpdateDictionary(Control text, Control background)
        {
            int hash = text.GetHashCode();

            if(backgrounds.ContainsKey(hash) && backgrounds[hash] != background){
                backgrounds.Remove(hash);
            }

            if (!backgrounds.ContainsKey(hash))
            {
                backgrounds.Add(hash, background);
                switch (background)
                {
                    case TemplatedControl templated:
                        var observer = templated.GetObservable(TemplatedControl.BackgroundProperty);
                        templated.GetObservable(TemplatedControl.BackgroundProperty).Subscribe(_ => EnableAutoText(text));
                        break;
                    case Panel panel:
                        panel.GetObservable(TemplatedControl.BackgroundProperty).Subscribe(_ => EnableAutoText(text));
                        break;
                    case Border border:
                        border.GetObservable(TemplatedControl.BackgroundProperty).Subscribe(_ => EnableAutoText(text));
                        break;
                }
            }
        }

        private static void AutoComboBox(ComboBox comboBox)
        {
            /*if (boxDictionary.TryGetValue(comboBox, out EventHandler? oldHandler))
            {
                comboBox.DropDownOpened -= oldHandler;
            }*/

            EventHandler? handler = null;

            handler = (_, _) =>
            {
                foreach (var c in comboBox.GetRealizedContainers().OfType<Control>())
                {
                    if (c is ComboBoxItem item)
                    {
                        bool darkMode = false;

                        if(comboBox.GetVisualDescendants().OfType<ContentPresenter>().FirstOrDefault() is ContentPresenter presenter)
                        {
                            darkMode = !AutoColor.NeedsWhite(presenter);
                        }

                        if (item.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault() is TextBlock text)
                        {
                            if (darkMode)
                            {
                                text.Foreground = Brushes.Black;
                            }
                            else
                            {
                                text.Foreground = Brushes.White;
                            }
                        }
                    }
                }
                comboBox.DropDownOpened -= handler;
            };

            
            comboBox.DropDownOpened += handler;
        }
    }
}
