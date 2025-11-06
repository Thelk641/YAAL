using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.VisualTree;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YAAL
{
    public static class WindowManager 
    {
        static bool doneStarting = true;
        public static MainWindow? mainWindow;
        public static Action? DoneStarting;
        public static Vector2 GetWindowSize()
        {
            return new Vector2(700, 400);
        }

        public static Vector2 GetSlotSize()
        {
            float mathedX = (float)(GetWindowSize().X - (70 * App.Settings.Zoom));
            float mathedY = (float)(112 * App.Settings.Zoom);
            Vector2 baseSize = new Vector2(mathedX, 52);
            return new Vector2(mathedX, mathedY);
        }

        public static Vector2 GetSlotForegroundSize()
        {
            Vector2 baseSize = GetSlotSize();
            float mathedX = (float)(baseSize.X - (38 * App.Settings.Zoom)); //38 is the spacing for the "update" button
            float mathedY = (float)(baseSize.Y / 2);
            return new Vector2(mathedX, mathedY);
        }

        public static void OpenWindow()
        {
            mainWindow = new MainWindow();
            mainWindow.IsVisible = true;
        }

        public static Window OpenWindow(WindowType windowType, Window source)
        {
            // This will need adjusting once everything is a scalable window
            Window window;
            switch (windowType)
            {
                case WindowType.CLMaker:
                    window = CLMakerWindow.GetCLMakerWindow();
                    break;
                case WindowType.CustomThemeMaker:
                    window = new CustomThemeMaker();
                    break;
                case WindowType.NewLauncher:
                    window = new NewLauncher();
                    break;
                case WindowType.TestWindow:
                    window = new TestWindow();
                    break;
                case WindowType.UpdateWindow:
                    window = new UpdateWindow();
                    break;
                default:
                    window = new MainWindow();
                    break;
            }

            window.IsVisible = true;
            window.Closing += (_, _) =>
            {
                source.Topmost = true;
                source.Topmost = false;
            };
            return window;
        }

        public static void ChangeHeight(Window toResize, int newHeight)
        {
            double trueHeight = newHeight * App.Settings.Zoom;
            toResize.MinHeight = trueHeight;
            toResize.MaxHeight = trueHeight;
            toResize.Height = trueHeight;
        }

        public static void UpdateComboBox(ComboBox comboBox)
        {
            var template = comboBox.ItemTemplate;
            comboBox.ItemTemplate = null;
            comboBox.ItemTemplate = template;
        }

        public static MainWindow? GetMainWindow()
        {
            if (!doneStarting)
            {
                return null;
            }

            if (mainWindow == null)
            {
                doneStarting = false;
                mainWindow = new MainWindow();
                mainWindow.Show();
                mainWindow.IsVisible = true;
                doneStarting = true;
                DoneStarting?.Invoke();
            }

            return mainWindow;
        }
    }
}
