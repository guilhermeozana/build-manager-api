using Marelli.Domain.Dtos;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Marelli.Test.Utils.Factories
{
    public class FileFactory
    {
        public static MultipartFormDataContent GetMultipartFormDataContent()
        {
            var formFile = GetFormFile();
            var formData = new MultipartFormDataContent();
            var fileContent = new StreamContent(formFile.OpenReadStream());

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(formFile.ContentType);

            formData.Add(fileContent, "file", formFile.FileName);

            return formData;
        }

        public static IFormFile GetFormFile()
        {
            var fileName = "test.txt";
            var contentType = "text/plain";
            var content = Encoding.UTF8.GetBytes("This is a test file content.");
            var stream = new MemoryStream(content);

            var formFile = new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType,
                ContentDisposition = $"form-data; name=\"file\"; filename=\"{fileName}\""
            };

            return formFile;
        }

        public static MultipartFormDataContent GetEmptyMultipartFormDataContent()
        {
            var formFile = GetEmptyFormFile();
            var formData = new MultipartFormDataContent();
            var fileContent = new StreamContent(formFile.OpenReadStream());

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(formFile.ContentType);

            formData.Add(fileContent, "file", formFile.FileName);

            return formData;
        }

        public static IFormFile GetEmptyFormFile()
        {
            var fileName = "test.txt";
            var contentType = "text/plain";
            var content = Encoding.UTF8.GetBytes("");
            var stream = new MemoryStream(content);

            var formFile = new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType,
                ContentDisposition = $"form-data; name=\"file\"; filename=\"{fileName}\""
            };

            return formFile;
        }

        public static FileResponse GetFileResponse()
        {
            return new FileResponse
            {
                Name = "test",
                MimeType = "application/zip",
                Url = "http://test.com",
            };
        }
    }
}
