﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDBManager.Entities
{
    //[Table("FileCollectionAssociations")]
    public class FileCollectionAssociation
    {
        public static string TableName = "FileCollectionAssociations";
        public static Dictionary<string, string> Columns
            = new Dictionary<string, string>()
            {
                { "CollectionID", "INTEGER REFERENCES Collections (ID) ON DELETE CASCADE" },
                { "FileID", "INTEGER REFERENCES Files (ID) ON DELETE CASCADE" },
                { "Position", "INTEGER" }
            };
        public static string Constraint = 
            "PRIMARY KEY (CollectionID, FileID), " +
            "UNIQUE (CollectionID, Position) ON CONFLICT IGNORE";

        //[PrimaryKey]
        //[Column("CollectionID")]
        public int CollectionID { get; set; }

        //[PrimaryKey]
        //[Column("FileID")]
        public int FileID { get; set; }

        //[Column("Position")]
        public int Position { get; set; }

    }
}
