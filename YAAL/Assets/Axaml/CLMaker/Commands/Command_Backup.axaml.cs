using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using YAAL.Assets.Scripts;
using static YAAL.BackupSettings;
using System.Threading.Tasks;

namespace YAAL;

public partial class Command_Backup : Command
{
    private List<string> modeList = new List<string>() 
    {
        "Process exit",
        "Output contains",
        "Process / Output",
        "Timer",
        "Off"
    };

    public Command_Backup()
    {
        InitializeComponent();
        linkedInstruction = new Backup();
        SetDebouncedEvents();
        TurnEventsOn();
        ModeSelector.ItemsSource = modeList;
        ModeSelector.SelectedIndex = 0;

        BackupFileButton.Click += _FileExplorer;
        BackupFolderButton.Click += _FolderExplorer;
        DefaultFileButton.Click += _FileExplorer;
        DefaultFolderButton.Click += _FolderExplorer;
        AutoTheme.SetTheme(BackgroundColor, ThemeSettings.foregroundColor);
    }

    public override void SetDebouncedEvents()
    {
        base.SetDebouncedEvents();
        debouncedSettings[BackupTarget] = target.ToString();
        debouncedSettings[DefaultFile] = defaultFile.ToString();
        debouncedSettings[Delay] = timer.ToString();

        debouncedEvents["ProcessName"] = _KeyChanged;
        debouncedEvents["CombinedProcess"] = _KeyChanged;
        debouncedEvents["OutputKey"] = _KeyChanged;
        debouncedEvents["OutputContains"] = _OutputChanged;
        debouncedEvents["CombinedOutput"] = _OutputChanged;

        explorers[BackupFileButton] = BackupTarget;
        explorers[BackupFolderButton] = BackupTarget;
        explorers[DefaultFileButton] = DefaultFile;
        explorers[DefaultFolderButton] = DefaultFile;
    }

    public override void LoadInstruction(Interface_Instruction newInstruction)
    {
        base.LoadInstruction(newInstruction);
        TurnEventsOff();
        BackupTarget.Text = this.linkedInstruction.GetSetting(target.ToString());
        DefaultFile.Text = this.linkedInstruction.GetSetting(defaultFile.ToString());
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
        BackupTarget.TextChanged += _TextChanged;
        Delay.TextChanged += _TextChanged;
        ProcessName.TextChanged += _TextChanged;
        CombinedProcess.TextChanged += _TextChanged;
        OutputKey.TextChanged += _TextChanged;
        OutputContains.TextChanged += _TextChanged;
        CombinedOutput.TextChanged += _TextChanged;
        DefaultFile.TextChanged += _TextChanged;
    }
    protected void TurnEventsOff()
    {
        ModeSelector.SelectionChanged -= _ChangedMode;
        BackupTarget.TextChanged -= _TextChanged;
        Delay.TextChanged -= _TextChanged;
        ProcessName.TextChanged -= _TextChanged;
        CombinedProcess.TextChanged -= _TextChanged;
        OutputKey.TextChanged -= _TextChanged;
        OutputContains.TextChanged -= _TextChanged;
        CombinedOutput.TextChanged -= _TextChanged;
        DefaultFile.TextChanged -= _TextChanged;
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