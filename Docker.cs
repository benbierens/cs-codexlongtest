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
            Utils.Sleep(TimeSpan.FromSeconds(30));
        }

        public void FetchDockerLogs()
        {
            Process.Start("docker-compose", "Utils.Logs > docker_Utils.Logs.txt");
        }
    }
}
