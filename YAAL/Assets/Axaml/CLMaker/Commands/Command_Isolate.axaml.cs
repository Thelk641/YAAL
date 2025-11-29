using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using YAAL.Assets.Scripts;
using static YAAL.BackupSettings;

namespace YAAL;

public partial class Command_Isolate : Command
{
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
        linkedInstruction = new Isolate();
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

    public override void LoadInstruction(Interface_Instruction newInstruction)
    {
        base.LoadInstruction(newInstruction);
        TurnEventsOff();
        Delay.Text = this.linkedInstruction.GetSetting(timer.ToString());
        ProcessName.Text = this.linkedInstruction.GetSetting(processName.ToString());
        CombinedProcess.Text = this.linkedInstruction.GetSetting(processName.ToString());
        OutputContains.Text = this.linkedInstruction.GetSetting(outputToLookFor.ToString());
        CombinedOutput.Text = this.linkedInstruction.GetSetting(outputToLookFor.ToString());
        switch (this.linkedInstruction.GetSetting(modeSelect.ToString()))
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
        if(CombinedOutput.Text == linkedInstruction.GetSetting(outputToLookFor.ToString()))
        {
            CombinedOutput.Text = OutputContains.Text;
        } else
        {
            OutputContains.Text = CombinedOutput.Text;
        }
        linkedInstruction.SetSetting(outputToLookFor.ToString(), OutputContains.Text ?? "");
        TurnEventsBackOn();
    }
    public void _KeyChanged()
    {
        TurnEventsOff();
        string oldKey = linkedInstruction.GetSetting(processName.ToString());
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
        linkedInstruction.SetSetting(processName.ToString(), ProcessName.Text ?? "");
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
                linkedInstruction.SetSetting(modeSelect.ToString(), "process");
                Mode_Process.IsVisible = true;
                break;
            case "Output contains":
                linkedInstruction.SetSetting(modeSelect.ToString(), "output");
                Mode_Output.IsVisible = true;
                break;
            case "Process / Output":
                linkedInstruction.SetSetting(modeSelect.ToString(), "combined");
                Mode_Combined.IsVisible = true;
                break;
            case "Timer":
                linkedInstruction.SetSetting(modeSelect.ToString(), "timer");
                Mode_Timer.IsVisible = true;
                break;
            case "Off":
                linkedInstruction.SetSetting(modeSelect.ToString(), "off");
                Mode_Off.IsVisible = true;
                break;
        }
    }
}