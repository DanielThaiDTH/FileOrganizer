using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileOrganizerCore.JSONOutput
{
    public enum FileSizeUnit
    {
        Gigabytes = 1000000000,
        Megabytes = 1000000,
        Kilobytes = 1000,
        Bytes = 1
    }

    public class JSONOptions
    {
        public bool IncludeFullName { get; set; }
        public bool IncludePath { get; set; }
        public bool IncludeHash { get; set; }
        public bool RemoveSeparators { get; set; }
        public bool IncludeFileSize { get; set; }
        public FileSizeUnit SizeUnit { get; set; }
        public bool IncludeCreatedDate { get; set; }
        public bool IncludeTags { get; set; }
    }
}
