using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace YAAL.Assets.Scripts
{
    public static class Debouncer
    {
        public static DispatcherTimer timer;

        private static Dictionary<Action, float> countdown = new Dictionary<Action, float>();
        private static Dictionary<TextBox, float> boxCountdown = new Dictionary<TextBox, float>();
        private static Dictionary<TextBox, Command> boxOrigin = new Dictionary<TextBox, Command>();

        public static event Action DebounceCompleted;
        public static bool isDone = true;

        static Debouncer() {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            timer.Tick += EvaluateCountdown;
            timer.Start();
        }

        private static void EvaluateCountdown(object? sender, EventArgs e)
        {
            foreach (var item in countdown)
            {
                countdown[item.Key] -= 0.1f;
                if (item.Value <= 0)
                {
                    item.Key();
                    countdown.Remove(item.Key);
                    if (countdown.Count == 0)
                    {
                        isDone = true;
                        DebounceCompleted?.Invoke();
                    }
                }
            }

            foreach (var item in boxCountdown)
            {
                boxCountdown[item.Key] -= 0.1f;
                if (item.Value <= 0)
                {
                    boxOrigin[item.Key].Trigger(item.Key);
                    boxCountdown.Remove(item.Key);
                    boxOrigin.Remove(item.Key);
                    if(boxCountdown.Count == 0)
                    {
                        isDone = true;
                        DebounceCompleted?.Invoke();
                    }
                }
            }
        }

        public static void Debounce(Action debouncedFunction, float time)
        {
            if (countdown.ContainsKey(debouncedFunction) && countdown[debouncedFunction] > time)
            {
                return;
            }
            countdown[debouncedFunction] = time;
            isDone = false;
        }

        public static void Debounce(TextBox box, Command origin, float time)
        {
            if(boxCountdown.ContainsKey(box) && boxCountdown[box] > time)
            {
                return;
            }
            boxCountdown[box] = time;
            boxOrigin[box] = origin;
            isDone = false;
        }
    }
}
