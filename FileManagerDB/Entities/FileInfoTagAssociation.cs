﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerDB.Entities
{
    [Table("FileTagAssociations")]
    class FileInfoTagAssociation
    {
        [PrimaryKey]
        [Column("FileID")]
        public int FileID { get; set; }

        [PrimaryKey]
        [Column("TagID")]
        public int TagID { get; set; }
    }
}
