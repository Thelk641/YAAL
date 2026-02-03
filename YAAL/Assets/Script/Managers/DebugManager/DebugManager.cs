using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YAAL
{
    public class DebugManager : TraceListener
    {
        ConcurrentQueue<string> messages = new ConcurrentQueue<string>();

        public override void Write(string? message)
        {
            if (message is string note)
            {
                messages.Enqueue(note.Trim());
            }
        }

        public override void WriteLine(string? message)
        {
            if(message is string note)
            {
                messages.Enqueue(note.Trim());
            }
        }

        public void Save()
        {
            string output = "";
            foreach (var item in messages)
            {
                if(item is string note && note.Length > 0)
                {
                    output += item + "\n";
                }
            }
            LogsIOManager.SaveCacheLogs(output);
        }
    }
}
