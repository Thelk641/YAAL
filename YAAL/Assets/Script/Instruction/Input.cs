using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Newtonsoft.Json;
using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YAAL.Assets.Scripts;
using static YAAL.InputSettings;

namespace YAAL
{
    public class Input : Instruction<InputSettings>
    {
        public Input()
        {
            this.InstructionSetting[saveResult] = true.ToString();
            instructionType = "Input";
        }

        public override bool Execute()
        {
            string name = this.InstructionSetting[InputSettings.variableName];
            string value = "";

            if (customLauncher.GetTemporarySetting(name) is string previousValue)
            {
                // We've already ran through this for this slot before, and saved the result
                return true;
            }

            if (name == "")
            {
                ErrorManager.AddNewError(
                    "Input instruction - Empty variable name",
                    "You have an input instruction without a variable name. This is not allowed");
                return false;
            }

            if(WindowManager.OpenWindow(WindowType.InputWindow, null) is InputWindow window)
            {
                window.Setup(name);
                value = ShowAndWait(window);
                customLauncher.SetTemporarySetting(name, value);
            } else
            {
                ErrorManager.AddNewError(
                    "Input instruction - failed to open window",
                    "YAAL failed to open an InputWindow, please report.");
                return false;
            }

            if (this.InstructionSetting[InputSettings.saveResult] != true.ToString())
            {
                return true;
            }

            string async = "";
            string slot = "";

            if (customLauncher.GetTemporarySetting(AsyncSettings.asyncName.ToString()) is string asyncName)
            {
                async = asyncName;
            }
            else
            {
                ErrorManager.AddNewError(
                    "Input instruction - Couldn't find async name",
                    "Trying to get asyncName out of the launcher's temporary settings failed, please report this.");
                return false;
            }

            if (customLauncher.GetTemporarySetting(SlotSettings.slotName.ToString()) is string slotName)
            {
                slot = slotName;
            }
            else
            {
                ErrorManager.AddNewError(
                    "Input instruction - Couldn't find slot name",
                    "Trying to get slotName out of the launcher's temporary settings failed, please report this.");
                return false;
            }

            IOManager.SetSlotSetting(async, slot, name, value);

            return true;
        }

        public string ShowAndWait(InputWindow window)
        {
            string output = "";

            var frame = new DispatcherFrame();
            window.Closed += (_, _) =>
            {
                output = window.GetVariableContent();
            };

            window.Show();

            Dispatcher.UIThread.PushFrame(frame);

            return output;
        }

        public override void ParseProcess(object? sender, EventArgs e)
        {
            return;
        }    
    }
}
