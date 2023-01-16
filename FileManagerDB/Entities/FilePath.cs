using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerDB.Entities
{
    [Table("Filepaths")]
    class FilePath
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int ID { get; set; }

        [Column("Path")]
        public string PathString { get; set; }
    }
}
