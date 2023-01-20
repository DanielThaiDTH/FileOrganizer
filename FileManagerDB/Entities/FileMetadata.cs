using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDBManager.Entities
{
    [Table("Files")]
    public class FileMetadata
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int ID { get; set; }

        [Indexed]
        [Column("PathID")]
        public int PathID { get; set; }

        [Unique]
        [Column("Fullname")]
        public string Fullname { get; set; }

        [Column("Filename")]
        public string Filename { get; set; }

        [Column("AltName")]
        public string AltName { get; set; }

        [Indexed]
        [Column("FileType")]
        public int FileTypeID { get; set; }

        [Column("Hash")]
        public string Hash { get; set; }
    }
}
