using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDBManager.Entities
{
    /// <summary>
    ///     Helper methods for handling dates, wrapper for null Datetime equivalents.
    /// </summary>
    public class DateTimeOptional
    {
        public static readonly long unixEpoch = 116444736000000000; //already reduced
        public static readonly long ticksPerSec = 10000000;

        public DateTimeOptional(DateTime date)
        {
            Date = date;
        }

        public static long ToUnixTime(DateTime dt)
        {
            return (dt.Date.ToFileTime() - unixEpoch) / ticksPerSec;
        }

        public static long ToUnixTime(DateTimeOptional dto)
        {
            return ToUnixTime(dto.Date);
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            long windowsTime = (unixTime * ticksPerSec) + unixEpoch;
            var dt = DateTime.FromFileTime(windowsTime);

            return dt;
        }

        public static DateTime RoundToUnixPrecision(DateTime dt)
        {
            var roundedDt = FromUnixTime(ToUnixTime(dt));

            return roundedDt;
        }

        public DateTime Date { get; set; }

    }


    /// <summary>
    ///     A search filter for seraching files.
    /// </summary>
    public class FileSearchFilter : ICloneable
    {
        int _ID;
        int pathID;
        string path;
        bool pathExact;
        string fullname;
        bool fullnameExact;
        string filename;
        bool filenameExact;
        string altname;
        bool altnameExact;
        int filetypeID;
        string fileType;
        bool fileTypeExact;
        string hash;
        bool hashExact;
        long size;
        bool usingSize;
        bool isSizeLesser;
        DateTime created;
        List<int> tagIDs;
        List<string> tagNames;
        bool tagFilterAnd;
        List<int> excludeTagIDs;
        List<string> excludeTagNames;
        bool excludeTagFilterAnd;
        Func<List<GetFileMetadataType>, List<GetFileMetadataType>> customFilter;

        public bool PathFilterExact { get { return pathExact; } }
        public bool FullnameFilterExact { get { return fullnameExact; } }
        public bool FilenameFilterExact { get { return filenameExact; } }
        public bool AltnameFilterExact { get { return altnameExact; } }
        public bool FileTypeFilterExact { get { return fileTypeExact; } }
        public bool HashFilterExact { get { return hashExact; } }

        public bool UsingID { get { return _ID >= 0; } }
        public bool UsingPathID { get { return pathID >= 0; } }
        public bool UsingPath { get { return path != null; } }
        public bool UsingFullname { get { return fullname != null; } }
        public bool UsingFilename { get { return filename != null; } }
        public bool UsingAltname { get { return altname != null; } }
        public bool UsingFileTypeID { get { return filetypeID >= 0; } }
        public bool UsingFileType { get { return fileType != null; } }
        public bool UsingHash { get { return hash != null; } }
        public bool UsingSize { get { return usingSize; } }
        public bool IsSizeLesser { get { return isSizeLesser; } }
        public bool UsingTags
        {
            get
            {
                return !((tagIDs is null || tagIDs.Count == 0) && (tagNames is null || tagNames.Count == 0));
            }
        }
        public bool UsingExcludeTags
        {
            get
            {
                return !((excludeTagIDs is null || excludeTagIDs.Count == 0)
                    && (excludeTagNames is null || excludeTagNames.Count == 0));
            }
        }
        public bool UsingCustomFilter { get { return customFilter != null; } }
        public bool IsEmpty
        {
            get
            {
                return !(UsingID || UsingPathID || UsingPath || UsingFullname
                    || UsingFilename || UsingAltname || UsingFileTypeID
                    || UsingFileType || UsingHash || UsingCustomFilter
                    || UsingSize);
            }
        }

        public int ID { get { return _ID; } }
        public int PathID { get { return pathID; } }
        public string Path { get { return path; } }
        public string Fullname { get { return fullname; } }
        public string Filename { get { return filename; } }
        public string Altname { get { return altname; } }
        public int FileTypeID { get { return filetypeID; } }
        public string FileType { get { return fileType; } }
        public string Hash { get { return hash; } }
        public long Size { get { return size; } }
        public DateTime Created { get { return created; } }
        public List<int> TagIDs { get { return tagIDs; } }
        public List<string> TagNames { get { return tagNames; } }
        public bool UsingTagAnd { get { return tagFilterAnd; } }
        public List<int> ExcludeTagIDs { get { return excludeTagIDs; } }
        public List<string> ExcludeTagNames { get { return excludeTagNames; } }
        public bool UsingExcludeTagAnd { get { return excludeTagFilterAnd; } }
        public Func<List<GetFileMetadataType>, List<GetFileMetadataType>> CustomFilter { get { return customFilter; } }

        public FileSearchFilter()
        {
            Reset();
        }

        public FileSearchFilter Reset()
        {
            _ID = int.MinValue;
            pathID = int.MinValue;
            path = null;
            pathExact = true;
            fullname = null;
            fullnameExact = true;
            filename = null;
            filenameExact = true;
            altname = null;
            altnameExact = true;
            filetypeID = int.MinValue;
            fileType = null;
            fileTypeExact = true;
            hash = null;
            hashExact = true;
            size = 0;
            usingSize = false;
            isSizeLesser = true;
            tagIDs = null;
            tagNames = null;
            tagFilterAnd = false;
            excludeTagIDs = null;
            excludeTagNames = null;
            excludeTagFilterAnd = false;

            customFilter = null;

            return this;
        }

        public FileSearchFilter SetIDFilter(int id)
        {
            _ID = id;
            return this;
        }

        public FileSearchFilter SetPathIDFilter(int id)
        {
            pathID = id;
            return this;
        }

        public FileSearchFilter SetPathFilter(string path, bool exact = true)
        {
            this.path = path;
            pathExact = exact;
            return this;
        }

        public FileSearchFilter SetFullnameFilter(string name, bool exact = true)
        {
            fullname = name;
            fullnameExact = exact;
            return this;
        }

        public FileSearchFilter SetFilenameFilter(string name, bool exact = true)
        {
            filename = name;
            filenameExact = exact;
            return this;
        }

        public FileSearchFilter SetAltnameFilter(string name, bool exact = true)
        {
            altname = name;
            altnameExact = exact;
            return this;
        }

        public FileSearchFilter SetFileTypeIDFilter(int id)
        {
            filetypeID = id;
            return this;
        }

        public FileSearchFilter SetFileTypeFilter(string filetype, bool exact = true)
        {
            fileType = filetype;
            fileTypeExact = exact;
            return this;
        }

        public FileSearchFilter SetHashFilter(string hash, bool exact = true)
        {
            this.hash = hash;
            hashExact = exact;
            return this;
        }

        /// <summary>
        ///     Only supports simple greater/lesser or equal than to size. 
        ///     For more compelx queries, use a custome filter.
        /// </summary>
        /// <param name="size"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public FileSearchFilter SetSizeFilter(long size, bool isLesserThan)
        {
            this.size = size;
            isSizeLesser = isLesserThan;
            usingSize = true;
            return this;
        }

        /// <summary>
        ///     Filters by tag ids. Second parameters is to set if the 
        ///     filter will be for files including all the given tags or just one.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="usingAnd"></param>
        /// <returns></returns>
        public FileSearchFilter SetTagFilter(List<int> ids, bool usingAnd = false)
        {
            tagNames = null;
            tagFilterAnd = usingAnd;
            tagIDs = ids;
            return this;
        }

        /// <summary>
        ///     Filters by tag ids. Second parameters is to set if the 
        ///     filter will be for files including all the given tags or just one.
        /// </summary>
        /// <param name="tagNames"></param>
        /// <param name="usingAnd"></param>
        /// <returns></returns>
        public FileSearchFilter SetTagFilter(List<string> tagNames, bool usingAnd = false)
        {
            tagIDs = null;
            tagFilterAnd = usingAnd;
            this.tagNames = tagNames;
            return this;
        }

        /// <summary>
        ///     Filters by tags a file does not have. Second parameter is to set if the 
        ///     filter will be for files including all the given tags or just one.
        /// </summary>
        /// <param name="tagNames"></param>
        /// <param name="usingAnd"></param>
        /// <returns></returns>
        public FileSearchFilter SetExcludeTagFilter(List<string> tagNames, bool usingAnd = false)
        {
            excludeTagIDs = null;
            excludeTagFilterAnd = usingAnd;
            excludeTagNames = tagNames;
            return this;
        }

        /// <summary>
        ///     Filters by tag ids not used by a file. Second parameter is to set if the 
        ///     filter will be for files including all the given tags or just one.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="usingAnd"></param>
        /// <returns></returns>
        public FileSearchFilter SetExcludeTagFilter(List<int> ids, bool usingAnd = false)
        {
            excludeTagNames = null;
            excludeTagFilterAnd = usingAnd;
            excludeTagIDs = ids;
            return this;
        }

        /// <summary>
        ///     Sets a client defined filter that filters lists of file metadata result types.
        ///     The filter must use attributes that are part of the file metadata.
        ///     The custom filter is applied after the filtered SQL query is applied.
        /// </summary>
        /// <param name="customFilter"></param>
        /// <returns></returns>
        public FileSearchFilter SetCustom(Func<List<GetFileMetadataType>, List<GetFileMetadataType>> customFilter)
        {
            this.customFilter = customFilter;
            return this;
        }

        /// <summary>
        ///     Builds where arrays containing basic filters. 
        /// </summary>
        /// <param name="wheres"></param>
        /// <param name="whereValues"></param>
        private void BuildWhereArrays(ref List<string> wheres, ref List<object> whereValues)
        {
            if (UsingID) {
                wheres.Add("Files.ID = ? ");
                whereValues.Add(ID);
            }
            if (UsingPathID) {
                wheres.Add("PathID = ? ");
                whereValues.Add(PathID);
            }
            if (UsingFilename) {
                if (FilenameFilterExact) {
                    wheres.Add("Filename = ?");
                    whereValues.Add(Filename);
                } else {
                    wheres.Add("Filename LIKE ?");
                    whereValues.Add("%" + Filename + "%");
                }
            }
            if (UsingFullname) {
                if (FullnameFilterExact) {
                    wheres.Add("Path || '\\' || Filename = ?");
                    whereValues.Add(Fullname);
                } else {
                    wheres.Add("Path || '\\' || Filename LIKE ?");
                    whereValues.Add("%" + Fullname + "%");
                }
            }
            if (UsingAltname) {
                if (AltnameFilterExact) {
                    wheres.Add("Altname = ?");
                    whereValues.Add(Altname);
                } else {
                    wheres.Add("Altname LIKE ?");
                    whereValues.Add("%" + Altname + "%");
                }
            }
            if (UsingFileTypeID) {
                wheres.Add("FileTypeID = ?");
                whereValues.Add(FileTypeID);
            }
            if (UsingHash) {
                if (HashFilterExact) {
                    wheres.Add("Hash = ?");
                    whereValues.Add(Hash);
                } else {
                    wheres.Add("Hash LIKE ?");
                    whereValues.Add("%" + Hash + "%");
                }
            }
            if (UsingPath) {
                if (PathFilterExact) {
                    wheres.Add("Path = ?");
                    whereValues.Add(Path);
                } else {
                    wheres.Add("Path LIKE ?");
                    whereValues.Add("%" + Path + "%");
                }
            }
            if (UsingFileType) {
                if (FileTypeFilterExact) {
                    wheres.Add("Name = ?");
                    whereValues.Add(FileType);
                } else {
                    wheres.Add("Name LIKE ?");
                    whereValues.Add("%" + FileType + "%");
                }
            }
            if (UsingSize) {
                string comp = IsSizeLesser ? "<=" : ">=";
                wheres.Add("Size " + comp + " ?");
                whereValues.Add(Size);
            }
        }

        public void BuildWhereStatementPart(ref string statement, ref List<object> whereValues, bool initial=true)
        {
            List<string> wheres = new List<string>();
            BuildWhereArrays(ref wheres, ref whereValues);

            for (int i = 0; i < wheres.Count; i++) {
                if (i == 0 && initial) {
                    statement += "WHERE ";
                } else {
                    statement += " AND ";
                }

                statement += wheres[i];
            }

            if (UsingTags && TagIDs != null && TagIDs.Count > 0) {
                statement += " AND (";
                for (int i = 0; i < TagIDs.Count; i++) {
                    statement += "? IN (SELECT TagID FROM FileTagAssociations WHERE FileID=Files.ID)";
                    whereValues.Add(TagIDs[i]);
                    if (i + 1 < TagIDs.Count) {
                        statement += UsingTagAnd ? " AND " : " OR ";
                    }
                }
                statement += ")";
            } else if (UsingTags && TagNames != null && TagNames.Count > 0) {
                statement += " AND (";
                for (int i = 0; i < TagNames.Count; i++) {
                    statement += "? IN (SELECT Tags.Name FROM " +
                        "FileTagAssociations JOIN Tags ON TagID=Tags.ID WHERE FileID=Files.ID)";
                    whereValues.Add(TagNames[i]);
                    if (i + 1 < TagNames.Count) {
                        statement += UsingTagAnd ? " AND " : " OR ";
                    }
                }
                statement += ")";
            }

            if (UsingExcludeTags && ExcludeTagIDs != null && ExcludeTagIDs.Count > 0) {
                statement += " AND (";
                for (int i = 0; i < ExcludeTagIDs.Count; i++) {
                    statement += "? NOT IN (SELECT TagID FROM FileTagAssociations WHERE FileID=Files.ID)";
                    whereValues.Add(ExcludeTagIDs[i]);
                    if (i + 1 < ExcludeTagIDs.Count) {
                        statement += UsingExcludeTagAnd ? " AND " : " OR ";
                    }
                }
                statement += ")";
            } else if (UsingExcludeTags && ExcludeTagNames != null && ExcludeTagNames.Count > 0) {
                statement += " AND (";
                for (int i = 0; i < ExcludeTagNames.Count; i++) {
                    statement += "? NOT IN (SELECT Tags.Name FROM " +
                        "FileTagAssociations JOIN Tags ON TagID=Tags.ID WHERE FileID=Files.ID)";
                    whereValues.Add(ExcludeTagNames[i]);
                    if (i + 1 < ExcludeTagNames.Count) {
                        statement += UsingExcludeTagAnd ? " AND " : " OR ";
                    }
                }
                statement += ")";
            }

        }

        public override string ToString()
        {
            string str = "ID: ";
            if (UsingID) str += ID.ToString();
            str += "\nPath: ";
            if (UsingPath) str += Path;
            str += "\nPathID: ";
            if (UsingPathID) str += PathID.ToString();
            str += "\nFilename: ";
            if (UsingFilename) str += Filename;
            str += "\nAltname: ";
            if (UsingAltname) str += Altname;
            str += "\nFullname";
            if (UsingFullname) str += Fullname;
            str += "\nFileType: ";
            if (UsingFileType) str += FileType;
            str += "\nFileTypeID";
            if (UsingFileTypeID) str += FileTypeID.ToString();
            str += "\nHash: ";
            if (UsingHash) str += Hash;

            if (UsingCustomFilter) str += "\nUsing custom filter";

            return str;
        }

        public object Clone()
        {
            FileSearchFilter newFilter = new FileSearchFilter();
            newFilter.path = path;
            newFilter.pathExact = pathExact;
            newFilter.pathID = pathID;
            newFilter.hash = hash;
            newFilter.hashExact = hashExact;
            newFilter.altname = altname;
            newFilter.altnameExact = altnameExact;
            newFilter.filename = filename;
            newFilter.filenameExact = filenameExact;
            newFilter.fullname = fullname;
            newFilter.fullnameExact = fullnameExact;
            newFilter.filetypeID = filetypeID;
            newFilter.fileType = fileType;
            newFilter.fileTypeExact = fileTypeExact;
            newFilter.customFilter = customFilter;

            return newFilter;
        }
    }
}
