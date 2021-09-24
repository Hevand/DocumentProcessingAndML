using System;

namespace Common.Model
{
    public class ProcessingSummary
    {        
        public string Id { get; set; }

        public DocumentType Type { get; set; }

        public string Name { get; set; }

        public DateTime? RequestedOn { get; set; }
        public DateTime? CompletedOn { get; set; }

        public ProcessingStatus Status
        {
            get; set;
        }
    }
}
