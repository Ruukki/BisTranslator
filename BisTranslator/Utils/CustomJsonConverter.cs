using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BisTranslator.Utils
{
    public class CustomJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var jsonString = JsonConvert.SerializeObject(value);
            var base64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
            writer.WriteValue(base64Encoded);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) return null;
            var base64Encoded = (string)reader.Value;
            var jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(base64Encoded));
            return JsonConvert.DeserializeObject(jsonString, objectType);
        }

        public override bool CanConvert(Type objectType)
        {
            return true; // Can be adjusted to only convert certain types if needed
        }
    }
}
