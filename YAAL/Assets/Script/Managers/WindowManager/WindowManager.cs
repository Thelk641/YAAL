using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
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
        public static MainWindow mainWindow;
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
            float mathedX = (float)(baseSize.X - (38 * App.Settings.Zoom));
            float mathedY = (float)(((baseSize.Y - (8 * App.Settings.Zoom)) / 2) - 1);
            return new Vector2(mathedX, mathedY);
        }

        public static void OpenWindow()
        {
            mainWindow = new MainWindow();
            mainWindow.IsVisible = true;
        }

        public static void ChangeHeight(Window toResize, int newHeight)
        {
            double trueHeight = newHeight * App.Settings.Zoom;
            toResize.MinHeight = trueHeight;
            toResize.MaxHeight = trueHeight;
            toResize.Height = trueHeight;
        }
    }
}
