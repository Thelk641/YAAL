using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YAAL.Assets.Scripts;
using static YAAL.WaitSettings;

namespace YAAL
{
    public class Wait : Instruction<WaitSettings>
    {
        string processKey = "";

        public Wait()
        {
            instructionType = "Wait";
        }

        public override bool Execute()
        {
            switch (this.InstructionSetting[WaitSettings.mode])
            {
                case "Timer":
                    if (float.TryParse(this.InstructionSetting[WaitSettings.timer], out float time))
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(time));
                        return true;
                    } else
                    {
                        ErrorManager.AddNewError(
                        "Wait instruction - invalid timer",
                        "A wait instruction was given a timer of '" + this.InstructionSetting[WaitSettings.timer] + "', which isn't a valid float."
                        );
                    }
                    break;
                case "Process":
                    processKey = this.InstructionSetting[WaitSettings.processName];
                    if(processKey != "")
                    {
                        customLauncher.AttachToClosing(this, processKey);
                        return true;
                    }
                    ErrorManager.AddNewError(
                        "Wait instruction - invalid process key",
                        "A wait instruction was given an empty process name to look for. This is not allowed."
                        );
                    break;
            }

            return false;
        }

        public override void ParseProcess(object? sender, EventArgs e)
        {
            customLauncher.DetachToClosing(this, processKey);
            customLauncher.RemoveWait(this);
            return;
        }

        
    }
}
