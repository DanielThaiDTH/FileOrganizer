using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDBManager.Entities
{
    //[Table("TagCategories")]
    class TagCategory
    {
        public static string TableName = "TagCategories";
        public static Dictionary<string, string> Columns
            = new Dictionary<string, string>()
            {
                { "ID", "INTEGER PRIMARY KEY" },
                { "Name", "TEXT UNIQUE" }
            };
        //[PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        //[Unique]
        //[Column("Name")]
        public string Name { get; set; }
    }
}
