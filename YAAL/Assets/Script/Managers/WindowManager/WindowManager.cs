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
            float mathedX = GetWindowSize().X - 70;
            Vector2 baseSize = new Vector2(mathedX, 52);
            return new Vector2((float)(baseSize.X * App.Settings.Zoom), (float)(baseSize.Y * App.Settings.Zoom));
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
