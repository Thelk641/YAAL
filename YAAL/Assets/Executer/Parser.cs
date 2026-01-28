using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YAAL;
using YAAL.Assets.Script.Cache;
using YAAL.Assets.Scripts;
using static YAAL.AsyncSettings;
using static YAAL.LauncherSettings;
using static YAAL.PreviousAsyncSettings;
using static YAAL.SlotSettings;

public class Parser
{
    public SettingsHandler settingsHandler;
    public ProcessHandler processHandler;

    public Parser(SettingsHandler newSettingsHandler, ProcessHandler newProcessHandler)
    {
        settingsHandler = newSettingsHandler;
        processHandler = newProcessHandler;
    }

    public List<string> ParseTextWithSettings(List<string> input, bool clearQuote = true)
    {
        List<string> output = new List<string>();
        foreach (var item in input)
        {
            output.Add(ParseTextWithSettings(item, clearQuote));
        }
        return output;
    }

    public string ParseTextWithSettings(string text, bool clearQuote = true)
    {
        if (text == null)
        {
            ErrorManager.AddNewError(
                "CustomLauncher - Tried to parse null",
                "Something asked the Custom Launcher to parse a null text. This shouldn't ever happen. Please report this issue."
                );
            processHandler.hasErroredOut = true;
            return "";
        }

        if (text.EndsWith(";"))
        {
            text = text.TrimEnd(';');
        }

        if (!text.Contains("${"))
        {
            while (text.StartsWith("\"") && clearQuote)
            {
                text = text.TrimStart('\"');
            }

            while (text.EndsWith("\"") && clearQuote)
            {
                text = text.TrimEnd('\"');
            }

            if (text.Contains(".apworld") && !text.Contains("\\"))
            {
                return IOManager.FindApworld(settingsHandler.settings[LauncherSettings.apfolder]!, text);
            }

            return text;
        }


        Executer baseLauncher = null;
        if (settingsHandler.settings[SlotSettings.baseLauncher] != settingsHandler.settings[launcherName])
        {
            baseLauncher = settingsHandler.GetBaseLauncher();
            if(baseLauncher == null)
            {
                ErrorManager.AddNewError(
                    "Executer - Failed to find base launcher",
                    "YAAL was asked to parse a tool but couldn't find the game it's meant to run with. This is not allowed"
                    );
                processHandler.hasErroredOut = true;
                return "";
            }
        }

        string tempString = text;

        if (text.Contains("${baseSetting:"))
        {
            if (settingsHandler.settings[IsGame] == true.ToString())
            {
                ErrorManager.AddNewError(
                    "CustomLauncher - Tried to use tool-specific settings in non-tool launcher",
                    "YAAL was asked to parse a baseSetting in a game. This is not allowed. BaseSettings are only there for tools."
                    );
                processHandler.hasErroredOut = true;
                return "";
            }
            foreach (Match m in Regex.Matches(text, @"\$\{baseSetting:(?<key>[^}]+)\}"))
            {

                string key = m.Groups["key"].Value;
                string pattern = "${baseSetting:" + key + "}";

                tempString = tempString.Replace(pattern, baseLauncher!.Parser.ParseTextWithSettings("${" + key + "}"));
            }
        }

        text = tempString;

        if (text.Contains("${base:apworld}"))
        {
            if (settingsHandler.settings[IsGame] == true.ToString())
            {
                ErrorManager.AddNewError(
                    "CustomLauncher - Tried to use tool-specific settings in non-tool launcher",
                    "YAAL was asked to parse base:apworld in a game. This is not allowed. 'Base' options are only there for tools."
                    );
                processHandler.hasErroredOut = true;
                return "";
            }
            text = text.Replace("${base:apworld}", baseLauncher!.SettingsHandler.settings[LauncherSettings.apworld]);
        }

        if (text.Contains("${apworld}"))
        {
            string apworldList = "";
            foreach (var item in settingsHandler.GetApworlds())
            {
                apworldList += "\"" + item + "\";";
            }

            text = text.Replace("${apworld}", apworldList);
        }

        int i = 0;
        while (text.Contains("${") && i < 10)
        {
            // Text might contain things like ${aplauncher}, we're replacing those by their value
            string[] splitText = text.Split("${");

            if (splitText.Length > 1)
            {
                string output = "";
                string cleaned;
                foreach (var item in splitText)
                {
                    if (item == "" || item == "\"" || item == " ")
                    {
                        continue;
                    }

                    if (!item.Contains("}"))
                    {
                        output += item;
                        continue;
                    }

                    cleaned = item;

                    if (item.Trim().StartsWith("\""))
                    {
                        do
                        {
                            cleaned = cleaned.TrimStart('\"').Trim();
                        } while (cleaned.StartsWith("\""));

                        output += "\"";
                    }

                    string[] split = cleaned.Split('}');
                    if (split.Length > 1)
                    {
                        if (settingsHandler.settings.Has(split[0]))
                        {
                            try
                            {
                                if (split[0] == slotInfo.ToString())
                                {
                                    output =
                                        output
                                        + "\""
                                        + settingsHandler.settings[slotName].Trim()
                                        + ":"
                                        + settingsHandler.settings[password]
                                        + "@"
                                        + settingsHandler.settings[roomAddress]
                                        + ":"
                                        + settingsHandler.settings[roomPort]
                                        + "\"";
                                }
                                else if (split[0] == HardcodedSettings.connect.ToString())
                                {
                                    output =
                                        output
                                        + "\""
                                        + "--connect "
                                        + settingsHandler.settings[slotName].Trim()
                                        + ":"
                                        + settingsHandler.settings[password]
                                        + "@"
                                        + settingsHandler.settings[roomAddress]
                                        + ":"
                                        + settingsHandler.settings[roomPort]
                                        + "\"";
                                }
                                else
                                {
                                    output = output + settingsHandler.settings[split[0]].Trim();
                                }
                            }
                            catch (Exception e)
                            {
                                ErrorManager.AddNewError(
                                    "CustomLauncher - Exception while reading settings",
                                    "Trying to parse " + split[0] + " lead to the following exception : " + e.Message
                                    );
                                processHandler.hasErroredOut = true;
                                return "";
                            }


                        }
                        else if (split[0] == "apDebug" && settingsHandler.settings.Has(GeneralSettings.aplauncher))
                        {
                            output = output + IOManager.ToDebug(settingsHandler.settings[GeneralSettings.aplauncher]);
                        }
                        else
                        {
                            ErrorManager.AddNewError(
                                "CustomLauncher - Variable name doesn't exist.",
                                "Trying to parse text with settings failed, variable " + split[0] + " doesn't appear to exist."
                                );
                            processHandler.hasErroredOut = true;
                            return "";
                        }

                        output = output + split[1];
                    }
                    else
                    {
                        output = output + split[0];
                    }

                }

                text = output.Trim();
                ++i;
            }
        }

        return text;
    }

    public List<string> SplitAndParse(string input, bool clearQuote = true)
    {
        List<string> output = new List<string>();

        if (input.Contains(";"))
        {
            List<string> parsedSplit = SplitString(input);
            foreach (var item in parsedSplit)
            {
                output.Add(ParseTextWithSettings(item, clearQuote));
            }
            return output;
        }

        string parsed = ParseTextWithSettings(input, clearQuote);


        if (parsed == "\" \"")
        {
            output.Add("");
        }
        else
        {
            output.Add(parsed);
        }
        return output;
    }

    public List<string> SplitString(string input)
    {
        List<string> output = new List<string>();
        if (!input.Contains(";"))
        {
            output.Add(input);
            return output;
        }

        bool inQuotes = false;
        StringBuilder current = new StringBuilder();

        foreach (char c in input)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                current.Append(c);
            }
            else if (c == ';' && !inQuotes)
            {
                if (current.ToString().EndsWith("\"\""))
                {
                    output.Add(current.ToString().Trim().Trim('\"').Trim() + "\"");
                }
                else
                {
                    output.Add(current.ToString().Trim().Trim('\"').Trim());
                }

                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
        {
            string toAdd = current.ToString().Trim();
            if (toAdd.Trim('\"').Trim() == "")
            {
                output.Add("");
            }
            else
            {
                output.Add(current.ToString());
            }
        }

        return output;
    }
}