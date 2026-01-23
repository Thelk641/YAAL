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
    public CommandSetting<WaitSettings> CommandSettings => (CommandSetting<WaitSettings>)settings;

    private Dictionary<WaitSettings, string> defaultValues = new Dictionary<WaitSettings, string>() {
        {mode,"Timer" },
        {processName, "" },
        {timer, "" }
    };

    public Command_Wait()
    {
        InitializeComponent();
        settings = new CommandSetting<WaitSettings>();
        CommandSettings.SetDefaultSetting(defaultValues);
        CommandSettings.SetCommandType("Wait");

        SetDebouncedEvents();

        ModeSelector.ItemsSource = new List<string>() { "Pause launcher until timer is finished", "Keep launcher open until process closes" };

        ModeSelector.SelectedIndex = 0;
        WindowManager.UpdateComboBox(ModeSelector);
        TurnEventsOn();
    }

    

    public override void LoadInstruction(Interface_CommandSetting newInstruction)
    {
        TurnEventsOff();
        base.LoadInstruction(newInstruction);

        TimerInput.Text = CommandSettings.GetSetting(timer);
        ProcessInput.Text = CommandSettings.GetSetting(processName);

        if (CommandSettings.GetSetting(mode) == "Process")
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
            CommandSettings.SetSetting(mode, "Timer");
        }
        else
        {
            CommandSettings.SetSetting(mode, "Process");
        }

        TimerInput.IsVisible = !TimerInput.IsVisible;
        ProcessInput.IsVisible = !ProcessInput.IsVisible;
    }
}