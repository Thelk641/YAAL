using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.TextFormatting.Unicode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using YAAL.Assets.Scripts;

namespace YAAL;

public partial class App : Application
{
    string launcher = "";
    string async = "";
    string slot = "";
    bool hasErroredOut = false;

    public static UISettings Settings { get; } = new UISettings();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Newtonsoft.Json.JsonConvert.DefaultSettings = () => new Newtonsoft.Json.JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            Converters = { new CachedBrushConverter() }
        };

        /*if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new CustomThemeMaker();
            base.OnFrameworkInitializationCompleted();
            return;
        }*/


        //TODO : this is debug
        //ParseBuildErrors();
        //return;
        Settings.LoadThemes();
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();

        if(args.Length == 0)
        {
            //args = new string[5] {"--restore", "--async", "Pingu ER", "--slot", "Masaru Mengsk" };
            //args = new string[1] { "--restore" };
        }

        //var args = new string[2] { "--async PatchTest", "--slot Slot 2" };
        //var args = new string[2]{"--error ", "D:\\Unity\\Avalonia port\\YAAL\\Logs\\31-07-2025-22-27-27.json" };
        //var args = new string[2]{"--restore", "--exit" };
        //var args = new string[4] { "--async", "\"Debug_CLMaker_Async\"", "--slot\"Debug_CLMaker_Slot\"", "--launcher\"Universal Tracker\"" };
        //var args = new string[4] { "--async", "Early test", "--slot", "M-Ash-saru", };

        if (args == null || args.Length == 0)
        {
            Start();
            return;
        }

        string argsString = "";

        foreach (var item in args)
        {
            argsString += item + " ";
        }
        string[] split;
        string settingName = "";
        string settingValue = "";
        string[] temp = argsString.Split("--");

        foreach (var item in argsString.Split("--"))
        {
            if(item.Trim() == "")
            {
                continue;
            }
            split = item.Split(" ");
            int i = 0;
            while (settingName == "")
            {
                settingName = split[i].Trim();
                ++i;
            }
            for (int j = i; j < split.Length; j++)
            {
                settingValue += split[j] + " ";
            }
            if(!ParseArgs(settingName.Trim(), settingValue.Trim()))
            {
                return;
            }
            
            settingName = "";
            settingValue = "";
        }

        if (hasErroredOut)
        {
            ErrorManager.ThrowError();
            Environment.Exit(1);
        }


        if (launcher != "" && !IOManager.GetLauncherList().Contains(launcher))
        {
            ErrorManager.ThrowError(
                "App - Invalid launcher name",
                "Launcher " + launcher + " doesn't seem to exists."
                );
            Environment.Exit(1);
        }

        if(async != "")
        {
            if(!IOManager.GetAsyncList().Contains(async))
            {
                ErrorManager.ThrowError(
                    "App - Invalid async name",
                    "Async " + async + " doesn't seem to exist"
                    );
                Environment.Exit(1);
            }

            if(slot == "")
            {
                ErrorManager.ThrowError(
                "App - Empty slot name",
                "You've picked an async, but no slot, this is not allowed."
                );
                Environment.Exit(1);
            } else
            {
                if(!IOManager.GetSlotList(async).Contains(slot))
                {
                    ErrorManager.ThrowError(
                        "App - Invalid slot name",
                        "Async " + async + " doesn't contain a slot named " + slot
                        );
                    Environment.Exit(1);
                }
            }

            if(launcher == "")
            {
                launcher = IOManager.GetLauncherNameFromSlot(async, slot);
                if(launcher == null || launcher == "")
                {
                    ErrorManager.ThrowError(
                        "App - Coudldn't automatically get launcher name",
                        "You set an async and slot, but this slot doesn't have a launcher set. Either set one or use --launcher to manually pick one"
                        );
                    Environment.Exit(1);
                }
            }
        }

        if (launcher != "" && async != "" && slot != "")
        {
            // for some reason, the instruction are not set to this launcher !?
            CustomLauncher customLauncher = IOManager.LoadLauncher(launcher);
            customLauncher.ReadSettings(async, slot);
            customLauncher.Execute();
            if (customLauncher.waitingForRestore)
            {
                _ = WaitForRestore(customLauncher);
                base.OnFrameworkInitializationCompleted();
                return;
            } else
            {
                base.OnFrameworkInitializationCompleted();
                ErrorManager.ThrowError();
                Environment.Exit(0);
            }
        }

        Start();
    }

    private void ParseBuildErrors()
    {
        Dictionary<string, List<string>> warnings = new Dictionary<string, List<string>>();
        var warningRegex = new Regex(@"(?<file>.+?)\((?<line>\d+),(?<col>\d+)\): warning (?<code>CS\d+): (?<message>.+)", RegexOptions.Compiled);


        Dictionary<string, List<string>> warningsByCode = new();

        foreach (string line in File.ReadLines("D:\\Unity\\Avalonia port\\build.log"))
        {
            var match = warningRegex.Match(line);
            if (!match.Success) continue;

            string code = match.Groups["code"].Value;
            string message = $"{match.Groups["file"]}:{match.Groups["line"]},{match.Groups["col"]} - {match.Groups["message"].Value}";

            if (!warningsByCode.ContainsKey(code))
                warningsByCode[code] = new List<string>();

            warningsByCode[code].Add(message);
        }

        // Ensure logs folder exists
        string logsDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        Directory.CreateDirectory(logsDir);

        foreach (var kvp in warningsByCode)
        {
            string path = Path.Combine(logsDir, $"{kvp.Key}.txt");
            File.WriteAllLines(path, kvp.Value);
            Console.WriteLine($"Saved {kvp.Value.Count} warnings to {path}");
        }

        Environment.Exit(0);
    }

    private void Start()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            /*if (launcher != "")
            {
                CLMakerWindow clmaker = new CLMakerWindow(launcher);
                desktop.MainWindow = clmaker;
            }
            else
            {
                desktop.MainWindow = new MainWindow();
                //desktop.MainWindow = new UpdateWindow();
            }*/
            //desktop.MainWindow = new CLMakerWindow(launcher);
            desktop.MainWindow = WindowManager.GetMainWindow();
        }
        base.OnFrameworkInitializationCompleted();
    }

    private bool ParseArgs(string name, string value)
    {
        switch (name)
        {
            case "error":
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    ErrorManager.ReadError(value, desktop);
                }
                return false;
            case "launcher":
                launcher = value;
                break;
            case "async":
                async = value;
                break;
            case "slot":
                slot = value;
                break;
            case "restore":
                Restore();
                break;
            case "exit":
                Environment.Exit(0);
                break;
            default:
                ErrorManager.AddNewError(
                    "App - Incorrect argument",
                    "Tried to parse argument of type " + name + " which isn't a valid argument type."
                    );
                break;
        }
        return true;
    }

    private void Restore()
    {
        if (!IOManager.RestoreApworlds())
        {
            ErrorManager.ThrowError(
                    "App - Failed to restore apworlds",
                    "Something went wrong while trying to restore apworlds directly."
                    );
            return;
        }

        if (!IOManager.RestoreBackups())
        {
            ErrorManager.ThrowError(
                    "App - Failed to restore backups",
                    "Something went wrong while trying to restore backups directly."
                    );
            return;
        }

        IOManager.ResetBackupList();
    }

    static async Task WaitForRestore(CustomLauncher launcher)
    {
        var tcs = new TaskCompletionSource();

        launcher.DoneRestoring += () =>
        {
            tcs.SetResult();
        };

        await tcs.Task;

        ErrorManager.ThrowError();
        Environment.Exit(0);
    }
}