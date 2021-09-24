using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Model
{
    public class ModelProcessingResult
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public string Id { get; set; }

        [Newtonsoft.Json.JsonProperty("questions")]
        public IEnumerable<ModelQuestionResult> Questions { get; set; }
    }

    public class ModelQuestionResult
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public string Id { get; set; }

        [Newtonsoft.Json.JsonProperty("answers")]
        public IEnumerable<ModelQuestionAnswer> Answers { get; set; }
    }

    public class ModelQuestionAnswer
    {
        [Newtonsoft.Json.JsonProperty("score")]
        public float Score { get; set; }
        
        [Newtonsoft.Json.JsonProperty("start")]
        public int Start { get; set; }
        
        [Newtonsoft.Json.JsonProperty("end")]
        public int End { get; set; }
        
        [Newtonsoft.Json.JsonProperty("answer")]
        public string Answer { get; set; }

    }
}
