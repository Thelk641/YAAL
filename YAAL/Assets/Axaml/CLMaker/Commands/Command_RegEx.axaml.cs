using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using YAAL.Assets.Scripts;
using static YAAL.RegExSettings;

namespace YAAL;

public partial class Command_RegEx : Command
{
    public Command_RegEx()
    {
        InitializeComponent();
        BackgroundSetter.Set(BackgroundColor, GeneralSettings.foregroundColor);
        SetDebouncedEvents();
        RegEx.Text = "localhost|archipelago\\.gg:\\d+";
        linkedInstruction = new RegEx();
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

    public override void LoadInstruction(Interface_Instruction newInstruction)
    {
        TurnEventsOff();
        linkedInstruction = newInstruction;
        InputFile.Text = this.linkedInstruction.GetSetting(targetFile.ToString());
        InputString.Text = this.linkedInstruction.GetSetting(targetString.ToString());
        OutputFile.Text = this.linkedInstruction.GetSetting(outputFile.ToString());
        OutputString.Text = this.linkedInstruction.GetSetting(outputVar.ToString());
        RegEx.Text = this.linkedInstruction.GetSetting(regex.ToString());
        Replacement.Text = this.linkedInstruction.GetSetting(replacement.ToString());
        if (this.linkedInstruction.GetSetting(modeInput.ToString()) == "String")
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
        if (this.linkedInstruction.GetSetting(modeOutput.ToString()) == "String")
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
            this.linkedInstruction.SetSetting(modeInput.ToString(), "String");
        }
        else
        {
            GridInputFile.IsVisible = true;
            GridInputString.IsVisible = false;
            this.linkedInstruction.SetSetting(modeInput.ToString(), "File");
        }
    }

    private void _OutputTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (OutputType.SelectedItem.ToString() == "Store in var :")
        {
            GridOutputFile.IsVisible = false;
            GridOutputString.IsVisible = true;
            this.linkedInstruction.SetSetting(modeOutput.ToString(), "String");
        }
        else
        {
            GridOutputFile.IsVisible = true;
            GridOutputString.IsVisible = false;
            this.linkedInstruction.SetSetting(modeOutput.ToString(), "File");
        }
    }
}