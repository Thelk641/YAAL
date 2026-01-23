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
    public CommandSetting<OpenSettings> CommandSettings => (CommandSetting<OpenSettings>)settings;

    private Dictionary<OpenSettings, string> defaultValues = new Dictionary<OpenSettings, string>() {
        {args, "" },
        {processName, "" },
        {path, "" },
        {redirectOutput, "False" }
    };
    public Command_Open()
    {
        InitializeComponent();
        settings = new CommandSetting<OpenSettings>();
        CommandSettings.SetDefaultSetting(defaultValues);
        CommandSettings.SetCommandType("Open");

        SetDebouncedEvents();
        TurnEventsOn();

        File.Click += _FileExplorer;
        Folder.Click += _FolderExplorer;
    }

    public override void LoadInstruction(Interface_CommandSetting newInstruction)
    {
        base.LoadInstruction(newInstruction);
        TurnEventsOff();
        FilePath.Text = CommandSettings.GetSetting(path);
        FileArgs.Text = CommandSettings.GetSetting(args);
        VarName.Text = CommandSettings.GetSetting(processName);
        RedirectOutput.IsChecked = CommandSettings.GetSetting(redirectOutput) == true.ToString();
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
            CommandSettings.SetSetting(redirectOutput, clicked.ToString());
        } else
        {
            CommandSettings.SetSetting(redirectOutput, false.ToString());
        }
    }
}