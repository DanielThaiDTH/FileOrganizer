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
        #region TableDef
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
                //{ "Modified", "INTEGER DEFAULT 0" }
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
        public string Altname { get; set; }

        //[Indexed]
        //[Column("FileType")]
        public int FileTypeID { get; set; }

        //[Column("Hash")]
        public string Hash { get; set; }
        public long Size { get; set; }
        public DateTime Created { get; set; }
        //public DateTime Modified { get; set; }
        #endregion

        public string Path { get; private set; }
        public string FileType { get; private set; }

        public bool UsingID { get; private set; } = false;
        public bool UsingPathID { get; private set; } = false;
        public bool UsingPath { get; private set; } = false;
        public bool UsingFilename { get; private set; } = false;
        public bool UsingHash { get; private set; } = false;
        public bool UsingAltname { get; private set; } = false;
        public bool UsingSize { get; private set; } = false;
        public bool UsingCreated { get; private set; } = false;
        public bool UsingFileType { get; private set; } = false;
        public bool UsingFileTypeID { get; private set; } = false;

        public bool IsEmpty { get {
                return !(UsingID || UsingPathID || UsingPath
                    || UsingFilename || UsingAltname || UsingFileTypeID
                    || UsingFileType || UsingHash || UsingCreated
                    || UsingSize);
            } }

        public FileMetadata SetFilename(string filename)
        {
            UsingFilename = true;
            Filename = filename;
            return this;
        }

        public FileMetadata SetID(int id)
        {
            UsingID = true;
            this.ID = id;
            return this;
        }

        public FileMetadata SetPathID(int id)
        {
            UsingPathID = true;
            PathID = id;
            return this;
        }

        public FileMetadata SetPath(string path)
        {
            UsingPath = true;
            Path = path;
            return this;
        }

        public FileMetadata SetHash(string hash)
        {
            UsingHash = true;
            Hash = hash;
            return this;
        }

        public FileMetadata SetAltname(string altname)
        {
            UsingAltname = true;
            Altname = altname;
            return this;
        }

        public FileMetadata SetSize(long size)
        {
            UsingSize = true;
            Size = size;
            return this;
        }

        public FileMetadata SetCreated(DateTime created)
        {
            UsingCreated = true;
            Created = created;
            return this;
        }

        public FileMetadata SetFileType(string fileType)
        {
            UsingFileType = true;
            FileType = fileType;
            return this;
        }

        public FileMetadata SetFileTypeID(int id)
        {
            UsingFileTypeID = true;
            FileTypeID = id;
            return this;
        }

        /// <summary>
        /// Prints information in metadata
        /// </summary>
        /// <returns></returns>
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
            str += "\nFileType: ";
            if (UsingFileType) str += FileType;
            str += "\nFileTypeID";
            if (UsingFileTypeID) str += FileTypeID.ToString();
            str += "\nHash: ";
            if (UsingHash) str += Hash;
            str += "\nCreated: ";
            if (UsingCreated) str += Created.ToString("yyyy-MM-dd");

            return str;
        }
    }

    
}
