namespace Avina.Extensions
{
    public static class StringHelper
    {
        public static bool IsNullEmptyOrWhitespace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }
    }
}