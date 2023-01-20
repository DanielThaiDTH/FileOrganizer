using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerDB.Entities
{
    [Table("Collections")]
    public class FileCollection
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int ID { get; set; }

        [Unique]
        [Column("Name")]
        public string Name { get; set; }


    }
}
