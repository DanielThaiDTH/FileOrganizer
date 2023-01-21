using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDBManager.Entities
{
    public class FileCollectionAssociation
    {
        [PrimaryKey]
        [Column("CollectionID")]
        public int CollectionID { get; set; }

        [PrimaryKey]
        [Column("FileID")]
        public int FileID { get; set; }

        [Column("Position")]
        public int Position { get; set; }

    }
}
