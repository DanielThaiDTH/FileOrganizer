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
    ///     A search filter for seraching files. Can compose multiple filters to build
    ///     complex queries. 
    ///     
    ///     Base state for Or and Not settings are false.
    /// </summary>
    public class FileSearchFilter : ICloneable
    {
        int _ID;
        int pathID;
        string path;
        string fullname;
        string filename;
        string altname;
        int filetypeID;
        string fileType;
        string hash;
        long size;
        bool usingSize;
        bool isSizeLesser;
        List<int> tagIDs;
        List<string> tagNames;
        bool tagFilterAnd;
        List<int> excludeTagIDs;
        List<string> excludeTagNames;
        bool excludeTagFilterAnd;
        Func<List<GetFileMetadataType>, List<GetFileMetadataType>> customFilter;
        List<FileSearchFilter> subFilters;

        public bool PathFilterExact { get; private set; }
        public bool FullnameFilterExact { get; private set; }
        public bool FilenameFilterExact { get; private set; }
        public bool AltnameFilterExact { get; private set; }
        public bool FileTypeFilterExact { get; private set; }
        public bool HashFilterExact { get; private set; }
        public bool TagFilterExact { get; private set; }

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
                    || UsingSize || UsingTags || UsingExcludeTags);
            }
        }
        public bool IsCustomOnly
        {
            get
            {
                return UsingCustomFilter && !(UsingID || UsingPathID || UsingPath || UsingFullname
                    || UsingFilename || UsingAltname || UsingFileTypeID
                    || UsingFileType || UsingHash || UsingSize || UsingTags);
            }
        }
        public bool IsOr { get; set; } = false;
        public bool IsNot { get; set; } = false;
        public bool IsBaseFilter { get { return subFilters is null || subFilters.Count == 0; } }

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
            PathFilterExact = true;
            fullname = null;
            FullnameFilterExact = true;
            filename = null;
            FilenameFilterExact = true;
            altname = null;
            AltnameFilterExact = true;
            filetypeID = int.MinValue;
            fileType = null;
            FileTypeFilterExact = true;
            hash = null;
            HashFilterExact = true;
            size = 0;
            usingSize = false;
            isSizeLesser = true;
            tagIDs = null;
            tagNames = null;
            tagFilterAnd = false;
            excludeTagIDs = null;
            excludeTagNames = null;
            excludeTagFilterAnd = false;
            TagFilterExact = false;
            IsOr = false;
            IsNot = false;
            if (subFilters != null) subFilters.Clear();

            customFilter = null;

            return this;
        }

        public FileSearchFilter SetOr(bool val)
        {
            IsOr = val;
            return this;
        }

        public FileSearchFilter SetNot(bool val)
        {
            IsNot = val;
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
            PathFilterExact = exact;
            return this;
        }

        public FileSearchFilter SetFullnameFilter(string name, bool exact = true)
        {
            fullname = name;
            FullnameFilterExact = exact;
            return this;
        }

        public FileSearchFilter SetFilenameFilter(string name, bool exact = true)
        {
            filename = name;
            FilenameFilterExact = exact;
            return this;
        }

        public FileSearchFilter SetAltnameFilter(string name, bool exact = true)
        {
            altname = name;
            AltnameFilterExact = exact;
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
            FileTypeFilterExact = exact;
            return this;
        }

        public FileSearchFilter SetHashFilter(string hash, bool exact = true)
        {
            this.hash = hash;
            HashFilterExact = exact;
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
        ///     Filters by tag ids. Second parameter is to set if the 
        ///     filter will be for files including all the given tags or just one.
        ///     Defaults to file with any in the list.
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
        ///     Filters by tag ids. Second parameter is to set if the 
        ///     filter will be for files including all the given tags or just one. 
        ///     Can match tag name exactly or inexactly.
        ///     Defaults to file with any in the list.
        /// </summary>
        /// <param name="tagNames"></param>
        /// <param name="usingAnd"></param>
        /// <param name="isExact"></param>
        /// <returns></returns>
        public FileSearchFilter SetTagFilter(List<string> tagNames, bool isExact = true, bool usingAnd = false)
        {
            tagIDs = null;
            tagFilterAnd = usingAnd;
            TagFilterExact = isExact;
            this.tagNames = tagNames;
            return this;
        }

        /// <summary>
        ///     Filters by tags a file does not have. Second parameter is to set if the 
        ///     filter will be for files excluding all the given tags or just one. Can 
        ///     exlude tags exactly or inexactly.
        ///     Defaults to file excluding any in the list.
        /// </summary>
        /// <param name="tagNames"></param>
        /// <param name="usingAnd"></param>
        /// <param name="isExact"></param>
        /// <returns></returns>
        public FileSearchFilter SetExcludeTagFilter(List<string> tagNames, bool isExact = true, bool usingAnd = false)
        {
            excludeTagIDs = null;
            excludeTagFilterAnd = usingAnd;
            excludeTagNames = tagNames;
            TagFilterExact = isExact;
            return this;
        }

        /// <summary>
        ///     Filters by tag ids not used by a file. Second parameter is to set if the 
        ///     filter will be for files excluding all the given tags or just one.
        ///     Defaults to file with any in the list.
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

        public FileSearchFilter AddSubfilter(FileSearchFilter filter)
        {
            if (subFilters is null) subFilters = new List<FileSearchFilter>();
            subFilters.Add(filter);

            return this;
        }

        public FileSearchFilter ClearSubfilters()
        {
            subFilters.Clear();
            return this;
        }

        private string MakeGlobString(string query)
        {
            // | shall be a placeholder for [, : for ]
            query =  query.Replace("[","||:")
                            .Replace("]","|::")
                            .Replace("||:", "[[]")
                            .Replace("|::", "[]]")
                            .Replace("<!", "<^")
                            .Replace('<', '[')
                            .Replace('>', ']');

            if (query.StartsWith("^")) {
                query = query.Substring(1);
            } else {
                query = "*" + query;
            }

            if (query.EndsWith("$")) {
                query = query.Substring(0, query.Length - 1);
            } else {
                query = query + "*";
            }

            return query.ToLowerInvariant();
        }

        /// <summary>
        ///     Builds where arrays containing basic filters. 
        /// </summary>
        /// <param name="wheres"></param>
        /// <param name="whereValues"></param>
        private void BuildWhereArrays(ref List<string> wheres, ref List<object> whereValues)
        {
            if (UsingID) {
                wheres.Add("Files.ID = ?");
                whereValues.Add(ID);
            }
            if (UsingPathID) {
                wheres.Add("PathID = ?");
                whereValues.Add(PathID);
            }
            if (UsingFilename) {
                if (FilenameFilterExact) {
                    wheres.Add("Filename = ?");
                    whereValues.Add(Filename);
                } else {
                    wheres.Add("LOWER(Filename) GLOB ?");
                    whereValues.Add(MakeGlobString(Filename));
                }
            }
            if (UsingFullname) {
                if (FullnameFilterExact) {
                    wheres.Add("Path || '\\' || Filename = ?");
                    whereValues.Add(Fullname);
                } else {
                    wheres.Add("LOWER(Path || '\\' || Filename) GLOB ?");
                    whereValues.Add(MakeGlobString(Fullname));
                }
            }
            if (UsingAltname) {
                if (AltnameFilterExact) {
                    wheres.Add("Altname = ?");
                    whereValues.Add(Altname);
                } else {
                    wheres.Add("LOWER(Altname) GLOB ?");
                    whereValues.Add(MakeGlobString(Altname));
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
                    wheres.Add("LOWER(Hash) GLOB ?");
                    whereValues.Add(MakeGlobString(Hash));
                }
            }
            if (UsingPath) {
                if (PathFilterExact) {
                    wheres.Add("Path = ?");
                    whereValues.Add(Path);
                } else {
                    wheres.Add("LOWER(Path) GLOB ?");
                    whereValues.Add(MakeGlobString(Path));
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

        private void BuildTagWhereStatementPart(ref string statement, ref List<string> wheres, ref List<object> whereValues,
            ref bool initial, ref bool includeWhere)
        {
            if (UsingTags && TagIDs != null && TagIDs.Count > 0) {
                if (wheres.Count == 0 && initial) {
                    statement += includeWhere ? "WHERE " : " ";
                    initial = false;
                } else {
                    statement += (wheres.Count == 0 && IsOr) ? " OR " : " AND ";
                }
                if (wheres.Count == 0 && IsNot) statement += "NOT ";
                statement += "(";
                for (int i = 0; i < TagIDs.Count; i++) {
                    statement += "? IN (SELECT TagID FROM FileTagAssociations WHERE FileID=Files.ID)";
                    whereValues.Add(TagIDs[i]);
                    if (i + 1 < TagIDs.Count) {
                        statement += UsingTagAnd ? " AND " : " OR ";
                    }
                }
                statement += ")";
            } else if (UsingTags && TagNames != null && TagNames.Count > 0) {
                if (wheres.Count == 0 && initial) {
                    statement += includeWhere ? "WHERE " : " ";
                    initial = false;
                } else {
                    statement += (wheres.Count == 0 && IsOr) ? " OR " : " AND ";
                }
                if (wheres.Count == 0 && IsNot) statement += "NOT ";
                statement += "(";
                for (int i = 0; i < TagNames.Count; i++) {
                    if (TagFilterExact) {
                        statement += "UPPER(?) IN (SELECT UPPER(Tags.Name) FROM " +
                            "FileTagAssociations JOIN Tags ON TagID=Tags.ID WHERE FileID=Files.ID)";
                        whereValues.Add(TagNames[i]);
                    } else {
                        statement += "(SELECT COUNT(*) FROM FileTagAssociations " +
                            $"JOIN Tags ON TagID=Tags.ID WHERE FileID=Files.ID AND LOWER(Tags.Name) GLOB ?) > 0";
                        whereValues.Add(MakeGlobString(TagNames[i]));
                    }
                    if (i + 1 < TagNames.Count) {
                        statement += UsingTagAnd ? " AND " : " OR ";
                    }
                }
                statement += ")";
            }

            if (UsingExcludeTags && ExcludeTagIDs != null && ExcludeTagIDs.Count > 0) {
                if (wheres.Count == 0 && initial) {
                    statement += includeWhere ? "WHERE " : " ";
                } else {
                    statement += (wheres.Count == 0 && IsOr && !UsingTags) ? " OR " : " AND ";
                }
                if (IsNot && !UsingTags && wheres.Count == 0) statement += "NOT ";
                statement += "(";
                for (int i = 0; i < ExcludeTagIDs.Count; i++) {
                    statement += "? NOT IN (SELECT TagID FROM FileTagAssociations WHERE FileID=Files.ID)";
                    whereValues.Add(ExcludeTagIDs[i]);
                    if (i + 1 < ExcludeTagIDs.Count) {
                        statement += UsingExcludeTagAnd ? " AND " : " OR ";
                    }
                }
                statement += ")";
            } else if (UsingExcludeTags && ExcludeTagNames != null && ExcludeTagNames.Count > 0) {
                if (wheres.Count == 0 && initial) {
                    statement += includeWhere ? "WHERE " : " ";
                } else {
                    statement += (wheres.Count == 0 && IsOr && !UsingTags) ? " OR " : " AND ";
                }
                if (IsNot && !UsingTags && wheres.Count == 0) statement += "NOT ";
                statement += "(";
                for (int i = 0; i < ExcludeTagNames.Count; i++) {
                    if (TagFilterExact) {
                        statement += "UPPER(?) NOT IN (SELECT UPPER(Tags.Name) FROM " +
                            "FileTagAssociations JOIN Tags ON TagID=Tags.ID WHERE FileID=Files.ID)";
                        whereValues.Add(ExcludeTagNames[i]);
                    } else {
                        statement += "(SELECT COUNT(*) FROM FileTagAssociations " +
                            "JOIN Tags ON TagID=Tags.ID WHERE FileID=Files.ID AND LOWER(Tags.Name) GLOB ?) = 0";
                        whereValues.Add(MakeGlobString(ExcludeTagNames[i]));
                    }
                    if (i + 1 < ExcludeTagNames.Count) {
                        statement += UsingExcludeTagAnd ? " AND " : " OR ";
                    }
                }
                statement += ")";
            }
        }

        /// <summary>
        ///     Builds the WHERE part of an SQL statement. Will require a string with the SELECT ... FROM Files 
        ///     JOIN ... to be created before hand. An object List must also be created beforehand.
        /// </summary>
        /// <param name="statement"></param>
        /// <param name="whereValues"></param>
        /// <param name="initial"></param>
        /// <param name="includeWhere"></param>
        public void BuildWhereStatementPart(ref string statement, ref List<object> whereValues, 
            bool initial=true, bool includeWhere=true)
        {
            if (IsBaseFilter) {
                List<string> wheres = new List<string>();
                BuildWhereArrays(ref wheres, ref whereValues);

                for (int i = 0; i < wheres.Count; i++) {
                    if (i == 0 && initial) {
                        statement += includeWhere ? "WHERE " : " ";
                    } else {
                        statement += (IsOr && i == 0) ? " OR " : " AND ";
                    }

                    if (i == 0) {
                        if (IsNot) statement += "NOT ";
                        statement += "(";
                    }

                    statement += wheres[i];
                }

                BuildTagWhereStatementPart(ref statement, ref wheres, ref whereValues, ref initial, ref includeWhere);
                
                if (wheres.Count > 0) statement += ")";
            } else {
                if (initial) {
                    statement += includeWhere ? "WHERE " : " ";
                } else {
                    statement += IsOr ? " OR " : " AND ";
                }
                statement += "(";
                bool start = true;
                foreach (var f in subFilters) {
                    f.BuildWhereStatementPart(ref statement, ref whereValues, start, false);
                    start = false;
                }

                statement += ")";
            }

        }

        /// <summary>
        ///     Builds a filter from metadata object. Created value not created 
        ///     and size filter defaults to greater than or equal to. 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static FileSearchFilter FromMetadata(FileMetadata data)
        {
            FileSearchFilter filter = new FileSearchFilter();

            if (data.UsingFilename) filter.SetFilenameFilter(data.Filename);
            if (data.UsingAltname) filter.SetAltnameFilter(data.Altname);
            if (data.UsingFileType) filter.SetFileTypeFilter(data.FileType);
            if (data.UsingFileTypeID) filter.SetFileTypeIDFilter(data.FileTypeID);
            if (data.UsingHash) filter.SetHashFilter(data.Hash);
            if (data.UsingPath) filter.SetPathFilter(data.Path);
            if (data.UsingPathID) filter.SetPathIDFilter(data.PathID);
            if (data.UsingSize) filter.SetSizeFilter(data.Size, false);

            return filter;
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
            newFilter.PathFilterExact = PathFilterExact;
            newFilter.pathID = pathID;
            newFilter.hash = hash;
            newFilter.HashFilterExact = HashFilterExact;
            newFilter.altname = altname;
            newFilter.AltnameFilterExact = AltnameFilterExact;
            newFilter.filename = filename;
            newFilter.FilenameFilterExact = FilenameFilterExact;
            newFilter.fullname = fullname;
            newFilter.FullnameFilterExact = FullnameFilterExact;
            newFilter.filetypeID = filetypeID;
            newFilter.fileType = fileType;
            newFilter.FileTypeFilterExact = FileTypeFilterExact;
            newFilter.customFilter = customFilter;
            newFilter.subFilters = subFilters;
            newFilter.IsOr = IsOr;
            newFilter.IsNot = IsNot;
            newFilter.tagFilterAnd = tagFilterAnd;
            newFilter.TagFilterExact = TagFilterExact;
            newFilter.tagIDs = tagIDs;
            newFilter.tagNames = tagNames;

            return newFilter;
        }
    }
}
