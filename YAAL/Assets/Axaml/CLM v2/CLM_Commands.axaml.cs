using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Data;
using YAAL.Assets.Scripts;
using static YAAL.ApworldSettings;

namespace YAAL;

public partial class CLM_Commands : UserControl
{
    public Dictionary<Button, KeyValuePair<Avalonia.Svg.Skia.Svg, TextBlock>> buttonsList = new Dictionary<Button, KeyValuePair<Avalonia.Svg.Skia.Svg, TextBlock>>();
    private CLM clm;

    public CLM_Commands()
    {
        InitializeComponent();
        AutoTheme.SetTheme(CommandBackground, ThemeSettings.backgroundColor);
        AutoTheme.SetTheme(PaddingBorder, ThemeSettings.backgroundColor);
    }

    public CLM_Commands(CLM newWindow) : this()
    {
        clm = newWindow;
        CommandSelector.ItemsSource = Templates.commandNames;
        CommandSelector.SelectedIndex = 0;

        CommandAdder.Click += (_, _) =>
        {
            if (CommandSelector.SelectedItem is string commandName)
            {
                AddCommand(commandName);
            }
        };

        AutoTheme.SetScrollbarTheme(Scroll);
    }

    public void LoadCommands(List<Interface_CommandSetting> toAdd)
    {
        CommandContainer.Children.Clear();
        foreach (var item in toAdd)
        {
            if (AddCommand(item.GetCommandType()) is Command added)
            {
                added.LoadInstruction(item);
            }
        }
    }

    public Command? AddCommand(string commandName)
    {
        if (Templates.commandTemplates.TryGetValue(commandName, out var commandType))
        {
            Command command = (Command)Activator.CreateInstance(commandType)!;
            CommandContainer.Children.Add(command);
            // TODO : what does this do and why is it there ?
            //command.SetCustomLauncher(clm.CustomLauncher);
            command.holder = this;
            return command;
        }
        ErrorManager.ThrowError(
            "CLM_Commands - Command not found",
            "Tried to add a command of type " + commandName + "which doesn't exists in commandTemplates. Please report this issue.");
        return null;
    }

    public List<Interface_CommandSetting> GetCommands()
    {
        List<Interface_CommandSetting> output = new List<Interface_CommandSetting>();
        foreach (var item in CommandContainer.Children)
        {
            if(item is Command command)
            {
                output.Add(command.GetSettings());
            }
        }

        return output;
    }

    public void RemoveCommand(Command command)
    {
        if (CommandContainer.Children.Contains(command))
        {
            CommandContainer.Children.Remove(command);
        }
    }

    public void MoveCommandUp(Command command)
    {
        int index = CommandContainer.Children.IndexOf(command);
        if(index > 0)
        {
            CommandContainer.Children.Remove(command);
            CommandContainer.Children.Insert(index - 1, command);
        }
    }

    public void MoveCommandDown(Command command)
    {
        int index = CommandContainer.Children.IndexOf(command);
        if (index < CommandContainer.Children.Count - 1)
        {
            CommandContainer.Children.Remove(command);
            CommandContainer.Children.Insert(index + 1, command);
        }
    }
}