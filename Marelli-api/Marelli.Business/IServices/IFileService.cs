using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Marelli.Business.IServices
{
    public interface IFileService
    {
        public Task<BuildTableRow> UploadZip(IFormFile file, int userId, int projectId);

        public Task RenameZip(string oldDirectory, string oldKey, string newDirectory, string newKey);
        public Task<FileResponse> DownloadZip(int buildId);
        public Task RemoveZip(int userId, int projectId, int buildId);
        public Task<Baseline> UploadBaseline(IFormFile file, int projectId, string description);
        public Task<FileResponse> DownloadBaseline(int baselineId);
        public Task RemoveBaseline(int baselineId);
        public Task<FileResponse> DownloadLogs(int buildId);
    }
}
