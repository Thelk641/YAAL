using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Presenters;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static Cache_Windows windowsData;

        static WindowManager()
        {
            windowsData = IOManager.GetWindowSettings();
        }

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

        public static Window OpenWindow(WindowType windowType, Window? source)
        {
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
                case WindowType.DisplayWindow:
                    window = new DisplayWindow();
                    break;
                default:
                    window = new MainWindow();
                    break;
            }

            
            window.Closing += (_, _) =>
            {
                if(source != null)
                {
                    source.Topmost = true;
                    source.Topmost = false;
                }
                windowsData.positions[windowType] = window.Position;
                Vector2 size = new Vector2((float)window.Width, (float)window.Height);
                windowsData.size[windowType] = size;
                IOManager.SetWindowSettings(windowsData);
            };

            if (windowsData.positions.ContainsKey(windowType))
            {
                window.Position = windowsData.positions[windowType];
                window.Width = windowsData.size[windowType].X;
                window.Height = windowsData.size[windowType].Y;
            } else
            {
                if(source != null)
                {
                    Vector2 sourceCenter = new Vector2(
                        source.Position.X + (int)(source.ClientSize.Width / 2),
                        source.Position.Y + (int)(source.ClientSize.Height / 2)
                        );

                    Vector2 windowCenter = new Vector2(
                        (int)(window.Width / 2),
                        (int)(window.Height / 2)
                        );

                    Vector2 trueCenter = new Vector2(
                        sourceCenter.X - windowCenter.X,
                        sourceCenter.Y - windowCenter.Y
                        );

                    window.Position = new PixelPoint((int)trueCenter.X, (int)trueCenter.Y);
                }
            }

            if (mainWindow != null)
            {
                window.Show();
            }
            else
            {
                Dispatcher.UIThread.Post(() =>
                {
                    window.Show();
                }, DispatcherPriority.Background);
            }

            return window;
        }

        public static VersionManager OpenVersionWindow(string gameName)
        {
            VersionManager window = new VersionManager(gameName);

            if (windowsData.positions.ContainsKey(WindowType.CLMaker))
            {
                float X = windowsData.positions[WindowType.CLMaker].X + (windowsData.size[WindowType.CLMaker].X / 2);
                float Y = windowsData.positions[WindowType.CLMaker].Y + (windowsData.size[WindowType.CLMaker].Y / 2);
                PixelPoint truePosition = new PixelPoint((int)X, (int)Y);
                window.Position = truePosition;
            }

            window.IsVisible = true;
            return window;
        }

        

        public static VersionManager OpenVersionWindow(string gameName, string versionName)
        {
            VersionManager window = new VersionManager(gameName, versionName);

            if (windowsData.positions.ContainsKey(WindowType.CLMaker))
            {
                float X = windowsData.positions[WindowType.CLMaker].X + (windowsData.size[WindowType.CLMaker].X / 2);
                float Y = windowsData.positions[WindowType.CLMaker].Y + (windowsData.size[WindowType.CLMaker].Y / 2);
                PixelPoint truePosition = new PixelPoint((int)X, (int)Y);
                window.Position = truePosition;
            }

            window.IsVisible = true;
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
                if (windowsData.positions.ContainsKey(WindowType.MainWindow))
                {
                    mainWindow.Position = windowsData.positions[WindowType.MainWindow];
                    mainWindow.Width = windowsData.size[WindowType.MainWindow].X;
                    mainWindow.Height = windowsData.size[WindowType.MainWindow].Y;
                }

                mainWindow.Closing += (_, _) =>
                {
                    windowsData.positions[WindowType.MainWindow] = mainWindow.Position;
                    Vector2 size = new Vector2((float)mainWindow.Width, (float)mainWindow.Height);
                    windowsData.size[WindowType.MainWindow] = size;
                    IOManager.SetWindowSettings(windowsData);
                };

                DoneStarting?.Invoke();
            }

            return mainWindow;
        }
    }
}
