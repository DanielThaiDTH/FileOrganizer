using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDBManager.Entities
{
    //[Table("Tags")]
    public class Tag
    {
        public static string TableName = "Tags";


        //[PrimaryKey, AutoIncrement]
        //[Column("ID")]
        public int ID { get; set; }

        //[Unique]
        //[Column("Name")]
        public string Name { get; set; }

        //[Indexed]
        //[Column("CategoryID")]
        public int CategoryID { get; set; }
    }
}
