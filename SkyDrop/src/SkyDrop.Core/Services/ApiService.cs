using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.Services
{
    public class ApiService : IApiService
    {
        public async Task<SkyFile> UploadFile(string filename, byte[] file)
        {
            var url = "https://siasky.net/skynet/skyfile";

            var httpClient = new HttpClient();
            var form = new MultipartFormDataContent();

            form.Add(new ByteArrayContent(file), "file", filename);
            var response = await httpClient.PostAsync(url, form);

            response.EnsureSuccessStatusCode();
            httpClient.Dispose();

            var responseString = await response.Content.ReadAsStringAsync();
            var skyfile = JsonConvert.DeserializeObject<SkyFile>(responseString);
            skyfile.Filename = filename;
            skyfile.Skylink = $"siasky.net/{skyfile.Skylink}";

            return skyfile;
        }
    }

    public interface IApiService
    {
        Task<SkyFile> UploadFile(string filename, byte[] file);
    }
}
