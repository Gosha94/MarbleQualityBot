using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using MarbleQualityBot.Core.Config;
using System.Net;
using System.Net.Http.Headers;
using System.Web;

namespace MarbleQualityBot.Core.Integrations.Clients
{
    public class DetectionApi : IDetectionApi
    {
        private readonly DetectionApiSettings _detectionSettings;
        private readonly string BASE_URL;
        private readonly string _baseUploadUrl;

        public DetectionApi(IOptions<DetectionApiSettings> detectionSettings)
        {
            _detectionSettings = detectionSettings.Value;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var BASE_URL = new Uri(_detectionSettings.Url);
            string MODEL_ENDPOINT = $"{_detectionSettings.DatasetId}/{_detectionSettings.VersionId}";

            _baseUploadUrl =
                    BASE_URL + MODEL_ENDPOINT
                    + "?api_key=" + _detectionSettings.ApiKey
                    + "&confidence=" + _detectionSettings.Confidence;
        }

        public async Task<string> DetectFromUrl(string imageURL)
        {
            var uploadUrl =
                _baseUploadUrl
                + "&image=" + HttpUtility.UrlEncode(imageURL);

            var request = WebRequest.Create(uploadUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = 0;

            using (var response = await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public async Task<string> DetectFromPath(string filePath)
        {
            using (var httpClient = new HttpClient())
            using (var content = new MultipartFormDataContent())
            {
                content.Headers.ContentType.MediaType = "multipart/form-data";

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(filePath));
                    content.Add(fileContent, "file", Path.GetFileName(filePath));

                    var response = await httpClient.PostAsync(_baseUploadUrl, content);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public string GetContentType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out string contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}