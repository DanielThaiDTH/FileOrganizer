using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerDB.Entities
{
    [Table("FileType")]
    public class FileType
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int ID { get; set; }

        [Column("Name")]
        public string Name { get; set; }
    }
}
