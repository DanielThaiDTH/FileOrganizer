﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerDB.Entities
{
    [Table("TagCategories")]
    class TagCategory
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Column("Name")]
        public string Name { get; set; }
    }
}
