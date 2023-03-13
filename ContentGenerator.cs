namespace cs_codexlongtest
{
    public class ContentGenerator
    {
        public const string ContentFolder = "TestContent";

        private readonly Random random = new Random();

        public ContentGenerator()
        {
            if (!Directory.Exists(ContentFolder)) Directory.CreateDirectory(ContentFolder);
        }

        public TestContent Generate()
        {
            var result = new TestContent();
            result.Filename = Guid.NewGuid().ToString() + "_test.bin";

            var length = GetRandomSize();
            var bytes = new byte[length];
            random.NextBytes(bytes);

            File.WriteAllBytes(Path.Combine(ContentFolder, result.Filename), bytes);

            return result;
        }

        private int GetRandomSize()
        {
            return (1024 * 1024 * 10) + random.Next(1024 * 1024 * 100);
        }
    }

    public class TestContent
    {
        public string Filename { get; set; } = string.Empty;
        public string ContentId { get; set; } = string.Empty;

        public string FilePath()
        {
            return Path.Combine(ContentGenerator.ContentFolder, Filename);
        }

        public byte[] GetBytes()
        {
            return File.ReadAllBytes(FilePath());
        }

        public void Delete()
        {
            File.Delete(FilePath());
        }
    }
}
