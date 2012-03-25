namespace Avina.Extensions
{
    using System.IO;

    public static class StreamExtensions
    {
        public static string AsString(this Stream stream)
        {
            using(StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}