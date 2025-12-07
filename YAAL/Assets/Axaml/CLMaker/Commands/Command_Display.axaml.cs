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
    List<DisplayInfo> displays = new List<DisplayInfo>();

    public Command_Display()
    {
        InitializeComponent();
        AutoTheme.SetTheme(ContainerBorder, ThemeSettings.off);
        SetDebouncedEvents();
        linkedInstruction = new Display();

        Add.Click += (_, _) => { AddDisplay("", ""); };
    }

    public override void LoadInstruction(Interface_Instruction newInstruction)
    {
        linkedInstruction = newInstruction;

        string displayList = this.linkedInstruction.GetSetting(DisplaySettings.displayList.ToString());
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

    public override Interface_Instruction GetInstruction()
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
        linkedInstruction.SetSetting(DisplaySettings.displayList.ToString(), newSetting);

        return linkedInstruction;
    }

    protected override void TurnEventsOn()
    {
        return;
    }
}