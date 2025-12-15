using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using YAAL.Assets.Scripts;
using static YAAL.InputSettings;

namespace YAAL;

public partial class Command_Input : Command
{
    
    public Command_Input()
    {
        InitializeComponent();
        SetDebouncedEvents();
        linkedInstruction = new Input();
        TurnEventsOn();
    }

    

    public override void LoadInstruction(Interface_Instruction newInstruction)
    {
        TurnEventsOff();
        linkedInstruction = newInstruction;
        OnlyOnce.IsChecked = linkedInstruction.GetSetting(InputSettings.saveResult.ToString()) == true.ToString();
        VariableName.Text = linkedInstruction.GetSetting(InputSettings.variableName.ToString());
        TurnEventsBackOn();

        _OnlyOnceChanged(null, null);
    }

    public override void SetDebouncedEvents()
    {
        base.SetDebouncedEvents();
        debouncedSettings[VariableName] = variableName.ToString();
    }

    protected override void TurnEventsOn()
    {
        OnlyOnce.IsCheckedChanged += _OnlyOnceChanged;
        VariableName.TextChanged += _TextChanged;
    }

    private void TurnEventsOff()
    {
        OnlyOnce.IsCheckedChanged -= _OnlyOnceChanged;
        VariableName.TextChanged -= _TextChanged;
    }

    private void _OnlyOnceChanged(object? sender, RoutedEventArgs e)
    {
        linkedInstruction.SetSetting(InputSettings.saveResult.ToString(), OnlyOnce.IsChecked.ToString() ?? true.ToString());
    }

}