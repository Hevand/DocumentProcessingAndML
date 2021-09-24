using Newtonsoft.Json;
using System.Collections.Generic;

namespace Common.Model
{
    public class ProcessingBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("userName")]
        public string User { get; set; }

        public string Name { get; set; }

        public DocumentType Type { get; set; }
        
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
