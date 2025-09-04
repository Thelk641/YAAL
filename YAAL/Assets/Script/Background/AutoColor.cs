using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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

namespace YAAL
{
    public static class AutoColor
    {
        public static Control FindNearestBackground(Control ctrl, out IBrush brush)
        {

            Control current = ctrl;

            while(current != null)
            {
                if (ctrl is TextBlock text && text.Text != null && text.Text == "aplauncher")
                {
                    //Debug.WriteLine(current);
                }

                brush = GetBrush(current);
                if(brush != null && !(brush is SolidColorBrush color && color.Color == Colors.Transparent))
                {
                    return current;
                }

                Control parent = current.GetVisualParent() as Control;
                if(parent != null)
                {
                    current = parent;
                } else
                {
                    current = current.Parent as Control;
                }
            }

            brush = null;
            return null;
        }

        public static IBrush? GetBrush(Control ctrl)
        {
            switch (ctrl)
            {
                case TemplatedControl templated when templated.Background != null:
                    return templated.Background;
                case Panel panel when panel.Background != null:
                    return panel.Background;
                case Border border when border.Background != null:
                    return border.Background;
            }
            return null;
        }

        public static bool NeedsWhite(Control control)
        {
            if(control == null)
            {
                ErrorManager.ThrowError(
                "Color Manager - No control found",
                "Tried to find if a null needs white text. Please report this."
                );
                return true;
            }

            IBrush brush;
            Control holder = FindNearestBackground(control, out brush);
            if(brush == null)
            {
                ErrorManager.ThrowError(
                "Color Manager - No background found",
                "While trying to find the best text color, we couldn't find a nearest background for " + control.ToString() + ". Please report this."
                );
                return false;
            }


            if (brush is SolidColorBrush solid)
            {
                return NeedsWhite(solid.Color);
            } else if (brush is ImageBrush image)
            {
                return NeedsWhite(control, holder, image);
            }

            ErrorManager.ThrowError(
                "Color Manager - No compatible background found",
                "While trying to find the best text color, we found this " + control.ToString() + " with a type not allowed. Please report this."
                );
            return false;
        }

        public static bool NeedsWhite(Color color)
        {
            double R = color.R / 255.0;
            double G = color.G / 255.0;
            double B = color.B / 255.0;

            double luminance = 0.299 * R + 0.587 * G + 0.114 * B;

            return luminance < 0.5;
        }

        public static bool NeedsWhite(Control target, Control imageHolder, ImageBrush brush)
        {
            var rect = target.Bounds;
            var pointOnScreen = target.TranslatePoint(new Point(rect.Width / 2, rect.Height / 2), imageHolder);

            int pixelX = 0;
            int pixelY = 0;
            double scaleX;
            double scaleY;
            double uniformScale;
            double drawnW;
            double drawnH;
            double offsetX;
            double offsetY;
            double localX;
            double localY;

            Bitmap bitmap = (brush.Source as Bitmap);

            if (bitmap == null)
            {
                return false;
            }

            switch (brush.Stretch)
            {
                case Stretch.Fill:
                    // Math out the scale
                    scaleX = bitmap.PixelSize.Width / imageHolder.Bounds.Width;
                    scaleY = bitmap.PixelSize.Height / imageHolder.Bounds.Height;

                    // Math out the actual position on the image
                    pixelX = (int)(pointOnScreen.Value.X * scaleX);
                    pixelY = (int)(pointOnScreen.Value.Y * scaleY);

                    // Ensure floating point doesn't carry us out of the image
                    pixelX = Math.Clamp(pixelX, 0, bitmap.PixelSize.Width - 1);
                    pixelY = Math.Clamp(pixelY, 0, bitmap.PixelSize.Height - 1);
                    break;
                case Stretch.Uniform:
                    // Math out the uniform scale
                    scaleX = imageHolder.Bounds.Width / bitmap.PixelSize.Width;
                    scaleY = imageHolder.Bounds.Height / bitmap.PixelSize.Height;
                    uniformScale = Math.Min(scaleX, scaleY);

                    // Math the actual image size, as drawn
                    drawnW = bitmap.PixelSize.Width * uniformScale;
                    drawnH = bitmap.PixelSize.Height * uniformScale;

                    // Math the letter box (black bars)
                    offsetX = (imageHolder.Bounds.Width - drawnW) / 2;
                    offsetY = (imageHolder.Bounds.Height - drawnH) / 2;

                    // Adjust X and Y based on the letter box
                    localX = pointOnScreen.Value.X - offsetX;
                    localY = pointOnScreen.Value.Y - offsetY;

                    // Math out the actual position on the image
                    pixelX = (int)(localX / uniformScale);
                    pixelY = (int)(localY / uniformScale);

                    if(
                        pixelX < 0 || pixelX >= bitmap.PixelSize.Width ||
                        pixelY < 0 || pixelY >= bitmap.PixelSize.Height)
                    {
                        // We're on the letter box, so the background is black, so we do need text to be turned to white
                        if(imageHolder.Parent != null)
                        {
                            return NeedsWhite(imageHolder.Parent as Control);
                        } else
                        {
                            return true;
                        }
                    }
                    break;
                /*
                case Stretch.UniformToFill:
                case Stretch.None:
                    break;*/
            }

            pixelX = Math.Clamp(pixelX, 0, bitmap.PixelSize.Width - 1);
            pixelY = Math.Clamp(pixelY, 0, bitmap.PixelSize.Height - 1);

            return false;
        }

        public static Color HexToColor(string hex)
        {
            if (Color.TryParse(hex, out Color color))
            {
                return color;
            }
            else
            {
                return Color.FromRgb(0, 0, 0);
            }
        }

        public static string ColorToHex(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static Color Darken(Color color, double factor = 0.9)
        {
            return Color.FromArgb(
                color.A,
                (byte)(color.R * factor),
                (byte)(color.G * factor),
                (byte)(color.B * factor));
        }
    }
}
