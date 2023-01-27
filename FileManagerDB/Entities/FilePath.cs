using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDBManager.Entities
{
    //[Table("FilePaths")]
    class FilePath
    {
        public static string TableName = "FilePaths";
        public static Dictionary<string, string> Columns 
            = new Dictionary<string, string>(){
                { "ID", "INTEGER PRIMARY KEY" },
                { "Path", "TEXT UNIQUE" }
            };

        //[PrimaryKey, AutoIncrement]
        //[Column("ID")]
        public int ID { get; set; }

        //[Unique]
        //[Column("Path")]
        public string PathString { get; set; }
    }
}
