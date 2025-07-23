using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using YAAL.Assets.Scripts;
using static YAAL.PatchSettings;

namespace YAAL;

public partial class Command_Patch : Command
{
    public List<string> modes = new List<string>()
    {
        "Apply the patch",
        "Copy to the patch to this folder :"
    };
    public Command_Patch()
    {
        InitializeComponent();
        _up = MoveUp;
        _down = MoveDown;
        _delete = X;
        SetDebouncedEvents();
        Optimize.IsChecked = true;
        linkedInstruction = new Patch();
        background = BackgroundColor;
        SetBackground();
        Target.Text = "";
        TurnEventsOn();

        Mode.ItemsSource = modes;
        Mode.SelectedIndex = 0;

        TargetFolder.Click += _FileExplorer;
        
    }

    public override void SetDebouncedEvents()
    {
        base.SetDebouncedEvents();
        debouncedSettings[Target] = target.ToString();

        explorers[TargetFolder] = Target;
    }

    public override void LoadInstruction(Interface_Instruction newInstruction)
    {
        TurnEventsOff();
        linkedInstruction = newInstruction;
        Target.Text = linkedInstruction.GetSetting(target.ToString());

        if(linkedInstruction.GetSetting(mode.ToString()) == "Copy")
        {
            Mode.SelectedIndex = 1;
            _ModeChanged(null, null);
        }
        Optimize.IsChecked = linkedInstruction.GetSetting(optimize.ToString()) == true.ToString();
        TurnEventsBackOn();
    }

    protected override void TurnEventsOn()
    {
        Mode.SelectionChanged += _ModeChanged;
        Target.TextChanged += _TextChanged;
        Optimize.IsCheckedChanged += _OptimizeChanged;
    }

    protected void TurnEventsOff()
    {
        Mode.SelectionChanged -= _ModeChanged;
        Target.TextChanged -= _TextChanged;
        Optimize.IsCheckedChanged -= _OptimizeChanged;
    }

    private void _OptimizeChanged(object? sender, RoutedEventArgs e)
    {
        linkedInstruction.SetSetting(optimize.ToString(), Optimize.IsChecked.ToString());
    }

    private void _ModeChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (Mode.SelectedItem.ToString())
        {
            case "Apply the patch":
                linkedInstruction.SetSetting(mode.ToString(), "Apply");
                ApplyGrid.IsVisible = true;
                CopyGrid.IsVisible = false;
                break;
            case "Copy to the patch to this folder :":
                linkedInstruction.SetSetting(mode.ToString(), "Copy");
                ApplyGrid.IsVisible = false;
                CopyGrid.IsVisible = true;
                break;
        }
    }
}