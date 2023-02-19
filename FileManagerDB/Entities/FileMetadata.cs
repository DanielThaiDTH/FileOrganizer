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
        public static string TableName = "Files";
        public static Dictionary<string, string> Columns
            = new Dictionary<string, string>(){
                { "ID", "INTEGER PRIMARY KEY" },
                { "PathID", "INTEGER REFERENCES FilePaths (ID) ON DELETE RESTRICT" },
                { "Filename", "TEXT" },
                { "Altname", "TEXT" },
                { "FileTypeID", "INTEGER REFERENCES FileTypes (ID) ON DELETE RESTRICT" },
                { "Hash", "TEXT" },
                { "Size", "INTEGER DEFAULT 0" },
                { "Created", "INTEGER DEFAULT 0" },
                { "Modified", "INTEGER DEFAULT 0" }
            };
        public static string Constraint = "UNIQUE (PathID, Filename) ON CONFLICT IGNORE";

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
        public long Size { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }

    /// <summary>
    ///     A search filter for seraching files.
    /// </summary>
    public class FileSearchFilter : ICloneable
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
        long size;
        bool usingSizeFilter;
        bool isSizeLesser;
        DateTime created;
        DateTime modified;
        Func<List<GetFileMetadataType>, List<GetFileMetadataType>> customFilter;

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
        public bool UsingSizeFilter { get { return usingSizeFilter; } }
        public bool IsSizeLesser { get { return isSizeLesser; } }
        public bool UsingCustomFilter { get { return customFilter != null; } }
        public bool IsEmpty { get 
            {
                return !(UsingID || UsingPathID || UsingPath || UsingFullname 
                    || UsingFilename || UsingAltname || UsingFileTypeID 
                    || UsingFileType || UsingHash || UsingCustomFilter
                    || UsingSizeFilter);
            } }

        public int ID { get { return _ID; } }
        public int PathID { get { return pathID; } }
        public string Path { get { return path; } }
        public string Fullname { get { return fullname; } }
        public string Filename { get { return filename; } }
        public string Altname { get { return altname; } }
        public int FileTypeID { get { return filetypeID; } }
        public string FileType { get { return fileType; } }
        public string Hash { get { return hash; } }
        public long Size { get { return size; } }
        public DateTime Created { get { return created; } }
        public DateTime Modfied { get { return modified; } }
        public Func<List<GetFileMetadataType>, List<GetFileMetadataType>> CustomFilter { get { return customFilter; } }

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
            size = 0;
            usingSizeFilter = false;
            isSizeLesser = true;

            customFilter = null;
        }

        public FileSearchFilter Reset()
        {
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
            size = 0;
            usingSizeFilter = false;
            isSizeLesser = true;

            customFilter = null;

            return this;
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

        public FileSearchFilter SetPathFilter(string path, bool exact = true)
        {
            this.path = path;
            pathExact = exact;
            return this;
        }

        public FileSearchFilter SetFullnameFilter(string name, bool exact = true)
        {
            fullname = name;
            fullnameExact = exact;
            return this;
        }

        public FileSearchFilter SetFilenameFilter(string name, bool exact = true)
        {
            filename = name;
            filenameExact = exact;
            return this;
        }

        public FileSearchFilter SetAltnameFilter(string name, bool exact = true)
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

        public FileSearchFilter SetFileTypeFilter(string filetype, bool exact = true)
        {
            fileType = filetype;
            fileTypeExact = exact;
            return this;
        }

        public FileSearchFilter SetHashFilter(string hash, bool exact = true)
        {
            this.hash = hash;
            hashExact = exact;
            return this;
        }

        /// <summary>
        ///     Only supports simple greater/lesser or equal than to size. 
        ///     For more compelx queries, use a custome filter.
        /// </summary>
        /// <param name="size"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public FileSearchFilter SetSizeFilter(long size, bool isLesserThan)
        {
            this.size = size;
            isSizeLesser = isLesserThan;
            usingSizeFilter = true;
            return this;
        }

        /// <summary>
        ///     Sets a client defined filter that filters lists of file metadata result types.
        ///     The filter must use attributes that are part of the file metadata.
        ///     The custom filter is applied after the filtered SQL query is applied.
        /// </summary>
        /// <param name="customFilter"></param>
        /// <returns></returns>
        public FileSearchFilter SetCustom(Func<List<GetFileMetadataType>, List<GetFileMetadataType>> customFilter)
        {
            this.customFilter = customFilter;
            return this;
        }

        public override string ToString()
        {
            string str = "ID: ";
            if (UsingID) str += ID.ToString();
            str += "\nPath: ";
            if (UsingPath) str += Path;
            str += "\nPathID: ";
            if (UsingPathID) str += PathID.ToString();
            str += "\nFilename: ";
            if (UsingFilename) str += Filename;
            str += "\nAltname: ";
            if (UsingAltname) str += Altname;
            str += "\nFullname";
            if (UsingFullname) str += Fullname;
            str += "\nFileType: ";
            if (UsingFileType) str += FileType;
            str += "\nFileTypeID";
            if (UsingFileTypeID) str += FileTypeID.ToString();
            str += "\nHash: ";
            if (UsingHash) str += Hash;

            if (UsingCustomFilter) str += "\nUsing custom filter";

            return str;
        }

        public object Clone()
        {
            FileSearchFilter newFilter = new FileSearchFilter();
            newFilter.path = path;
            newFilter.pathExact = pathExact;
            newFilter.pathID = pathID;
            newFilter.hash = hash;
            newFilter.hashExact = hashExact;
            newFilter.altname = altname;
            newFilter.altnameExact = altnameExact;
            newFilter.filename = filename;
            newFilter.filenameExact = filenameExact;
            newFilter.fullname = fullname;
            newFilter.fullnameExact = fullnameExact;
            newFilter.filetypeID = filetypeID;
            newFilter.fileType = fileType;
            newFilter.fileTypeExact = fileTypeExact;
            newFilter.customFilter = customFilter;

            return newFilter;
        }
    }
}
