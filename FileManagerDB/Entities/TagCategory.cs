﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDBManager.Entities
{
    //[Table("TagCategories")]
    /// <summary>
    ///     Name is always in lower case.
    /// </summary>
    class TagCategory
    {
        public static string TableName = "TagCategories";
        public static Dictionary<string, string> Columns
            = new Dictionary<string, string>()
            {
                { "ID", "INTEGER PRIMARY KEY" },
                { "Name", "TEXT UNIQUE ON CONFLICT IGNORE" }
            };
        //[PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        //[Unique]
        //[Column("Name")]
        public string Name { get; set; }
    }
}
