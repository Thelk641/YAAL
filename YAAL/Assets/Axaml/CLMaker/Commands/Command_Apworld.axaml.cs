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
    public Command_Apworld()
    {
        InitializeComponent();
        SetDebouncedEvents();
        linkedInstruction = new Apworld();
        TurnEventsOn();
        IsNecessary.IsChecked = true;
        FileTarget.Click += _FileExplorer;
        FolderTarget.Click += _FolderExplorer;
        AutoTheme.SetTheme(TrueBackground, ThemeSettings.transparent);
    }

    protected override void TurnEventsOn()
    {
        BackupTarget.TextChanged += _TextChanged;
        IsNecessary.Click += _IsNecessaryChanged;
    }

    public override void SetDebouncedEvents()
    {
        base.SetDebouncedEvents();
        debouncedSettings[BackupTarget] = apworldTarget.ToString();

        explorers[FileTarget] = BackupTarget;
        explorers[FolderTarget] = BackupTarget;
    }

    protected void TurnEventsOff()
    {
        BackupTarget.TextChanged -= _TextChanged;
    }

    public override void LoadInstruction(Interface_Instruction newInstruction)
    {
        TurnEventsOff();
        linkedInstruction = newInstruction;
        BackupTarget.Text = this.linkedInstruction.GetSetting(apworldTarget.ToString());
        IsNecessary.IsChecked = (this.linkedInstruction.GetSetting(necessaryFile.ToString()) == true.ToString());
        TurnEventsBackOn();
    }



    private void _IsNecessaryChanged(object? sender, RoutedEventArgs e)
    {
        this.linkedInstruction.SetSetting(necessaryFile.ToString(), IsNecessary.IsChecked.ToString());
    }
}