using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using YAAL.Assets.Scripts;
using static YAAL.PatchSettings;

namespace YAAL;

public partial class Command_Patch : Command
{
    public CommandSetting<PatchSettings> CommandSettings => (CommandSetting<PatchSettings>)settings;

    private Dictionary<PatchSettings, string> defaultValues = new Dictionary<PatchSettings, string>() {
        {mode, "Apply" },
        {target, "" },
        {optimize, "True" },
        {rename, "" }
    };
    public List<string> modes = new List<string>()
    {
        "Apply the patch",
        "Copy to the patch to this folder :"
    };
    public Command_Patch()
    {
        InitializeComponent();
        settings = new CommandSetting<PatchSettings>();
        CommandSettings.SetDefaultSetting(defaultValues);
        CommandSettings.SetCommandType("Patch");

        SetDebouncedEvents();
        Optimize.IsChecked = true;
        Target.Text = "";
        TurnEventsOn();

        Mode.ItemsSource = modes;
        Mode.SelectedIndex = 0;

        TargetFolder.Click += _FolderExplorer;
        
    }

    public override void SetDebouncedEvents()
    {
        base.SetDebouncedEvents();
        debouncedSettings[Target] = target.ToString();
        debouncedSettings[NewName] = rename.ToString();

        explorers[TargetFolder] = Target;
    }

    public override void LoadInstruction(Interface_CommandSetting newInstruction)
    {
        TurnEventsOff();
        base.LoadInstruction(newInstruction);
        Target.Text = CommandSettings.GetSetting(target);
        NewName.Text = CommandSettings.GetSetting(rename);

        if(CommandSettings.GetSetting(mode) == "Copy")
        {
            Mode.SelectedIndex = 1;
            _ModeChanged(null, null);
        }
        Optimize.IsChecked = CommandSettings.GetSetting(optimize) == true.ToString();
        TurnEventsBackOn();
    }

    protected override void TurnEventsOn()
    {
        Mode.SelectionChanged += _ModeChanged;
        Target.TextChanged += _TextChanged;
        NewName.TextChanged += _TextChanged;
        Optimize.IsCheckedChanged += _OptimizeChanged;
    }

    protected void TurnEventsOff()
    {
        Mode.SelectionChanged -= _ModeChanged;
        Target.TextChanged -= _TextChanged;
        NewName.TextChanged -= _TextChanged;
        Optimize.IsCheckedChanged -= _OptimizeChanged;
    }

    private void _OptimizeChanged(object? sender, RoutedEventArgs e)
    {
        CommandSettings.SetSetting(optimize, Optimize.IsChecked.ToString());
    }

    private void _ModeChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (Mode.SelectedItem.ToString())
        {
            case "Apply the patch":
                CommandSettings.SetSetting(mode, "Apply");
                ApplyGrid.IsVisible = true;
                CopyGrid.IsVisible = false;
                break;
            case "Copy to the patch to this folder :":
                CommandSettings.SetSetting(mode, "Copy");
                ApplyGrid.IsVisible = false;
                CopyGrid.IsVisible = true;
                break;
        }
    }
}