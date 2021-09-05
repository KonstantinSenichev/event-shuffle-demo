using System;
using System.Globalization;
using System.Text.Json;

namespace EventShuffle.FunctionApp.V1
{
    public class JsonDateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
    {
        private static readonly string DateFormatString = Environment.GetEnvironmentVariable("DateFormatString");

        public static string ToDateOnlyString(DateTime date)
        {
            return date.ToString(DateFormatString);
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var date = DateTime.ParseExact(reader.GetString(), DateFormatString, CultureInfo.InvariantCulture);
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            return date;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(ToDateOnlyString(value));
        }
    }
}
