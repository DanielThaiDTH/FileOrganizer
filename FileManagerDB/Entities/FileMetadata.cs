using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDBManager.Entities
{
    //[Table("Files")]
    public class FileMetadata
    {
        //[PrimaryKey, AutoIncrement]
        //[Column("ID")]
        public int ID { get; set; }

        //[Indexed]
        //[Column("PathID")]
        public int PathID { get; set; }

        //[Unique]
        //[Column("Fullname")]
        public string Fullname { get; set; }

        //[Column("Filename")]
        public string Filename { get; set; }

        //[Column("AltName")]
        public string AltName { get; set; }

        //[Indexed]
        //[Column("FileType")]
        public int FileTypeID { get; set; }

        //[Column("Hash")]
        public string Hash { get; set; }
    }

    public class FileSearchFilter
    {
        int _ID;
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

        public bool UsingID { get { return _ID >= 0; } }
        public bool UsingPathID { get { return pathID >= 0; } }
        public bool UsingPath { get { return path != null; } }
        public bool UsingFullname { get { return fullname != null; } }
        public bool UsingFilename { get { return filename != null; } }
        public bool UsingAltname { get { return altname != null; } }
        public bool UsingFileTypeID { get { return filetypeID >= 0; } }
        public bool UsingFileType {  get { return fileType != null; } }
        public bool UsingHash { get { return hash != null; } }

        public int ID { get { return _ID; } }
        public int PathID { get { return pathID; } }
        public string Path { get { return path; } }
        public string Fullname { get { return fullname; } }
        public string Filename { get { return filename; } }
        public string Altname { get { return altname; } }
        public int FileTypeID { get { return filetypeID; } }
        public string FileType { get { return fileType; } }
        public string Hash { get { return hash; } }

        public FileSearchFilter() {
            _ID = int.MinValue;
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
            _ID = id;
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

    public class FileMetadataUpdateObj
    {
        public int ID { get; set; } = int.MinValue;
        public string Path { get; set; } = null;
        public string Filename { get; set; } = null;
        public string Altname { get; set; } = null;
        public string FileType { get; set; } = null;
        public string Hash { get; set; } = null;

        public override string ToString()
        {
            string str = "ID: " + ID.ToString();
            str += "\nPath: ";
            if (Path != null) str += Path;
            str += "\nFilename: ";
            if (Filename != null) str += Filename;
            str += "\nAltname: ";
            if (Altname != null) str += Altname;
            str += "\nFileType: ";
            if (FileType != null) str += FileType;
            str += "\nHash: ";
            if (Hash != null) str += Hash;

            return str;
        }
    }
}
