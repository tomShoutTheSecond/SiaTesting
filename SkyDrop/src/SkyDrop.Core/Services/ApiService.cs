using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkyDrop.Core.Services
{
    public class ApiService : IApiService
    {
        public async Task<string> UploadFile(string filename, byte[] file)
        {
            var url = "https://siasky.net/skynet/skyfile";

            var httpClient = new HttpClient();
            var form = new MultipartFormDataContent();

            form.Add(new ByteArrayContent(file), "file", filename);
            var response = await httpClient.PostAsync(url, form);

            response.EnsureSuccessStatusCode();
            httpClient.Dispose();

            return response.Content.ReadAsStringAsync().Result;

        }
    }

    public interface IApiService
    {
        Task<string> UploadFile(string filename, byte[] file);
    }
}
