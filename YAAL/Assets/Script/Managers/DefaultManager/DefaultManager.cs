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
                typeof(Cache_CustomTheme), () =>
                {
                    return theme;
                }

            },

            {
                typeof(Cache_UserSettings), () =>
                {
                    return userSettings;
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



        public static Cache_CustomTheme theme
        {
            get
            {
                string json = LoadFile("theme.json");
                return JsonConvert.DeserializeObject<Cache_CustomTheme>(json)!;
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

        private static string LoadFile(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            string trueName = "YAAL.Assets.Script.Managers.DefaultManager.Config." + fileName;

            using (Stream? stream = assembly.GetManifestResourceStream(trueName))
            {
                if (stream == null)
                {
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