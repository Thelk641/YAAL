using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.OpenGL;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static System.Diagnostics.Debug;
using static YAAL.LauncherSettings;

namespace YAAL;

public partial class CLMakerWindow : Window
{
    public CustomLauncher customLauncher;
    public SettingManager settingManager;
    public List<Command> commandList = new List<Command>();

    public Command AddCommand(string type, bool addToCustomLauncher = true)
    {
        if (Templates.commandTemplates.TryGetValue(type, out var commandType))
        {
            Command command = (Command)Activator.CreateInstance(commandType)!;
            CommandContainer.Children.Add(command);
            commandList.Add(command);
            command.SetCustomLauncher(customLauncher);
            command.clMaker = this;

            // if we're loading, we'll read from listOfInstructions
            // If we're not, we'll be adding to listOfInstructions
            if (addToCustomLauncher) {
                customLauncher.listOfInstructions.Add(command.GetInstruction());
            }
            
            return command;
        }
        ErrorManager.ThrowError(
            "CLMaker_func - Command not found",
            "Tried to add a command of type " + type + "which doesn't exists in commandTemplates. Please report this issue.");
        return null;
    }

    public void RemoveCommand(Command command)
    {
        commandList.Remove(command);
        customLauncher.RemoveInstruction(command.linkedInstruction);
    }

    public void ReloadLauncher()
    {
        List<Interface_Instruction> newList = new List<Interface_Instruction>();
        foreach (var item in commandList)
        {
            newList.Add(item.GetInstruction());
        }
        customLauncher.ResetInstructionList(newList);
    }

    public void ReloadLauncherList(bool autoLoad = false)
    {
        List<string> launcherList = IOManager.GetLauncherList();
        LauncherSelector.ItemsSource = launcherList;
        LauncherSelector.SelectedIndex = 0;
        if (!autoLoad)
        {
            return;
        }

        if (launcherList.Count > 0)
        {
            LoadLauncher(LauncherSelector.SelectedItem.ToString());
        } else
        {
            CreateEmptyLauncher();
        }
    }

    public void MoveUp(Command toMove)
    {
        int index = commandList.IndexOf(toMove);
        commandList.Remove(toMove);
        commandList.Insert(index - 1, toMove);
        customLauncher.MoveInstructionUp(toMove.linkedInstruction);
    }

    public void MoveDown(Command toMove)
    {
        int index = commandList.IndexOf(toMove);
        commandList.Remove(toMove);
        commandList.Insert(index + 1, toMove);
        customLauncher.MoveInstructionDown(toMove.linkedInstruction);
    }


    public string GetVersionToDownload()
    {
        return GitHubVersions.SelectedItem.ToString();
    }

    public string GetGameName()
    {
        return LauncherName.Text;
    }

    public string GetLatestAvailableVersion()
    {
        foreach (var item in GitHubVersions.Items)
        {
            if (AvailableVersions.Items.Source.Contains(item))
            {
                return item.ToString();
            }
        }
        string defaultVersion = AvailableVersions.SelectedItem.ToString();
        if (defaultVersion == null || defaultVersion == "None")
        {
            foreach (var item in commandList)
            {
                if (item is Command_Apworld)
                {
                    ErrorManager.ThrowError(
                        "CLMaker_events - Missing default version",
                        "This customlauncher contains an Apworld instruction, but no installed version. Please install one and try again."
                        );
                    break;
                }
            }
        }
        return defaultVersion;
    }

    public void LoadLauncher(string launcherName)
    {
        if(customLauncher != null)
        {
            Save();
        }
        Debug.WriteLine("Attempting to load launcher " + launcherName);
        if (!IOManager.GetLauncherList().Contains(launcherName))
        {
            ErrorManager.ThrowError(
                "CLMaker_func - Launcher doesn't exists",
                "Unable to load launcher, there doesn't seem to appear a folder named " + launcherName + " containing a launcher.json file.");
            return;
        }
        

        TurnEventsOff();
        customLauncher = IOManager.LoadLauncher(launcherName);
        LauncherName.Text = customLauncher.GetSetting("launcherName");

        foreach (var item in commandList)
        {
            item.DeleteComponent(false);
        }
        commandList = new List<Command>();

        string gitlink = customLauncher.GetSetting("githubURL");


        if (gitlink == null || gitlink == "")
        {
            GitHubVersions.ItemsSource = new List<string> { "None" };
            GitHubVersions.IsEnabled = false;
        } else
        {
            // Let's try to fill up the list of versions from the github link
            UpdateGitVersions();
        }

        foreach (Interface_Instruction inst in customLauncher.listOfInstructions)
        {
            Command command = AddCommand(inst.GetInstructionType(), false);
            command.LoadInstruction(inst);
        }
        UpdateAvailableVersion();

        if(customLauncher.isGame)
        {
            ModeSelector.SelectedIndex = 0;
        } else
        {
            ModeSelector.SelectedIndex = 1;
        }
        TurnEventsBackOn();
    }

    public void UpdateAvailableVersion()
    {
        List<string> newList = IOManager.GetDownloadedVersions(customLauncher.GetSetting(launcherName));
        if (newList.Count > 0)
        {
            AvailableVersions.ItemsSource = newList;
            AvailableVersions.SelectedIndex = 0;
        }
        else
        {
            AvailableVersions.ItemsSource = new List<string> { "None" };
            AvailableVersions.SelectedIndex = 0;
        }
    }

    public void UpdateGitVersions()
    {
        string newLink = this.customLauncher.selfsettings[LauncherSettings.githubURL] ?? "";

        if (newLink == "") 
        {
            GitHubVersions.ItemsSource = new List<string> { "None" };
            GitHubVersions.IsEnabled = false;
            return;
        }

        if (!WebManager.IsValidGitURL(newLink))
        {
            //probably triggered while the user was typing, or there's a typo, either way, better ignore it
            return;
        }
        // This might take a few seconds, so let's just fire it and let it do its thing
        _ = WebManager.GetVersions(newLink, GitHubVersions);
    }
}