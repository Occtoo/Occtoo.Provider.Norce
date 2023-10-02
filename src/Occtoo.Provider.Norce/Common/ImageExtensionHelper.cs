namespace Occtoo.Provider.Norce.Common
{
    using System.Collections.Generic;

    public static class ImageExtensionHelper
    {
        private static readonly Dictionary<string, string> MimeToExtensionMap = new Dictionary<string, string>
    {
        { "image/jpeg", ".jpg" },
        { "image/png", ".png" },
        { "image/gif", ".gif" },
        { "image/bmp", ".bmp" },
    };

        public static string GetImageExtensionFromMime(string mimeType)
        {
            if (!string.IsNullOrEmpty(mimeType))
            {
                if (MimeToExtensionMap.TryGetValue(mimeType, out string extension))
                {
                    return extension;
                }

                return ".jpg";
            }

            return ".jpg";
        }
    }

}
