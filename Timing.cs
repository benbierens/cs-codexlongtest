namespace cs_codexlongtest
{
    public static class Timing
    {
        public static TimeSpan HttpCallTimeout()
        {
            return TimeSpan.FromMinutes(10);
        }

        public static void RetryDelay()
        {
            Utils.Sleep(TimeSpan.FromSeconds(30));
        }

        public static void DockerImagePullDelay()
        {
            Utils.Sleep(TimeSpan.FromMinutes(3));
        }

        public static void TestLoopDelay()
        {
            Utils.Sleep(TimeSpan.FromMinutes(10));
        }
    }
}
