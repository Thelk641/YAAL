using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using YAAL.Assets.Scripts;
using static YAAL.WaitSettings;

namespace YAAL;

public partial class Command_Wait : Command
{
    
    public Command_Wait()
    {
        InitializeComponent();
        SetDebouncedEvents();
        linkedInstruction = new Wait();

        ModeSelector.ItemsSource = new List<string>() { "Pause launcher until timer is finished", "Keep launcher open until process closes" };

        ModeSelector.SelectedIndex = 0;
        WindowManager.UpdateComboBox(ModeSelector);
    }

    

    public override void LoadInstruction(Interface_Instruction newInstruction)
    {
        linkedInstruction = newInstruction;

        TimerInput.Text = linkedInstruction.GetSetting(timer.ToString());
        ProcessInput.Text = linkedInstruction.GetSetting(processName.ToString());

        if (linkedInstruction.GetSetting(mode.ToString()) == "Process")
        {
            ModeSelector.SelectedItem = "Keep launcher open until process closes";
        }
        TurnEventsBackOn();
    }

    public override void SetDebouncedEvents()
    {
        base.SetDebouncedEvents();
        debouncedSettings[TimerInput] = timer.ToString();
        debouncedSettings[ProcessInput] = processName.ToString();
    }

    protected override void TurnEventsOn()
    {
        ModeSelector.SelectionChanged += _SwitchMode;
        TimerInput.TextChanged += _TextChanged;
        ProcessInput.TextChanged += _TextChanged;
    }

    private void TurnEventsOff()
    {
        ModeSelector.SelectionChanged -= _SwitchMode;
        TimerInput.TextChanged -= _TextChanged;
        ProcessInput.TextChanged -= _TextChanged;
    }


    private void _SwitchMode(object? sender, SelectionChangedEventArgs e)
    {
        if (ModeSelector.SelectedItem is string selection && selection == "Pause launcher until timer is finished")
        {
            linkedInstruction.SetSetting(mode.ToString(), "Timer");
        }
        else
        {
            linkedInstruction.SetSetting(mode.ToString(), "Process");
        }

        TimerInput.IsVisible = !TimerInput.IsVisible;
        ProcessInput.IsVisible = !ProcessInput.IsVisible;
    }
}