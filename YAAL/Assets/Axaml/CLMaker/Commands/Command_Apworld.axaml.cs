using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using YAAL.Assets.Scripts;
using static YAAL.ApworldSettings;

namespace YAAL;

public partial class Command_Apworld : Command
{
    public CommandSetting<ApworldSettings> CommandSettings => (CommandSetting<ApworldSettings>)settings;

    private Dictionary<ApworldSettings, string> defaultValues = new Dictionary<ApworldSettings, string>() { 
        {apworldTarget,"" },
        {processName, "" },
        {optimize,"True" },
        {necessaryFile,"True" }
    };

    public Command_Apworld()
    {
        InitializeComponent();
        settings = new CommandSetting<ApworldSettings>();
        CommandSettings.SetDefaultSetting(defaultValues);
        CommandSettings.SetCommandType("Apworld");

        SetDebouncedEvents();
        TurnEventsOn();
        IsNecessary.IsChecked = true;
        Optimize.IsChecked = true;
        FileTarget.Click += _FileExplorer;
        FolderTarget.Click += _FolderExplorer;
        AutoTheme.SetTheme(TrueBackground, ThemeSettings.transparent);
    }

    protected override void TurnEventsOn()
    {
        BackupTarget.TextChanged += _TextChanged;
        IsNecessary.Click += _IsNecessaryChanged;
        Optimize.IsCheckedChanged += _OptimizeChanged;
        VarName.TextChanged += _TextChanged;
    }

    public override void SetDebouncedEvents()
    {
        base.SetDebouncedEvents();
        debouncedSettings[BackupTarget] = apworldTarget.ToString();
        debouncedSettings[VarName] = processName.ToString();

        explorers[FileTarget] = BackupTarget;
        explorers[FolderTarget] = BackupTarget;

    }

    protected void TurnEventsOff()
    {
        BackupTarget.TextChanged -= _TextChanged;
        Optimize.IsCheckedChanged -= _OptimizeChanged;
        VarName.TextChanged -= _TextChanged;
    }

    public override void LoadInstruction(Interface_CommandSetting newSettings)
    {
        TurnEventsOff();
        base.LoadInstruction(newSettings);
        BackupTarget.Text = CommandSettings.GetSetting(apworldTarget);
        IsNecessary.IsChecked = CommandSettings.GetSetting(necessaryFile) == true.ToString();
        Optimize.IsChecked = CommandSettings.GetSetting(optimize) == true.ToString();
        VarName.Text = CommandSettings.GetSetting(processName);
        TurnEventsBackOn();
    }



    private void _IsNecessaryChanged(object? sender, RoutedEventArgs e)
    {
        CommandSettings.SetSetting(necessaryFile, IsNecessary.IsChecked.ToString() ?? "False");
    }

    private void _OptimizeChanged(object? sender, RoutedEventArgs e)
    {
        CommandSettings.SetSetting(optimize, Optimize.IsChecked.ToString() ?? "False");
    }
}