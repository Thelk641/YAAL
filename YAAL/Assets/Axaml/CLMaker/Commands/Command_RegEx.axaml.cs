using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using YAAL.Assets.Scripts;
using static YAAL.RegExSettings;

namespace YAAL;

public partial class Command_RegEx : Command
{
    public CommandSetting<RegExSettings> CommandSettings => (CommandSetting<RegExSettings>)settings;

    private Dictionary<RegExSettings, string> defaultValues = new Dictionary<RegExSettings, string>() {
        {targetFile,"" },
        {targetString, "" },
        {modeInput, "File" },
        {modeOutput, "File" },
        {regex, "localhost|archipelago\\.gg:\\d+" },
        {replacement, "" },
        {outputFile, "" },
        {outputVar, "" }
    };
    public Command_RegEx()
    {
        InitializeComponent();
        settings = new CommandSetting<RegExSettings>();
        CommandSettings.SetDefaultSetting(defaultValues);
        CommandSettings.SetCommandType("RegEx");

        SetDebouncedEvents();
        RegEx.Text = "localhost|archipelago\\.gg:\\d+";
        InputType.ItemsSource = new List<string>()
        {
            "From file :",
            "From string :"
        };
        InputType.SelectedIndex = 0;
        OutputType.ItemsSource = new List<string>()
        {
            "Save as :",
            "Store in var :"
        };
        OutputType.SelectedIndex = 0;

        InputFileButton.Click += _FileExplorer;
        OutputFileButton.Click += _FileExplorer;
        _InputTypeChanged(null, null);
        _OutputTypeChanged(null, null);

        TurnEventsOn();
    }

    public override void SetDebouncedEvents()
    {
        base.SetDebouncedEvents();
        debouncedSettings[InputFile] = targetFile.ToString();
        debouncedSettings[InputString] = targetString.ToString();
        debouncedSettings[OutputFile] = outputFile.ToString();
        debouncedSettings[OutputString] = outputVar.ToString();
        debouncedSettings[RegEx] = regex.ToString();
        debouncedSettings[Replacement] = replacement.ToString();

        explorers[InputFileButton] = InputFile;
        explorers[OutputFileButton] = OutputFile;
    }

    protected override void TurnEventsOn()
    {
        InputFile.TextChanged += _TextChanged;
        InputString.TextChanged += _TextChanged;
        OutputFile.TextChanged += _TextChanged;
        OutputString.TextChanged += _TextChanged;
        RegEx.TextChanged += _TextChanged;
        Replacement.TextChanged += _TextChanged;
        InputType.SelectionChanged += _InputTypeChanged;
        OutputType.SelectionChanged += _OutputTypeChanged;
    }

    protected void TurnEventsOff()
    {
        InputFile.TextChanged -= _TextChanged;
        InputString.TextChanged -= _TextChanged;
        OutputFile.TextChanged -= _TextChanged;
        OutputString.TextChanged -= _TextChanged;
        RegEx.TextChanged -= _TextChanged;
        Replacement.TextChanged -= _TextChanged;
        InputType.SelectionChanged -= _InputTypeChanged;
    }

    public override void LoadInstruction(Interface_CommandSetting newInstruction)
    {
        TurnEventsOff();
        base.LoadInstruction(newInstruction);
        InputFile.Text = CommandSettings.GetSetting(targetFile);
        InputString.Text = CommandSettings.GetSetting(targetString);
        OutputFile.Text = CommandSettings.GetSetting(outputFile);
        OutputString.Text = CommandSettings.GetSetting(outputVar);
        RegEx.Text = CommandSettings.GetSetting(regex);
        Replacement.Text = CommandSettings.GetSetting(replacement);
        if (CommandSettings.GetSetting(modeInput) == "String")
        {
            GridInputFile.IsVisible = false;
            GridInputString.IsVisible = true;
            InputType.SelectedIndex = 1;
        } else
        {
            GridInputFile.IsVisible = true;
            GridInputString.IsVisible = false;
            InputType.SelectedIndex = 0;
        }
        if (CommandSettings.GetSetting(modeOutput) == "String")
        {
            GridOutputFile.IsVisible = false;
            GridOutputString.IsVisible = true;
            OutputType.SelectedIndex = 1;
        }
        else
        {
            GridOutputFile.IsVisible = true;
            GridOutputString.IsVisible = false;
            OutputType.SelectedIndex = 0;
        }
        _InputTypeChanged(null, null);
        _OutputTypeChanged(null, null);
        TurnEventsBackOn();
    }

    // Combobox changed
    private void _InputTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (InputType.SelectedItem.ToString() == "From string :")
        {
            GridInputFile.IsVisible = false;
            GridInputString.IsVisible = true;
            CommandSettings.SetSetting(modeInput, "String");
        }
        else
        {
            GridInputFile.IsVisible = true;
            GridInputString.IsVisible = false;
            CommandSettings.SetSetting(modeInput, "File");
        }
    }

    private void _OutputTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (OutputType.SelectedItem.ToString() == "Store in var :")
        {
            GridOutputFile.IsVisible = false;
            GridOutputString.IsVisible = true;
            CommandSettings.SetSetting(modeOutput, "String");
        }
        else
        {
            GridOutputFile.IsVisible = true;
            GridOutputString.IsVisible = false;
            CommandSettings.SetSetting(modeOutput, "File");
        }
    }
}