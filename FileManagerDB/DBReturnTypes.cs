using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDBManager
{
    public class GetFileMetadataType : IEquatable<GetFileMetadataType>
    {
        public int ID { get; set; }
        public int PathID { get; set; }
        public string Path { get; set; }
        public string Filename { get; set; }
        public string Altname { get; set; }
        public int FileTypeID { get; set; }
        public string FileType { get; set; }
        public string Hash { get; set; }

        public bool Equals(GetFileMetadataType other)
        {
            if (other is null) return false;

            return ID == other.ID && PathID == other.PathID
                && Path == other.Path && Filename == other.Filename 
                && Altname == other.Altname && FileTypeID == other.FileTypeID
                && Hash == other.Hash;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as GetFileMetadataType);
        }
    }
}
