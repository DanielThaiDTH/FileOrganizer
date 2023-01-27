using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDBManager.Entities
{
    //[Table("FileTagAssociations")]
    class FileTagAssociation
    {
        public static string TableName = "FileTagAssociations";
        public static Dictionary<string, string> Columns
            = new Dictionary<string, string>()
            {
                { "FileID", "INTEGER REFERENCES Files (ID) ON DELETE CASCADE" },
                { "TagID", "INTEGER REFERENCES Tags (ID) ON DELETE CASCADE" }
            };
        public static string Constraint = "PRIMARY KEY (FileID, TagID)";

        //[PrimaryKey]
        //[Column("FileID")]
        public int FileID { get; set; }

        //[PrimaryKey]
        //[Column("TagID")]
        public int TagID { get; set; }
    }
}
