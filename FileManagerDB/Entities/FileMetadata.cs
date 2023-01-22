using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDBManager.Entities
{
    [Table("Files")]
    public class FileMetadata
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int ID { get; set; }

        [Indexed]
        [Column("PathID")]
        public int PathID { get; set; }

        [Unique]
        [Column("Fullname")]
        public string Fullname { get; set; }

        [Column("Filename")]
        public string Filename { get; set; }

        [Column("AltName")]
        public string AltName { get; set; }

        [Indexed]
        [Column("FileType")]
        public int FileTypeID { get; set; }

        [Column("Hash")]
        public string Hash { get; set; }
    }

    public class FileSearchFilter
    {
        int ID;
        int pathID;
        string path;
        bool pathExact;
        string fullname;
        bool fullnameExact;
        string filename;
        bool filenameExact;
        string altname;
        bool altnameExact;
        int filetypeID;
        string fileType;
        bool fileTypeExact;
        string hash;
        bool hashExact;

        public bool PathFilterExact { get { return pathExact; } }
        public bool FullnameFilterExact { get { return fullnameExact; } }
        public bool FilenameFilterExact { get { return filenameExact; } }
        public bool AltnameFilterExact { get { return altnameExact; } }
        public bool FileTypeFilterExact { get { return fileTypeExact; } }
        public bool hashFilterExact { get { return hashExact; } }

        public bool UsingID { get { return ID >= 0; } }
        public bool UsingPathID { get { return pathID >= 0; } }
        public bool UsingPath { get { return path != null; } }
        public bool UsingFullname { get { return fullname != null; } }
        public bool UsingFilename { get { return filename != null; } }
        public bool UsingAltname { get { return altname != null; } }
        public bool UsingFileTypeID { get { return filetypeID >= 0; } }
        public bool UsingFileType {  get { return fileType != null; } }
        public bool UsingHash { get { return hash != null; } }

        public FileSearchFilter() {
            ID = int.MinValue;
            pathID = int.MinValue;
            path = null;
            pathExact = true;
            fullname = null;
            fullnameExact = true;
            filename = null;
            filenameExact = true;
            altname = null;
            altnameExact = true;
            filetypeID = int.MinValue;
            fileType = null;
            fileTypeExact = true;
            hash = null;
            hashExact = true;
        }

        public FileSearchFilter SetIDFilter(int id)
        {
            ID = id;
            return this;
        }

        public FileSearchFilter SetPathIDFilter(int id)
        {
            pathID = id;
            return this;
        }

        public FileSearchFilter SetPathFilter(string path, bool exact)
        {
            this.path = path;
            pathExact = exact;
            return this;
        }

        public FileSearchFilter SetFullnameFilter(string name, bool exact)
        {
            fullname = name;
            fullnameExact = exact;
            return this;
        }

        public FileSearchFilter SetFilenameFilter(string name, bool exact)
        {
            filename = name;
            filenameExact = exact;
            return this;
        }

        public FileSearchFilter SetAltnameFilter(string name, bool exact)
        {
            altname = name;
            altnameExact = exact;
            return this;
        }

        public FileSearchFilter SetFileTypeIDFilter(int id)
        {
            filetypeID = id;
            return this;
        }

        public FileSearchFilter SetFileTypeFilter(string filetype, bool exact)
        {
            fileType = filetype;
            fileTypeExact = exact;
            return this;
        }

        public FileSearchFilter SetHashFilter(string hash, bool exact)
        {
            this.hash = hash;
            hashExact = exact;
            return this;
        }
    }
}
