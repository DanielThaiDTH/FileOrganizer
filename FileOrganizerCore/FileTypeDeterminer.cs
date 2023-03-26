using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileOrganizerCore
{
    public class FileTypeDeterminer
    {
        Dictionary<string, string> extTypeMap;

        public FileTypeDeterminer()
        {
            extTypeMap = new Dictionary<string, string>()
            {
                { "zip", "archive" }, { "7z", "archive" }, { "rar", "archive" },
                { "txt", "text" }, { "log", "text" },
                { "json", "json" },
                { "xml", "ml" }, { "html", "ml" }, { "htm", "ml" },
                { "md", "markup" },
                { "c", "source" }, { "h", "source" }, { "cpp", "source" }, { "hpp", "source" },
                { "java", "source" }, { "cs", "source" }, { "js", "source" }, { "py", "source" },
                { "jpg", "image" }, { "jpeg", "image" }, { "png", "image" }, { "gif", "image" },
                { "webp", "image" },
                { "webm", "video" }, { "mp4", "video" }, { "mkv", "video" }, { "avi", "video" },
                { "pdf", "document" }, { "epub", "document" },
                { "exe", "executable" },
                { "mp3", "audio" }, { "midi", "audio" }, { "wav", "audio" }
            };

        }

        /// <summary>
        ///     Returns a file type from an extension. If not known, returns "unknown".
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public string FromExt(string ext)
        {
            ext = ext.ToLowerInvariant();
            if (extTypeMap.ContainsKey(ext)) {
                return extTypeMap[ext];
            } else if (ext.StartsWith(".") && extTypeMap.ContainsKey(ext.Substring(1))) {
                return extTypeMap[ext.Substring(1)];
            } else {
                return "unknown";
            }
        }

        /// <summary>
        ///     Returns a file type using the extension of the file.
        ///     If not known, returns "unknown".
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string FromFilename(string filename)
        {
            string ext = Path.GetExtension(filename).ToLowerInvariant();
            return FromExt(ext);
        }
    }
}
