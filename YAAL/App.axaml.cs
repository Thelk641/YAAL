using Avalonia;
using Avalonia.Controls;
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
    bool hasReadLauncher = false;
    bool logDebug = false;
    bool hasErroredOut = false;
    DebugManager logger = new DebugManager();

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

        Trace.Listeners.Add(logger);
        ThemeManager.UpdateGeneralTheme();

        /*if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new CustomThemeMaker();
            base.OnFrameworkInitializationCompleted();
            return;
        }*/


        //TODO : this is debug
        //ParseBuildErrors();
        //return;
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();

#if DEBUG
        if(args.Length == 0)
        {
            //args = new string[5] { "--debug", "--async", "Gnya", "--slot", "Masaru_OoS" };
            //args = new string[7] {"--restore", "--async", "Pingu ER", "--slot", "Masaru_FF", "--launcher", "\"Text Client\"" };
            args = new string[7] {"--debug", "--async", "Gnya", "--slot", "Masaru_OoS", "--launcher", "\"Dice\"" };
            //args = new string[1] { "--debug" };
        }
#endif

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
        } else
        {
            IOManager.UpdateLauncherList();
            hasReadLauncher = true;
        }

            async = async.Trim().Trim('\"').Trim();
        slot = slot.Trim().Trim('\"').Trim();
        launcher = launcher.Trim().Trim('\"').Trim();



        if (launcher != "" && !IOManager.GetLauncherList(true).Contains(launcher))
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
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                //desktop.MainWindow = new Window { IsVisible = false };
                //desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                base.OnFrameworkInitializationCompleted();
            }

            // for some reason, the instruction are not set to this launcher !?
            CustomLauncher customLauncher = IOManager.LoadLauncher(launcher);
            customLauncher.ReadSettings(async, slot);
            customLauncher.Execute();
            if (customLauncher.waitingForRestore)
            {
                _ = WaitForRestore(customLauncher);
                return;
            } else
            {
                ErrorManager.ThrowError();
                if (logDebug)
                {
                    logger.Save();
                }
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
        if (!hasReadLauncher)
        {
            IOManager.UpdateLauncherList();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = WindowManager.GetMainWindow();
            
            if (logDebug && desktop.MainWindow is Avalonia.Controls.Window window)
            {
                window.Closing += (_, _) =>
                {
                    logger.Save();
                };
            }
        }
        base.OnFrameworkInitializationCompleted();
    }

    private bool ParseArgs(string name, string value)
    {
        switch (name)
        {
            case "error":
                App.Settings.IsReadingError = true;
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    base.OnFrameworkInitializationCompleted();
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
                IOManager.RestoreAll();
                break;
            case "exit":
                Environment.Exit(0);
                break;
            case "debug":
                logDebug = true;
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

    async Task WaitForRestore(CustomLauncher launcher)
    {
        var tcs = new TaskCompletionSource();

        launcher.DoneRestoring += () =>
        {
            tcs.SetResult();
        };

        await tcs.Task;

        ErrorManager.ThrowError();
        if (logDebug)
        {
            logger.Save();
        }
        Environment.Exit(0);
    }
}