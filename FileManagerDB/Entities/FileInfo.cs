using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerDB.Entities
{
    [Table("Files")]
    public class FileInfo
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int ID { get; set; }

        [Indexed]
        [Column("PathID")]
        public int PathID { get; set; }

        [Indexed]
        [Column("Filename")]
        public string Filename { get; set; }

        [Column("AltName")]
        public string AltName { get; set; }

        [Column("FileType")]
        public string FileType { get; set; }

        [Column("Hash")]
        public string Hash { get; set; }
    }
}
