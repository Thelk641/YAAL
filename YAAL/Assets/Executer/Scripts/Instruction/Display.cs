using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YAAL.DisplaySettings;

namespace YAAL
{
    public class Display : Instruction<DisplaySettings>
    {
        public Display()
        {
            instructionType = "Display";
        }

        public override bool Execute()
        {
            var list = JsonConvert.DeserializeObject<Dictionary<string, string>>(this.InstructionSetting[displayList]);

            Dictionary<string, string> toDisplay = new Dictionary<string, string>();
            foreach (var item in list)
            {
                string key = executer.Parser.ParseTextWithSettings(item.Key);
                string value = executer.Parser.ParseTextWithSettings(item.Value);
                toDisplay[key] = value;
            }


            if (WindowManager.OpenWindow(WindowType.DisplayWindow, null) is DisplayWindow window)
            {
                foreach (var item in toDisplay)
                {
                    window.AddInfo(item.Key, item.Value);
                }
                executer.AddWait(this);

                window.Closing += (_, _) => { executer.RemoveWait(this); };
                window.IsVisible = true;

                return true;
            }


            return false;
        }

        public override void ParseProcess(object? sender, EventArgs e)
        {
            return;
        }
    }
}
