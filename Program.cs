using System.Globalization;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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

public static class Program
{
    private static void Log(string s)
    {
        var msg = "[" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture) + "] " + s;
        Console.WriteLine(msg);
        File.AppendAllLines("TestLog.txt", new []{msg});
    }

    private const string DataDir = "hostdatadir";
    private const string TestContent = "Starmakers.png";

    private static string contentId;
    private static int successfulDownloadCounter;

    public static void Main(string[] args)
    {
        Log("Codex long-run test.");

        if (Directory.Exists(DataDir))
        {
            Log("Remove old datadir...");
            Directory.Delete(DataDir, true);
        }

        Log("Starting codex...");
        Process.Start("docker-compose", "up -d");

        Log("Fetching Debug/info endpoint...");
        var debugInfo = HttpGet<CodexDebugResponse>("debug/info");
        if (string.IsNullOrEmpty(debugInfo.spr))
        {
            Log("Received invalid response on debug/info endpoint");
            return;
        }
        Log("Success, Codex online.");

        while (true)
        {
            Sleep(TimeSpan.FromSeconds(10));

            if (string.IsNullOrEmpty(contentId))
            {
                UploadTestContent();
            }
            else
            {
                DownloadAndVerifyTestContent();
                Sleep(TimeSpan.FromSeconds(10));
            }
        }
    }
    
    private static void UploadTestContent(int retryCounter = 0)
    {
        successfulDownloadCounter = 0;

        try
        {
            var url = "http://127.0.0.1:8080/api/codex/v1/upload";
            using var client = new HttpClient();

            var byteData = File.ReadAllBytes(TestContent);
            using var content = new ByteArrayContent(byteData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var response = Wait(client.PostAsync(url, content));

            contentId = Wait(response.Content.ReadAsStringAsync());
            Log("Uploaded test content yielded contentId: " + contentId);
        }
        catch (Exception exception)
        {
            if (retryCounter > 5)
            {
                Log("HttpPost upload failed after retries with exception: " + exception);
                FetchDockerLogs();
                throw;
            }
            else
            {
                Sleep(TimeSpan.FromMinutes(30));
                UploadTestContent(retryCounter + 1);
            }
        }
    }

    private static void DownloadAndVerifyTestContent()
    {
        var bytes = HttpGetBytes("download/" + contentId);
        if (bytes == null)
        {
            // Download failed. Clear content Id. try again next time.
            Log("Download failed after " + successfulDownloadCounter + " successful downloads.");
            contentId = "";
            return;
        }

        var expectedBytes = File.ReadAllBytes(TestContent);

        if (!AreEqual(bytes, expectedBytes))
        {
            Log("Download returned different/unexpected bytes after " + successfulDownloadCounter + " successful downloads.");
            contentId = "";
        }
        else
        {
            successfulDownloadCounter++;
        }
    }

    private static void FetchDockerLogs()
    {
        Process.Start("docker-compose", "logs > docker_logs.txt");
    }

    private static void Sleep(TimeSpan span)
    {
        Thread.Sleep(span);
    }

    private static byte[] HttpGetBytes(string endpoint, int retryCounter = 0)
    {
        try
        {
            using var client = new HttpClient();
            var url = "http://127.0.0.1:8080/api/codex/v1/" + endpoint;
            var result = Wait(client.GetAsync(url));
            return Wait(result.Content.ReadAsByteArrayAsync());
        }
        catch (Exception exception)
        {
            if (retryCounter > 5)
            {
                Log("HttpGetBytes failed after retries with exception: " + exception);
                return null;
            }
            else
            {
                Sleep(TimeSpan.FromSeconds(10));
                return HttpGetBytes(endpoint, retryCounter + 1);
            }
        }
    }
      
    private static T HttpGet<T>(string endpoint, int retryCounter = 0)
    {
        try
        {
            using var client = new HttpClient();
            var url = "http://127.0.0.1:8080/api/codex/v1/" + endpoint;
            var result = Wait(client.GetAsync(url));
            var json = Wait(result.Content.ReadAsStringAsync());
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception exception)
        {
            if (retryCounter > 5)
            {
                Log("HttpGet failed after retries with exception: " + exception);
                FetchDockerLogs();
                throw;
            }
            else
            {
                Sleep(TimeSpan.FromSeconds(10));
                return HttpGet<T>(endpoint, retryCounter + 1);
            }
        }
    }

    private static T Wait<T>(Task<T> task)
    {
        task.Wait();
        return task.Result;
    }

    private static bool AreEqual(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) { Log ("len not equal"); return false; }
        for (var i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
            { 
                Log("not equal at " + i + " = " + a[i] + " vs " + b[i]);
                return false;
            }
        }
        return true;
    }
}
