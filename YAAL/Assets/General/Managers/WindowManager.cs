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
            windowsData = GetWindowSettings();
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

        public static Window OpenWindow(WindowType windowType, Window? source, bool autoClose = true)
        {
            Window window;
            bool useSavedSize = true;
            switch (windowType)
            {
                case WindowType.CLMaker:
                    window = CLM.GetWindow();
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
                    useSavedSize = false;
                    break;
                case WindowType.ConfirmationWindow:
                    window = new ConfirmationWindow();
                    break;
                case WindowType.InputWindow:
                    window = new InputWindow();
                    break;
                case WindowType.MainWindow:
                    window = new MainWindow();
                    break;
                default:
                    ErrorManager.ThrowError(
                        "WindowManager - Wrong window type", 
                        "Tried to open a window of type : " + windowType + ". That shouldn't happen, please report.");
                    return null;
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
                windowsData.maximized[windowType] = window.WindowState == WindowState.Maximized;
                SetWindowSettings(windowsData);
            };

            if(source != null && autoClose)
            {
                source.Closing += (_, _) => window.Close();
            }

            if (useSavedSize && windowsData.positions.ContainsKey(windowType))
            {
                window.Position = windowsData.positions[windowType];
                window.Width = windowsData.size[windowType].X;
                window.Height = windowsData.size[windowType].Y;
                if (windowsData.maximized[windowType])
                {
                    window.WindowState = WindowState.Maximized;
                }
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

        public static VersionWindow OpenVersionWindow(string gameName)
        {
            VersionWindow window = new VersionWindow(gameName);

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

        

        public static VersionWindow OpenVersionWindow(string gameName, string versionName)
        {
            VersionWindow window = new VersionWindow(gameName, versionName);

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

                if(OpenWindow(WindowType.MainWindow, null) is MainWindow window)
                {
                    mainWindow = window;
                    doneStarting = true;
                    DoneStarting?.Invoke();
                }
            }

            return mainWindow;
        }

        public static Cache_Windows GetWindowSettings()
        {
            return CacheManager.LoadCache<Cache_Windows>(SettingsManager.GetSaveLocation(FileSettings.windows));
        }

        public static void SetWindowSettings(Cache_Windows newSettings)
        {
            CacheManager.SaveCache<Cache_Windows>(SettingsManager.GetSaveLocation(FileSettings.windows), newSettings);
        }
    }
}
