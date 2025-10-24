using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace CMCS.Services
{
    public interface IFileService
    {
        Task<string> SaveClaimFileAsync(string lecturerId, string fileName, Stream fileStream);
    }

    public class FileService : IFileService
    {
        private readonly string _root;
        public FileService(IWebHostEnvironment env)
        {
            _root = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(_root);
        }

        public async Task<string> SaveClaimFileAsync(string lecturerId, string fileName, Stream fileStream)
        {
            var folder = Path.Combine(_root, lecturerId ?? "anonymous");
            Directory.CreateDirectory(folder);
            var safe = Path.GetFileName(fileName);
            var path = Path.Combine(folder, $"{Guid.NewGuid()}_{safe}");
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fs);
            var idx = path.IndexOf("uploads");
            return idx >= 0 ? path.Substring(idx).Replace("\\", "/") : path;
        }
    }
}
