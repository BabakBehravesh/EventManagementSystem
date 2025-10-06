using System.Text.Json;

namespace EventManagementSystem.Infrastructure.QrCode
{
    public class QRDataBuilder
    {
        private readonly Dictionary<string, string> _data = new Dictionary<string, string>();
        private string _delimiter = "|";
        private string _format = "KEYVALUE";

        public QRDataBuilder WithParticipant(string participantId)
        {
            _data["PARTICIPANT"] = participantId;
            return this;
        }

        public QRDataBuilder WithEvent(string eventId)
        {
            _data["EVENT"] = eventId;
            return this;
        }

        public QRDataBuilder WithTimestamp(DateTime? timestamp)
        {
            _data["TIMESTAMP"] = (timestamp ?? DateTime.Now).ToString("yyyyMMddHHmmss");
            return this;
        }

        public QRDataBuilder WithDelimiter(string delimiter)
        {
            _delimiter = delimiter;
            return this;
        }

        public QRDataBuilder WithCustomField(string key, string value)
        {
            if(!string.IsNullOrEmpty(key))
            {
                _data[key.ToUpperInvariant()] = value;
            }
            return this;
        }

        public QRDataBuilder AsJsonFormat()
        {
            _format = "JSON";
            return this;
        }

        public QRDataBuilder AsKeyValueFormat()
        {
            _format = "KEYVALUE";
            return this;
        }

        public string Build()
        {
            return _format switch
            {
                "JSON" => BuildJson(),
                _ => BuildKeyValue()
            };
        }

        private string BuildKeyValue()
        {
            return string.Join(_delimiter, _data.Select(kv => $"{kv.Key}:{kv.Value}"));
        }

        private string BuildJson()
        {
            return System.Text.Json.JsonSerializer.Serialize(_data, new JsonSerializerOptions
            {
                WriteIndented = false,
            });
        }

        public override string ToString()
        {
            return Build();
        }
    }
}
