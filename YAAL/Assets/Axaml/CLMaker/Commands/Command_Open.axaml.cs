using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using YAAL.Assets.Scripts;
using static YAAL.OpenSettings;

namespace YAAL;

public partial class Command_Open : Command
{
    public Command_Open()
    {
        InitializeComponent();
        BackgroundSetter.Set(BackgroundColor, GeneralSettings.foregroundColor);
        linkedInstruction = new Open();
        SetDebouncedEvents();
        TurnEventsOn();

        File.Click += _FileExplorer;
        Folder.Click += _FolderExplorer;
    }

    public override void LoadInstruction(Interface_Instruction newInstruction)
    {
        base.LoadInstruction(newInstruction);
        TurnEventsOff();
        FilePath.Text = this.linkedInstruction.GetSetting(path.ToString());
        FileArgs.Text = this.linkedInstruction.GetSetting(args.ToString());
        VarName.Text = this.linkedInstruction.GetSetting(processName.ToString());
        RedirectOutput.IsChecked = this.linkedInstruction.GetSetting(redirectOutput.ToString()) == true.ToString();
        TurnEventsBackOn();
    }

    public override void SetDebouncedEvents()
    {
        base.SetDebouncedEvents();
        debouncedSettings[FilePath] = path.ToString();
        debouncedSettings[FileArgs] = args.ToString();
        debouncedSettings[VarName] = processName.ToString();

        explorers[File] = FilePath;
        explorers[Folder] = FilePath;
    }

    protected void TurnEventsOff()
    {
        FilePath.TextChanged -= _TextChanged;
        FileArgs.TextChanged -= _TextChanged;
        VarName.TextChanged -= _TextChanged;
        RedirectOutput.Click -= Clicked;
    }

    protected override void TurnEventsOn()
    {
        FilePath.TextChanged += _TextChanged;
        FileArgs.TextChanged += _TextChanged;
        VarName.TextChanged += _TextChanged;
        RedirectOutput.Click += Clicked;
    }

    private void Clicked(object? sender, RoutedEventArgs e)
    {
        if(RedirectOutput.IsChecked is bool clicked)
        {
            this.linkedInstruction.SetSetting(redirectOutput.ToString(), clicked.ToString());
        } else
        {
            this.linkedInstruction.SetSetting(redirectOutput.ToString(), false.ToString());
        }
    }
}