using Common.Model;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public interface IDocumentRepository
    {
        Task<Attachment> UploadFile(string engagementId, string originalFileName, Stream content);
        Task<Stream> DownloadFile(Attachment attachment);
        Uri GetSASUri(Attachment a);
    }
}