using System.Diagnostics;

namespace cs_codexlongtest
{
    public class Docker
    {
        private const string DataDir = "hostdatadir";

        public void ClearDataDir()
        {
            if (Directory.Exists(DataDir))
            {
                Utils.Log("Remove old datadir...");
                Directory.Delete(DataDir, true);
            }
        }

        public void StartCodex()
        {
            Process.Start("docker-compose", "up -d");
            Timing.DockerImagePullDelay();
        }

        public void FetchDockerLogs()
        {
            Process.Start("docker-compose", "logs > docker_Utils.Logs.txt");
        }
    }
}
