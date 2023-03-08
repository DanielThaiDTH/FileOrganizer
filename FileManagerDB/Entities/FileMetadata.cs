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
        public string AltName { get; set; }

        //[Indexed]
        //[Column("FileType")]
        public int FileTypeID { get; set; }

        //[Column("Hash")]
        public string Hash { get; set; }
        public long Size { get; set; }
        public DateTime Created { get; set; }
        //public DateTime Modified { get; set; }
    }

    
}
