using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogAPI.CustomExceptions;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.Services
{
    public interface IFilesService
    {
        Task<string> SaveFile(IFormFile file);
        FileContentResult GetFile(string fileName);
        void DeleteFile(string fileName);
    }

    public class FilesService : IFilesService
    {
        private readonly IConfiguration _configuration;
        public FilesService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> SaveFile(IFormFile file)
        {
            string filePath = "./Uploads/";

            if (file.Length < 1)
            {
                throw new BadHttpRequestException("The file field was informed, but not a file present.");
            }

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            string newName = "";
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            while (true)
            {
                newName = new string(Enumerable.Repeat(chars, 25).Select(s => s[random.Next(s.Length)]).ToArray());
                newName += Path.GetExtension(file.FileName);
                filePath = Path.Combine(filePath, newName);

                if (!File.Exists(filePath)) break;
            }

            using (FileStream fileStream = File.Create(filePath))
            {
                await file.CopyToAsync(fileStream);
                fileStream.Flush();
                return newName;
            }
        }

        public FileContentResult GetFile(string fileName)
        {
            string path = Path.Combine("./Uploads/", fileName);

            if (File.Exists(path))
            {
                FileContentResult result = new FileContentResult(System.IO.File.ReadAllBytes(path), "application/octet-stream")
                {
                    FileDownloadName = Path.GetFileName(path)
                };

                return result;
            }

            throw new NotFoundException("File not found! " + path);
        }

        public void DeleteFile(string fileName)
        {
            var filePath = Path.Combine("./Uploads/", fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else
            {
                throw new NotFoundException("File not found! " + filePath);
            }
        }

    }

}