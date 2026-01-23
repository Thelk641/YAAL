using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using YAAL.Assets.Scripts;
using static YAAL.IsolateSettings;

namespace YAAL;

public partial class Command_Isolate : Command
{
    public CommandSetting<IsolateSettings> CommandSettings => (CommandSetting<IsolateSettings>)settings;

    private Dictionary<IsolateSettings, string> defaultValues = new Dictionary<IsolateSettings, string>() {
        {processName, "" },
        {outputToLookFor, "" },
        {timer, "" },
        {modeSelect,"Process exit" }
    };

    private List<string> modeList = new List<string>() 
    {
        "Process exit",
        "Output contains",
        "Process / Output",
        "Timer",
        "Off"
    };

    public Command_Isolate()
    {
        InitializeComponent();
        settings = new CommandSetting<IsolateSettings>();
        CommandSettings.SetDefaultSetting(defaultValues);
        CommandSettings.SetCommandType("Isolate");

        SetDebouncedEvents();
        TurnEventsOn();
        ModeSelector.ItemsSource = modeList;
        ModeSelector.SelectedIndex = 0;
    }

    public override void SetDebouncedEvents()
    {
        base.SetDebouncedEvents();

        debouncedSettings[Delay] = timer.ToString();

        debouncedEvents["ProcessName"] = _KeyChanged;
        debouncedEvents["CombinedProcess"] = _KeyChanged;
        debouncedEvents["OutputKey"] = _KeyChanged;
        debouncedEvents["OutputContains"] = _OutputChanged;
        debouncedEvents["CombinedOutput"] = _OutputChanged;
    }

    public override void LoadInstruction(Interface_CommandSetting newInstruction)
    {
        base.LoadInstruction(newInstruction);
        TurnEventsOff();
        Delay.Text = CommandSettings.GetSetting(timer);
        ProcessName.Text = CommandSettings.GetSetting(processName);
        CombinedProcess.Text = CommandSettings.GetSetting(processName);
        OutputContains.Text = CommandSettings.GetSetting(outputToLookFor);
        CombinedOutput.Text = CommandSettings.GetSetting(outputToLookFor);
        switch (CommandSettings.GetSetting(modeSelect))
        {
            case "process":
                ModeSelector.SelectedIndex = 0;
                break;
            case "output":
                ModeSelector.SelectedIndex = 1;
                break;
            case "combined":
                ModeSelector.SelectedIndex = 2;
                break;
            case "timer":
                ModeSelector.SelectedIndex = 3;
                break;
            case "off":
                ModeSelector.SelectedIndex = 4;
                break;
            default:
                ModeSelector.SelectedIndex = 0;
                break;
        }
        _ChangedMode(null, null);
        TurnEventsBackOn();
    }
    protected override void TurnEventsOn()
    {
        ModeSelector.SelectionChanged += _ChangedMode;
        Delay.TextChanged += _TextChanged;
        ProcessName.TextChanged += _TextChanged;
        CombinedProcess.TextChanged += _TextChanged;
        OutputKey.TextChanged += _TextChanged;
        OutputContains.TextChanged += _TextChanged;
        CombinedOutput.TextChanged += _TextChanged;
    }
    protected void TurnEventsOff()
    {
        ModeSelector.SelectionChanged -= _ChangedMode;
        Delay.TextChanged -= _TextChanged;
        ProcessName.TextChanged -= _TextChanged;
        CombinedProcess.TextChanged -= _TextChanged;
        OutputKey.TextChanged -= _TextChanged;
        OutputContains.TextChanged -= _TextChanged;
        CombinedOutput.TextChanged -= _TextChanged;
    }

    //Text Changed
    public void _OutputChanged()
    {
        TurnEventsOff();
        if(CombinedOutput.Text == CommandSettings.GetSetting(outputToLookFor))
        {
            CombinedOutput.Text = OutputContains.Text;
        } else
        {
            OutputContains.Text = CombinedOutput.Text;
        }
        CommandSettings.SetSetting(outputToLookFor, OutputContains.Text ?? "");
        TurnEventsBackOn();
    }
    public void _KeyChanged()
    {
        TurnEventsOff();
        string oldKey = CommandSettings.GetSetting(processName);
        if (CombinedProcess.Text != null && CombinedProcess.Text != oldKey)
        {
            ProcessName.Text = CombinedProcess.Text;
            OutputKey.Text = CombinedProcess.Text;
        }
        else if (OutputKey.Text != null && OutputKey.Text != oldKey)
        {
            ProcessName.Text = OutputKey.Text;
            CombinedProcess.Text = OutputKey.Text;
        } else
        {
            CombinedProcess.Text = ProcessName.Text;
            OutputKey.Text = ProcessName.Text;
        }
        CommandSettings.SetSetting(processName, ProcessName.Text ?? "");
        TurnEventsBackOn();
    }
    
    
    // Mode changed
    private void _ChangedMode(object? sender, SelectionChangedEventArgs e)
    {
        Mode_Process.IsVisible = false;
        Mode_Output.IsVisible = false;
        Mode_Combined.IsVisible = false;
        Mode_Timer.IsVisible = false;
        Mode_Off.IsVisible = false;

        switch (ModeSelector.SelectedItem.ToString())
        {
            case "Process exit":
                CommandSettings.SetSetting(modeSelect, "process");
                Mode_Process.IsVisible = true;
                break;
            case "Output contains":
                CommandSettings.SetSetting(modeSelect, "output");
                Mode_Output.IsVisible = true;
                break;
            case "Process / Output":
                CommandSettings.SetSetting(modeSelect, "combined");
                Mode_Combined.IsVisible = true;
                break;
            case "Timer":
                CommandSettings.SetSetting(modeSelect, "timer");
                Mode_Timer.IsVisible = true;
                break;
            case "Off":
                CommandSettings.SetSetting(modeSelect, "off");
                Mode_Off.IsVisible = true;
                break;
        }
    }
}