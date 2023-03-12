namespace cs_codexlongtest
{
    public class SimpleUploadDownloadTest
    {
        private readonly ContentGenerator contentGenerator = new ContentGenerator();
        private readonly Docker docker = new Docker();
        private readonly CodexNode node = new CodexNode(8080);

        public void Run()
        {
            try
            {
                RunTest();
            }
            catch (Exception ex)
            {
                Utils.Log("Failed with exception: " + ex);
                docker.FetchDockerLogs();
            }
        }

        private void RunTest()
        { 
            Utils.Log("Codex long-run test.");
            docker.ClearDataDir();

            Utils.Log("Starting codex...");
            docker.StartCodex();

            Utils.Log("Fetching Debug/info endpoint...");
            var debugInfo = node.GetDebugInfo();
            if (string.IsNullOrEmpty(debugInfo.spr))
            {
                Utils.Log("Received invalid response on debug/info endpoint");
                return;
            }
            Utils.Log("Success, Codex online.");

            while (true)
            {
                var content = contentGenerator.Generate();
                var expectedBytes = content.GetBytes();
                var successfulDownloads = 0;

                Utils.Sleep(TimeSpan.FromSeconds(10));
                content.ContentId = node.UploadFile(content.FilePath());

                var success = true;
                while (success)
                {
                    Utils.Sleep(TimeSpan.FromSeconds(10));
                    var receivedBytes = node.DownloadContent(content.ContentId);

                    if (receivedBytes != null && Utils.AreEqual(receivedBytes, expectedBytes))
                    {
                        successfulDownloads++;
                    }
                    else
                    {
                        success = false;
                        Utils.Log($"Download failed after {successfulDownloads} successful downloads.");
                    }
                }
            }
        }
    }
}
