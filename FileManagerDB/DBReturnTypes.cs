﻿using FileDBManager.Entities;
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
        public long Size { get; set; }
        public DateTime Created { get; set; }
        public string Fullname { get { return System.IO.Path.Combine(Path, Filename); } }

        public bool Equals(GetFileMetadataType other)
        {
            if (other is null) return false;

            return ID == other.ID && PathID == other.PathID
                && Path == other.Path && Filename == other.Filename 
                && Altname == other.Altname && FileTypeID == other.FileTypeID
                && Hash == other.Hash && Size == other.Size && Created == other.Created;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as GetFileMetadataType);
        }

        public override int GetHashCode()
        {
            return ID;
        }
    }

    public class GetTagType : Tag, IEquatable<GetTagType>
    {
        public string Category { get; set; }

        public bool Equals(GetTagType other)
        {
            if (other == null) return false;

            return Name == other.Name && ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as GetTagType);
        }

        public override int GetHashCode()
        {
            return ID;
        }
    }

    public class GetTagCategoryType : IEquatable<GetTagCategoryType>
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Color { get; set; }

        public bool Equals(GetTagCategoryType other)
        {
            if (other == null) return false;

            return ID == other.ID && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as GetTagCategoryType);
        }

        public override int GetHashCode()
        {
            return ID;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class GetFileCollectionAssociationType
    {
        public int CollectionID { get; set; }
        public int FileID { get; set; }
        public int Position { get; set; }
    }

    public class GetCollectionType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<GetFileCollectionAssociationType> Files { get; set; }
    }
}
