using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using YAAL;
using YAAL.Assets.Script.Cache;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static YAAL.FileSettings;
using static YAAL.GeneralSettings;
using static YAAL.LauncherSettings;

namespace YAAL
{

    public static class GameManager
    {
        public static List<string> games = new List<string>();

        public static List<string> GetGameList()
        {
            if (games.Count == 0)
            {
                ReadGameList();
            }

            return games;
        }

        public static List<Cache_DisplayLauncher> GetLaunchersForGame(string toFind)
        {
            // placeHolder, to be deleted later when Games are a thing

            List<Cache_DisplayLauncher> output = new List<Cache_DisplayLauncher>();
            List<string> otherGames = new List<string>();

            Cache_DisplayLauncher match = new Cache_DisplayLauncher();
            match.name = "-- Match game";
            match.isHeader = true;
            output.Add(match);


            foreach (var item in LauncherManager.launcherList.list)
            {
                string gameName = item.Value;
                if (gameName == toFind)
                {
                    Cache_DisplayLauncher toAdd = new Cache_DisplayLauncher();
                    toAdd.name = item.Key;
                    output.Add(toAdd);
                }
                else
                {
                    otherGames.Add(item.Key);
                }
            }

            Cache_DisplayLauncher other = new Cache_DisplayLauncher();
            other.name = "-- Other games";
            other.isHeader = true;
            output.Add(other);

            foreach (var item in otherGames)
            {
                Cache_DisplayLauncher toAdd = new Cache_DisplayLauncher();
                toAdd.name = item;
                output.Add(toAdd);
            }

            return output;
        }

        public static void ReadGameList()
        {
            games = new List<string>();

            foreach (var launcher in LauncherManager.launcherList.list)
            {
                string gameName = launcher.Value;
                if (!games.Contains(gameName))
                {
                    games.Add(gameName);
                }
            }
        }
    }
}