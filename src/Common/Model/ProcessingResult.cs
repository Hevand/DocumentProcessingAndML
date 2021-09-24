using System;

namespace Common.Model
{
    public class ProcessingResult : ProcessingBase
    {
        public DateTime StartedOn { get; set; }

        public DateTime CompletedOn { get; set; }
    }
}
