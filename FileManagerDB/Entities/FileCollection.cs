using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDBManager.Entities
{
    //[Table("Collections")]
    public class FileCollection
    {
        public static string TableName = "Collections";
        public static Dictionary<string, string> Columns
            = new Dictionary<string, string>()
            {
                { "ID", "INTEGER PRIMARY KEY" },
                { "Name", "TEXT UNIQUE ON CONFLICT IGNORE" },
            };

        //[PrimaryKey, AutoIncrement]
        //[Column("ID")]
        public int ID { get; set; }

        //[Unique]
        //[Column("Name")]
        public string Name { get; set; }
    }
}
