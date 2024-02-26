using Newtonsoft.Json;

namespace APSToolkit.Schema;

public class ForceDefaultConverter : JsonConverter
{
    public override bool CanRead => false;
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType)
    {
        throw new NotImplementedException();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
