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

namespace YAAL
{
    public class CachedBrushConverter : JsonConverter<Cached_Layer>
    {
        public override Cached_Layer? ReadJson(JsonReader reader, Type objectType, Cached_Layer? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var JsonObject = JObject.Load(reader);
            string brushType = JsonObject["brushType"]?.ToString() ?? "";

            switch (brushType)
            {
                case "Image":
                    return JsonObject.ToObject<Cached_ImageLayer>(GetDefaultSerializer(serializer));
                case "Color":
                    return JsonObject.ToObject<Cached_ColorLayer>(GetDefaultSerializer(serializer));
                default:
                    return null;
            }
        }

        public override void WriteJson(JsonWriter writer, Cached_Layer? value, JsonSerializer serializer)
        {
            JObject JsonObject = JObject.FromObject(value, GetDefaultSerializer(serializer));
            if (value is Cached_ColorLayer) JsonObject["brushType"] = "Color";
            if (value is Cached_ImageLayer) JsonObject["brushType"] = "Image";
            JsonObject.WriteTo(writer);
        }

        public JsonSerializer GetDefaultSerializer(JsonSerializer serializer)
        {
            JsonSerializer defaultSerializer = new JsonSerializer();
            foreach (var item in serializer.Converters)
            {
                if (!(item is CachedBrushConverter))
                {
                    defaultSerializer.Converters.Add(item);
                }
            }
            return defaultSerializer;
        }
    }
}