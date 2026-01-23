using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using YAAL.Assets.Scripts;
using static YAAL.DisplaySettings;

namespace YAAL;

public partial class Command_Display : Command
{
    public CommandSetting<DisplaySettings> CommandSettings => (CommandSetting<DisplaySettings>)settings;

    private Dictionary<DisplaySettings, string> defaultValues = new Dictionary<DisplaySettings, string>() {
        {displayList,"" }
    };

    List<DisplayInfo> displays = new List<DisplayInfo>();

    public Command_Display()
    {
        InitializeComponent();
        settings = new CommandSetting<DisplaySettings>();
        CommandSettings.SetDefaultSetting(defaultValues);
        CommandSettings.SetCommandType("Display");
        
        SetDebouncedEvents();
        Add.Click += (_, _) => { AddDisplay("", ""); };
        AutoTheme.SetTheme(ContainerBorder, ThemeSettings.off);
    }

    public override void LoadInstruction(Interface_CommandSetting newInstruction)
    {
        base.LoadInstruction(newInstruction);

        string displayList = CommandSettings.GetSetting(DisplaySettings.displayList);
        var list = JsonConvert.DeserializeObject<Dictionary<string, string>>(displayList);

        if(list is Dictionary<string, string> parsedList)
        {
            foreach (var item in parsedList)
            {
                AddDisplay(item.Key, item.Value);
            }
        }
    }

    public void RequestRemoval(DisplayInfo info)
    {
        displays.Remove(info);
        InfoContainer.Children.Remove(info);
    }

    public void AddDisplay(string tag, string value)
    {
        DisplayInfo info = new DisplayInfo(this, tag, value);
        InfoContainer.Children.Add(info);
        displays.Add(info);
    }

    public override Interface_CommandSetting GetInstruction()
    {
        Dictionary<string, string> newInfos = new Dictionary<string, string>();
        foreach (var item in displays) 
        {
            string tag = item.GetTag();
            if (newInfos.ContainsKey(tag))
            {
                tag += " (copy)";
            }
            newInfos[tag] = item.GetValue();
        }

        string newSetting = JsonConvert.SerializeObject(newInfos);
        CommandSettings.SetSetting(DisplaySettings.displayList, newSetting);

        return CommandSettings;
    }

    protected override void TurnEventsOn()
    {
        return;
    }
}