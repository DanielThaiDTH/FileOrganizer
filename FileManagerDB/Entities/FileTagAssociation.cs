using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDBManager.Entities
{
    //[Table("FileTagAssociations")]
    class FileTagAssociation
    {
        //[PrimaryKey]
        //[Column("FileID")]
        public int FileID { get; set; }

        //[PrimaryKey]
        //[Column("TagID")]
        public int TagID { get; set; }
    }
}
