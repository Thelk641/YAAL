using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using YAAL.Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace YAAL
{
    public abstract class Command<T> : Command where T : struct, Enum
    {
        public CommandSetting<T> settings { get; set; } = new CommandSetting<T>();

        public override void Trigger(TextBox box)
        {
            if (this.debouncedEvents.ContainsKey(box.Name))
            {
                this.debouncedEvents[box.Name]();
            }
            else
            {
                settings.SetSetting(debouncedSettings[box], box.Text);
            }
        }

        public override Interface_CommandSetting GetSettings()
        {
            return settings;
        }

        public override void LoadInstruction(Interface_CommandSetting newSettings)
        {
            if(newSettings is CommandSetting<T> validSettings)
            {
                settings = validSettings;
            } else
            {
                ErrorManager.ThrowError(
                    "Command generic - Invalid setting type", 
                    "Settings passed were of type " + newSettings.GetType() + " instead of " + typeof(CommandSetting<T>) + ". Please report this.");
            }
            
        }
    }
}
