using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DocumentType
    {
        Lease,
        Debt
    }
}
