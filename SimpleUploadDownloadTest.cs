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

                Timing.TestLoopDelay();
                content.ContentId = node.UploadFile(content.FilePath());

                DownloadUntilFailed(expectedBytes, content);
                content.Delete();
            }
        }

        private void DownloadUntilFailed(byte[] expectedBytes, TestContent content)
        {
            var successfulDownloads = 0;

            while (true)
            {
                Timing.TestLoopDelay();
                var receivedBytes = node.DownloadContent(content.ContentId);

                if (receivedBytes != null && Utils.AreEqual(receivedBytes, expectedBytes))
                {
                    successfulDownloads++;
                }
                else
                {
                    Utils.Log($"Download failed after {successfulDownloads} successful downloads.");
                    return;
                }
            }
        }
    }
}
