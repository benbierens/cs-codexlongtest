using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace cs_codexlongtest
{
    public class CodexNode
    {
        private readonly int port;

        public CodexNode(int port)
        {
            this.port = port;
        }

        public CodexDebugResponse GetDebugInfo()
        {
            return HttpGet<CodexDebugResponse>("debug/info");
        }

        public string UploadFile(string filename, int retryCounter = 0)
        {
            try
            {
                var url = $"http://127.0.0.1:{port}/api/codex/v1/upload";
                using var client = GetClient();

                var byteData = File.ReadAllBytes(filename);
                using var content = new ByteArrayContent(byteData);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response = Utils.Wait(client.PostAsync(url, content));

                var contentId = Utils.Wait(response.Content.ReadAsStringAsync());
                Utils.Log("Uploaded test content yielded contentId: " + contentId);
                return contentId;
            }
            catch (Exception exception)
            {
                if (retryCounter > 5)
                {
                    Utils.Log("HttpPost upload failed after retries with exception: " + exception);
                    throw;
                }
                else
                {
                    Utils.Sleep(TimeSpan.FromMinutes(30));
                    return UploadFile(filename, retryCounter + 1);
                }
            }
        }

        public byte[]? DownloadContent(string contentId)
        {
            return HttpGetBytes("download/" + contentId);
        }

        private byte[]? HttpGetBytes(string endpoint, int retryCounter = 0)
        {
            try
            {
                using var client = GetClient();
                var url = $"http://127.0.0.1:{port}/api/codex/v1/" + endpoint;
                var result = Utils.Wait(client.GetAsync(url));
                return Utils.Wait(result.Content.ReadAsByteArrayAsync());
            }
            catch (Exception exception)
            {
                if (retryCounter > 5)
                {
                    Utils.Log("HttpGetBytes failed after retries with exception: " + exception);
                    return null;
                }
                else
                {
                    Utils.Sleep(TimeSpan.FromSeconds(10));
                    return HttpGetBytes(endpoint, retryCounter + 1);
                }
            }
        }

        private T HttpGet<T>(string endpoint, int retryCounter = 0)
        {
            try
            {
                using var client = GetClient();
                var url = $"http://127.0.0.1:{port}/api/codex/v1/" + endpoint;
                var result = Utils.Wait(client.GetAsync(url));
                var json = Utils.Wait(result.Content.ReadAsStringAsync());
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception exception)
            {
                if (retryCounter > 5)
                {
                    Utils.Log("HttpGet failed after retries with exception: " + exception);
                    throw;
                }
                else
                {
                    Utils.Sleep(TimeSpan.FromSeconds(10));
                    return HttpGet<T>(endpoint, retryCounter + 1);
                }
            }
        }

        private HttpClient GetClient()
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(3);
            return client;
        }
    }

    public class CodexDebugResponse
    {
        public string id { get; set; }
        public string[] addrs { get; set; }
        public string repo { get; set; }
        public string spr { get; set; }
        public CodexDebugVersionResponse codex { get; set; }
    }

    public class CodexDebugVersionResponse
    {
        public string version { get; set; }
        public string revision { get; set; }
    }
}
