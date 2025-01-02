using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlcatModification.Editor.Build
{
    internal class JSONHelper
    {
        public static string ToJSON<T>(T input)
        {
            return JsonConvert.SerializeObject(input, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public static T FromJSON<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        public static void ToJSONFile<T>(string path, T input) =>
            File.WriteAllText(path, ToJSON(input));

        public static T FromJSONFile<T>(string path) =>
            FromJSON<T>(File.ReadAllText(path));
    }
}
