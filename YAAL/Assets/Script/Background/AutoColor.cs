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
                if(brush != null && !(brush is ISolidColorBrush color && color.Color == Colors.Transparent))
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
            } else if (brush is ImmutableSolidColorBrush immutableSolid){
                return NeedsWhite(immutableSolid.Color);
            }

            ErrorManager.ThrowError(
                    "Color Manager - No compatible background found",
                    "While trying to find the best text color, we found this " + control.ToString() + " with a type not allowed. Please report this."
                    );
            return false;
        }

        public static bool NeedsWhite(Color color)
        {
            return GetLuminance(color) < 0.5;
        }

        public static bool NeedsWhite(Control target, Control imageHolder, ImageBrush brush)
        {
            Vector2 location = new Vector2(0, 0);
            var rect = target.Bounds;
            var pointOnScreen = target.TranslatePoint(new Point(rect.Width / 2, rect.Height / 2), imageHolder);

            if(pointOnScreen == null)
            {
                ErrorManager.ThrowError(
                    "AutoColor - pointOnScreen is null",
                    "Found a way to call NeedsWhite too early. Please report."
                    );
            }

            Bitmap bitmap = (brush.Source as Bitmap);

            if (bitmap == null)
            {
                return false;
            }

            switch (brush.Stretch)
            {
                case Stretch.Fill:
                    location = StretchFill(target, imageHolder, brush);
                    break;
                case Stretch.Uniform:
                case Stretch.UniformToFill:
                    var tempUniform = StretchUniform(target, imageHolder, brush);
                    if(tempUniform is Vector2 validUniform)
                    {
                        location = validUniform;
                    } else
                    {
                        if (imageHolder.Parent != null)
                        {
                            return NeedsWhite(imageHolder.Parent as Control);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    break;
                case Stretch.None:
                    var tempNone = StretchNone(target, imageHolder, brush);
                    if (tempNone is Vector2 validNone)
                    {
                        location = validNone;
                    }
                    else
                    {
                        if (imageHolder.Parent != null)
                        {
                            return NeedsWhite(imageHolder.Parent as Control);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    break;
            }


            int start = Math.Max(0, (int)location.X - 2);
            int end = Math.Min(bitmap.PixelSize.Width - 1, (int)location.X + 2);

            double averageLuminance = 0;

            for (int i = start; i < end; i++)
            {
                Color color = GetPixelColor(bitmap, i, (int)location.Y);
                averageLuminance += GetLuminance(color);
            }

            averageLuminance = averageLuminance / 5;

            return averageLuminance < 0.5;
        }

        public static double GetLuminance(Color color)
        {
            double R = color.R / 255.0;
            double G = color.G / 255.0;
            double B = color.B / 255.0;

            double luminance = 0.299 * R + 0.587 * G + 0.114 * B;

            return luminance;
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

        public static Color Darken(Color color)
        {
            double factor;
            if(color.A > 0.5 && GetLuminance(color) < 0.5)
            {
                factor = 1.1;
            } else
            {
                factor = 0.9;
            }
            return Color.FromArgb(
                color.A,
                (byte)(color.R * factor),
                (byte)(color.G * factor),
                (byte)(color.B * factor));
        }

        public static Vector2 StretchFill(Control target, Control imageHolder, ImageBrush brush)
        {
            Vector2 output = new Vector2();

            var rect = target.Bounds;
            var pointOnScreen = target.TranslatePoint(new Point(rect.Width / 2, rect.Height / 2), imageHolder);
            double scaleX;
            double scaleY;

            Bitmap bitmap = (brush.Source as Bitmap);

            // Math out the scale
            scaleX = bitmap.PixelSize.Width / imageHolder.Bounds.Width;
            scaleY = bitmap.PixelSize.Height / imageHolder.Bounds.Height;

            // Math out the actual position on the image
            output.X = Math.Clamp((int)(pointOnScreen.Value.X * scaleX), 0, bitmap.PixelSize.Width - 1);
            output.Y = Math.Clamp((int)(pointOnScreen.Value.Y * scaleY), 0, bitmap.PixelSize.Height - 1);


            return output;
        }

        public static Vector2? StretchUniform(Control target, Control imageHolder, ImageBrush brush)
        {
            Vector2 output = new Vector2();
            var rect = target.Bounds;
            var pointOnScreen = target.TranslatePoint(new Point(rect.Width / 2, rect.Height / 2), imageHolder);

            double scaleX;
            double scaleY;
            double uniformScale;
            double drawnW;
            double drawnH;
            double offsetX = 0;
            double offsetY = 0;
            double localX;
            double localY;

            Vector2 location = new Vector2(0, 0);

            Bitmap bitmap = (brush.Source as Bitmap);

            // Math out the uniform scale
            scaleX = imageHolder.Bounds.Width / bitmap.PixelSize.Width;
            scaleY = imageHolder.Bounds.Height / bitmap.PixelSize.Height;
            if (brush.Stretch == Stretch.Uniform)
            {
                uniformScale = Math.Min(scaleX, scaleY);
            }
            else
            {
                uniformScale = Math.Max(scaleX, scaleY);
            }

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
            output.X = (int)(localX / uniformScale);
            output.Y = (int)(localY / uniformScale);

            if (
                output.X < 0 || output.X >= bitmap.PixelSize.Width ||
                output.Y < 0 || output.Y >= bitmap.PixelSize.Height)
            {
                return DealWithTile(output, brush, bitmap);
            }

            return output;
        }

        public static Vector2? StretchNone(Control target, Control imageHolder, ImageBrush brush)
        {
            Vector2 output = new Vector2();
            var rect = target.Bounds;
            var pointOnScreen = target.TranslatePoint(new Point(rect.Width / 2, rect.Height / 2), imageHolder);
            double offsetX = 0;
            double offsetY = 0;
            double localX;
            double localY;

            Vector2 location = new Vector2(0, 0);

            Bitmap bitmap = (brush.Source as Bitmap);


            switch (brush.AlignmentX)
            {
                case AlignmentX.Left:
                    offsetX = 0;
                    break;
                case AlignmentX.Center:
                    offsetX = (imageHolder.Bounds.Width - bitmap.PixelSize.Width) / 2;
                    break;
                case AlignmentX.Right:
                    offsetX = imageHolder.Bounds.Width - bitmap.PixelSize.Width;
                    break;
            }

            switch (brush.AlignmentY)
            {
                case AlignmentY.Top:
                    offsetY = 0;
                    break;
                case AlignmentY.Center:
                    offsetY = (imageHolder.Bounds.Height - bitmap.PixelSize.Height) / 2;
                    break;
                case AlignmentY.Bottom:
                    offsetY = imageHolder.Bounds.Height - bitmap.PixelSize.Height;
                    break;
            }

            output.X = (int)(pointOnScreen.Value.X - offsetX);
            output.Y = (int)(pointOnScreen.Value.Y - offsetY);

            if (
                output.X < 0 || output.X >= bitmap.PixelSize.Width ||
                output.Y < 0 || output.Y >= bitmap.PixelSize.Height)
            {
                return DealWithTile(output, brush, bitmap);
            }

            return output;
        }

        public static Vector2? DealWithTile(Vector2 position, ImageBrush brush, Bitmap bitmap)
        {
            Vector2 output = position;

            if(brush.TileMode == TileMode.None)
            {
                return null;
            }

            if(position.X < 0)
            {
                int offset = bitmap.PixelSize.Width - (int)((position.X * -1) % bitmap.PixelSize.Width);
                output.X = offset;
            }

            if (position.Y < 0)
            {
                int offset = bitmap.PixelSize.Height - (int)((position.Y * -1) % bitmap.PixelSize.Height);
                output.Y = offset;
            }

            if(brush.TileMode == TileMode.FlipX || brush.TileMode == TileMode.FlipXY)
            {
                int tileX = (int)(output.X / bitmap.PixelSize.Width);
                if(tileX % 2 == 1)
                {
                    output.X = bitmap.PixelSize.Width - output.X;
                }
            }

            if (brush.TileMode == TileMode.FlipY || brush.TileMode == TileMode.FlipXY)
            {
                int tileY = (int)(output.Y / bitmap.PixelSize.Height);
                if (tileY % 2 == 1)
                {
                    output.Y = bitmap.PixelSize.Height - output.Y;
                }
            }

            return output;
        }

        public static Color GetPixelColor(Bitmap bitmap, int x, int y)
        {
            var rect = new PixelRect(x, y, 1, 1);
            int bytesPerPixel = 4; // 4 bytes per pixel (BGRA)
            var stride = bytesPerPixel * rect.Width; 

            var buffer = new byte[bytesPerPixel * rect.Width];
            var bufferPtr = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                bitmap.CopyPixels(rect, bufferPtr.AddrOfPinnedObject(), buffer.Length, stride);

                var b = buffer[0];
                var g = buffer[1];
                var r = buffer[2];
                var a = buffer[3];

                Color output = new Color(a, r, g, b);
                return output;
            }
            catch (Exception e)
            {
                ErrorManager.ThrowError(
                    "AutoColor - Trying to read bitmap threw an exception",
                    "While trying to read a bitmap, we got the following exception : " + e.Message
                    );
                return new Color();
            }
            finally
            {
                bufferPtr.Free();
            }
        }
    }
}
