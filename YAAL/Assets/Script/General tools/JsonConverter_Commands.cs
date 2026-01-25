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
using Newtonsoft.Json.Linq;
using System.Security.Principal;

namespace YAAL
{
    public class CommandSettingConverter : JsonConverter<List<Interface_CommandSetting>>
    {
        public override List<Interface_CommandSetting>? ReadJson(JsonReader reader, Type objectType, List<Interface_CommandSetting>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JArray JsonObject = JArray.Load(reader); 

            List<Interface_CommandSetting> commands = new List<Interface_CommandSetting>();

            foreach (var item in JsonObject)
            {
                if (Templates.GetCommand(item["commandType"].ToString()) is Type type)
                {
                    Type enumType = type.BaseType!.GetGenericArguments()[0];
                    Type toAdd = typeof(CommandSetting<>).MakeGenericType(enumType);
                    try
                    {
                        commands.Add((Interface_CommandSetting)item.ToObject(toAdd, serializer));
                    }
                    catch (Exception e)
                    {
                        ErrorManager.ThrowError(
                            "JsonConverter_Command - Failed to parse command",
                            "Trying to read a json raised the following exception : " + e.Message
                            );
                    }
                    
                }
            }



            return commands;
        }

        public override void WriteJson(JsonWriter writer, List<Interface_CommandSetting>? value, JsonSerializer serializer)
        {
            var thisIndex = serializer.Converters.IndexOf(this);
            serializer.Converters.RemoveAt(thisIndex);

            serializer.Serialize(writer, value);

            serializer.Converters.Insert(thisIndex, this);
        }
    }
}