using System.Globalization;

namespace cs_codexlongtest
{
    public static class Utils
    {
        public static void Log(string s)
        {
            var msg = "[" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture) + "] " + s;
            Console.WriteLine(msg);
            File.AppendAllLines("TestLog.txt", new[] { msg });
        }

        public static void Sleep(TimeSpan span)
        {
            Thread.Sleep(span);
        }

        public static T Wait<T>(Task<T> task)
        {
            task.Wait();
            return task.Result;
        }

        public static bool AreEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) { Log("len not equal"); return false; }
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
}
