using YAAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using static YAAL.FileSettings;
using static YAAL.GeneralSettings;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using ReactiveUI;
using System.Reflection;

namespace YAAL
{
    public static class DefaultManager
    {
        private static Dictionary<Type, Func<object>> Factory = new Dictionary<Type, Func<object>>
        {
            {
                typeof(Cache_GeneralTheme), () =>
                {
                    return generalTheme;
                }

            },

            {
                typeof(Cache_UserSettings), () =>
                {
                    string path = FileSettings.userSettings.GetFullPath();
                    IOManager.SaveCache<Cache_UserSettings>(path, userSettings);
                    return userSettings;
                }
            },

            {
                typeof(Cache_CustomLauncher), () =>
                {
                    return launcher;
                }
            },
        };


        public static T GetDefault<T>() where T : new()
        {
            if(Factory.TryGetValue(typeof(T), out var creator))
            {
                return (T)creator();
            }

            return new T();
        }

        public static Cache_CustomTheme launcherTheme
        {
            get
            {
                string json = LoadFile("launcherTheme.json");
                return JsonConvert.DeserializeObject<Cache_CustomTheme>(json)!;
            }
        }

        public static Cache_GeneralTheme generalTheme
        {
            get
            {
                string json = LoadFile("generalTheme.json");
                return JsonConvert.DeserializeObject<Cache_GeneralTheme>(json)!;
            }
        }

        public static Cache_UserSettings userSettings
        {
            get
            {
                string json = LoadFile("userSettings.json");
                return JsonConvert.DeserializeObject<Cache_UserSettings>(json)!;
            }
        }

        public static Cache_CustomLauncher launcher
        {
            get
            {
                string json = LoadFile("launcher.json");
                return JsonConvert.DeserializeObject<Cache_CustomLauncher>(json)!;
            }
        }

        public static Cache_CustomLauncher GetDefaultLauncher(DefaultTools name)
        {
            string json = "";

            switch (name)
            {
                case DefaultTools.cheeseTracker:
                    json = LoadFile("cheesetracker.json");
                    break;
                case DefaultTools.textClient:
                    json = LoadFile("textClient.json");
                    break;
                case DefaultTools.webTracker:
                    json = LoadFile("webTracker.json");
                    break;
            }

            return JsonConvert.DeserializeObject<Cache_CustomLauncher>(json)!;
        }

        private static string LoadFile(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            string trueName = "YAAL.Assets.Script.Managers.DefaultManager.Config." + fileName;

            using (Stream? stream = assembly.GetManifestResourceStream(trueName))
            {
                if (stream == null)
                {
                    ErrorManager.ThrowError(
                        "DefaultManager - File Not Found",
                        $"Embedded resource '{trueName}' not found."
                        );
                    throw new FileNotFoundException($"Embedded resource '{trueName}' not found.");
                }  

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}