using System;

namespace Common.Model
{
    public class Attachment
    {
        public string Title { get; set; }
        public string ContainerName { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public DateTime UploadedOn { get; set; }
        public int Order { get; set; }
    }
}
