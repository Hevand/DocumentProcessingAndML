using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Model
{
    public class ModelProcessingRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
