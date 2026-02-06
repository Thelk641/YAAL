using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using YAAL.Assets.Scripts;
using static YAAL.InputSettings;

namespace YAAL;

public partial class Command_Input : Command<InputSettings>
{
    public CommandSetting<InputSettings> CommandSettings => (CommandSetting<InputSettings>)settings;

    private Dictionary<InputSettings, string> defaultValues = new Dictionary<InputSettings, string>() {
        {variableName,"" },
        {saveResult, "" }
    };

    public Command_Input()
    {
        InitializeComponent();
        CommandSettings.SetDefaultSetting(defaultValues);
        CommandSettings.SetCommandType("Input");
        SetDebouncedEvents();
        TurnEventsOn();
    }

    

    public override void LoadInstruction(Interface_CommandSetting newInstruction)
    {
        base.LoadInstruction(newInstruction);
        TurnEventsOff();
        OnlyOnce.IsChecked = CommandSettings.GetSetting(saveResult) == true.ToString();
        VariableName.Text = CommandSettings.GetSetting(variableName);
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
        CommandSettings.SetSetting(saveResult, OnlyOnce.IsChecked.ToString() ?? true.ToString());
    }

}