using System;
using System.IO;
using System.Reflection;
using System.Xml.XPath;
using Xunit;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.File;
using Serilog.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SQLiteConsole;
using FileDBManager.Entities;
using System.Data;
using System.Data.SQLite;

namespace FileDBManager.Test
{
    public static class TestLoader
    {
        public static string testConfigPath = @"..\..\TestData.xml";
        public static string GetNodeValue(string nodeName)
        {
            var xml = new XPathDocument(testConfigPath);
            var XMLNav = xml.CreateNavigator();
            var nodeList = XMLNav.Select("//" + nodeName);
            foreach (XPathNavigator n in nodeList) {
                return n.Value;
            }
            return null;
        }
    }
    
    public class TestFixture : IDisposable
    {
        public FileDBManagerClass db = null;
        public Microsoft.Extensions.Logging.ILogger logger;
        
        public TestFixture()
        {
            if (File.Exists(TestLoader.GetNodeValue("TestDB"))) {
                File.Delete(TestLoader.GetNodeValue("TestDB"));
            }
            string path = Path.Combine("logs", "log.log");
            if (!Directory.Exists("logs")) Directory.CreateDirectory("logs");
            Log.Logger = new LoggerConfiguration()
                //.MinimumLevel.Debug()
                .WriteTo.File(path,  rollingInterval: RollingInterval.Day)
                .WriteTo.Debug()
                .CreateLogger();
            logger = new SerilogLoggerFactory(Log.Logger).CreateLogger<IServiceProvider>();
            Log.Information(Path.GetFullPath(TestLoader.GetNodeValue("TestDB")));
            db = new FileDBManagerClass(TestLoader.GetNodeValue("TestDB"), logger);
        }

        public void Dispose()
        {
            Log.Information("Disposing");
            Log.CloseAndFlush();
            db.CloseConnection();
        }
    }
    
    [CollectionDefinition("DB")]
    public class DBCollection : ICollectionFixture<TestFixture>
    {

    }

    [Collection("DB")]
    public class Test
    {
        TestFixture fix;
        public Test(TestFixture fix)
        {
            this.fix = fix;
        }

        [Fact]
        public void AddFileForNewFileReturnsTrue()
        {
            Log.Information("TEST: AddFileForNewFileReturnsTrue");
            Assert.True(fix.db.AddFile(@"C:\Temp\file1.txt", "text", "aaa", "testfile"));
        }

        [Fact]
        public void AddFileHandlesSingleQuotes()
        {
            Log.Information("TEST: AddFileHandlesSingleQuotes");
            Assert.True(fix.db.AddFile(@"C:\Temp\file'.txt", "text", "aaa", "testfile"));
            List<GetFileMetadataType> results = fix.db.GetAllFileMetadata();
            Assert.True(results.Exists(r => r.Filename == "file'.txt"));
        }

        [Fact]
        public void AddFileCannotAddDuplicateFilepath()
        {
            Log.Information("TEST: AddFileCannotAddDuplicateFilepath");
            fix.db.AddFile(@"C:\Temp\dup.txt", "text", "aaa", "testfile");
            List<GetFileMetadataType> results = fix.db.GetAllFileMetadata();
            int oldCount = results.Count;
            Assert.False(fix.db.AddFile(@"C:\Temp\dup.txt", "log", "nnn", "dupfile"));
            results = fix.db.GetAllFileMetadata();
            Assert.Equal(oldCount, results.Count);
        }

        [Fact]
        public void GetAllFileMetadataReturnsAddedFiles()
        {
            Log.Information("TEST: GetAllFileMetadataReturnsAddedFiles");
            fix.db.AddFile(@"C:\Temp\file2.bmp", "image", "bbb", "testfile");
            fix.db.AddFile(@"C:\Temp\file3.mp3", "audio", "ccc", "testfile");
            List<GetFileMetadataType> results = fix.db.GetAllFileMetadata();
            Assert.True(results.Count >= 2);
            Assert.True(results.FindAll(r => r.Filename == "file2.bmp").Count == 1);
            Assert.True(results.FindAll(r => r.Filename == "file3.mp3").Count == 1);
            foreach (var res in results) {
                if (res.Filename == "file2.bmp") {
                    Assert.True(res.FileType == "image");
                    Assert.True(res.Hash == "bbb");
                    Assert.True(res.Altname == "testfile");
                } else if (res.Filename == "file3.bmp") {
                    Assert.True(res.FileType == "audio");
                    Assert.True(res.Hash == "ccc");
                    Assert.True(res.Altname == "testfile");
                }
            }
        }

        [Fact]
        public void GetFileMetadataReturnsCorrectDataWithID()
        {
            Log.Information("TEST: GetFileMetadataReturnsCorrectDataWithID");
            List<GetFileMetadataType> results = fix.db.GetAllFileMetadata();
            foreach (var res in results) {
                res.Equals(fix.db.GetFileMetadata(res.ID));
            }
        }

        [Fact]
        public void GetFileMetadataFilteredWithFilenamesReturnsCorrectResults()
        {
            Log.Information("TEST: GetFileMetadataFilteredWithFilenamesReturnsCorrectResults");
            fix.db.AddFile(@"C:\Temp\file_test1", "image", "testHash", "alt.bmp");
            fix.db.AddFile(@"C:\Temp\file_test2", "audio", "testHash2", "alt.mp3");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFilenameFilter("file_test1", true);
            List<GetFileMetadataType> results = fix.db.GetFileMetadataFiltered(filter);
            Assert.Single(results);
            Assert.Equal("file_test1", results[0].Filename);
            Assert.Equal("alt.bmp", results[0].Altname);
            Assert.Equal(@"C:\Temp", results[0].Path);
            Assert.Equal("image", results[0].FileType);
            filter.Reset().SetFilenameFilter("file_test", false);
            results = fix.db.GetFileMetadataFiltered(filter);
            Assert.Equal(2, results.Count);
            filter.Reset().SetFilenameFilter("cannot_be_found", false);
            results = fix.db.GetFileMetadataFiltered(filter);
            Assert.Empty(results);
        }

        [Fact]
        public void GetFileMetadataFilteredWithAltnamesReturnsCorrectResults()
        {
            Log.Information("TEST: GetFileMetadataFilteredWithAltnamesReturnsCorrectResults");
            fix.db.AddFile(@"C:\Temp\alt_test1", "text", "aaa", "altname1");
            fix.db.AddFile(@"C:\Temp\alt_test2", "text", "aaa", "altname2");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetAltnameFilter("altname1", true);
            Assert.Single(fix.db.GetFileMetadataFiltered(filter));
            filter.Reset().SetAltnameFilter("altname", false);
            Assert.Equal(2, fix.db.GetFileMetadataFiltered(filter).Count);
            filter.Reset().SetAltnameFilter("cannot_be_found", false);
            Assert.Empty(fix.db.GetFileMetadataFiltered(filter));
        }

        [Fact]
        public void GetFileMetadataFilteredWithPathReturnsCorrectResults()
        {
            Log.Information("TEST: GetFileMetadataFilteredWithPathReturnsCorrectResults");
            fix.db.AddFile(@"Z:\pathA\pathB\file", "text", "aaa", "testfile");
            fix.db.AddFile(@"Z:\pathA\pathC\file", "text", "aaa", "testfile");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetPathFilter(@"Z:\pathA\pathB", true);
            Assert.Single(fix.db.GetFileMetadataFiltered(filter));
            filter.Reset().SetPathFilter("Z:\\pathA\\path", false);
            Assert.Equal(2, fix.db.GetFileMetadataFiltered(filter).Count);
            filter.Reset().SetPathFilter("cannot_be_found", false);
            Assert.Empty(fix.db.GetFileMetadataFiltered(filter));
        }

        [Fact]
        public void GetFileMetadataFilteredWithHashReturnsCorrectResults()
        {
            Log.Information("TEST: GetFileMetadataFilteredWithHashReturnsCorrectResults");
            fix.db.AddFile(@"C:\Temp\hash_test_file1", "text", "000", "");
            fix.db.AddFile(@"C:\Temp\hash_test_file2", "text", "100", "");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetHashFilter("000", true);
            Assert.Single(fix.db.GetFileMetadataFiltered(filter));
            filter.Reset().SetHashFilter("00", false);
            Assert.Equal(2, fix.db.GetFileMetadataFiltered(filter).Count);
            filter.Reset().SetHashFilter("cannot_be_found", false);
            Assert.Empty(fix.db.GetFileMetadataFiltered(filter));
        }

        [Fact]
        public void GetFileMetadataWithFileTypeReturnsCorrectResults()
        {
            Log.Information("TEST: GetFileMetadataWithFileTypeReturnsCorrectResults");
            fix.db.AddFile(@"C:\Temp\type_file1", "type1", "aaa", "");
            fix.db.AddFile(@"C:\Temp\type_file2", "type2", "aaa", "");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFileTypeFilter("type1", true);
            Assert.Single(fix.db.GetFileMetadataFiltered(filter));
            filter.Reset().SetFileTypeFilter("type", false);
            Assert.Equal(2, fix.db.GetFileMetadataFiltered(filter).Count);
            filter.Reset().SetFileTypeFilter("cannot_be_found", false);
            Assert.Empty(fix.db.GetFileMetadataFiltered(filter));
        }

        [Fact]
        public void GetFileMetadataWithNonExactFullnameFilterWorks()
        {
            Log.Information("TEST: GetFileMetadataWithNonExactFullnameFilterWorks");
            fix.db.AddFile(@"S:\Temp\unique_file_inexact", "text", "a1a", "unique_alt");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFullnameFilter(@"\unique_file_in", false);
            Assert.Single(fix.db.GetFileMetadataFiltered(filter));
        }

        [Fact]
        public void GetFileMetadataWithAllFiltersWorks()
        {
            Log.Information("TEST: GetFileMetadataWithAllFiltersWorks");
            fix.db.AddFile(@"S:\Temp\unique_file", "text", "a1a", "unique_alt");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFilenameFilter("unique_file", true);
            var info = fix.db.GetFileMetadataFiltered(filter)[0];
            filter.Reset()
                .SetIDFilter(info.ID)
                .SetPathIDFilter(info.PathID)
                .SetFileTypeIDFilter(info.FileTypeID)
                .SetFullnameFilter(@"S:\Temp\unique_file", true)
                .SetFilenameFilter("unique_file", true)
                .SetAltnameFilter("unique_alt", true)
                .SetHashFilter("a1a", true)
                .SetFileTypeFilter("text", true)
                .SetPathFilter(@"S:\Temp", true);
            Assert.Single(fix.db.GetFileMetadataFiltered(filter));
        }

        [Fact]
        public void DeleteFileMetadataRemovesFile()
        {
            Log.Information("TEST: DeleteFileMetadataRemovesFile");
            fix.db.AddFile(@"C:\Temp\delete_file", "bin", "aaa");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFilenameFilter("delete_file", true);
            var info = fix.db.GetFileMetadataFiltered(filter)[0];
            var count = fix.db.GetAllFileMetadata().Count;
            Assert.True(fix.db.DeleteFileMetadata(info.ID));
            Assert.Equal(count - 1, fix.db.GetAllFileMetadata().Count);
        }

        [Fact]
        public void DeleteFileMetadataWithUnusedIDDoesNothing()
        {
            Log.Information("TEST: DeleteFileMetadataRemovesFile");
            var count = fix.db.GetAllFileMetadata().Count;
            Assert.False(fix.db.DeleteFileMetadata(-1));
            Assert.Equal(count, fix.db.GetAllFileMetadata().Count);
        }

        [Fact]
        public void UpdateFileMetadataChangesFile()
        {
            Log.Information("TEST: UpdateFileMetadataChangesFile");
            fix.db.AddFile(@"C:\Temp\file_to_update", "text", "aaa", "first_alt");
            FileSearchFilter updateValues = new FileSearchFilter();
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFilenameFilter("file_to_update", true);
            updateValues.SetFilenameFilter("updated_name")
                        .SetFileTypeFilter("bin")
                        .SetHashFilter("bbb")
                        .SetAltnameFilter("second_alt");
            Assert.Empty(fix.db.GetFileMetadataFiltered(updateValues));
            Assert.True(fix.db.UpdateFileMetadata(updateValues, filter));
            Assert.Single(fix.db.GetFileMetadataFiltered(updateValues));
        }

        [Fact]
        public void UpdateFileMetadataReturnsFalseUsingFilterWithNoMatch()
        {
            Log.Information("TEST: UpdateFileMetadataReturnsFalseUsingFilterWithNoMatch");
            FileSearchFilter updateValues = new FileSearchFilter();
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFilenameFilter("cannot_be_found", true);
            updateValues.SetFilenameFilter("unused");
            Assert.False(fix.db.UpdateFileMetadata(updateValues, filter));
        }

        [Fact]
        public void UpdateFileMetadataReturnsFalseWithNoValues()
        {
            Log.Information("TEST: UpdateFileMetadataReturnsFalseWithNoValues");
            fix.db.AddFile(@"C:\Temp\file_to_not_update", "text", "aaa", "first_alt");
            FileSearchFilter updateValues = new FileSearchFilter();
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFilenameFilter("file_to_not_update", true);
            Assert.False(fix.db.UpdateFileMetadata(updateValues, filter));
        }

        [Fact]
        public void UpdateFileMetadataReturnsFalseWithNoFilters()
        {
            Log.Information("TEST: UpdateFileMetadataReturnsFalseWithNoFilters");
            fix.db.AddFile(@"C:\Temp\file_to_not_update2", "text", "aaa", "first_alt");
            FileSearchFilter updateValues = new FileSearchFilter();
            FileSearchFilter filter = new FileSearchFilter();
            updateValues.SetFilenameFilter("unused");
            Assert.False(fix.db.UpdateFileMetadata(updateValues, filter));
        }

        [Fact]
        public void AddTagCategoryWorksAndReturnsTrue()
        {
            Log.Information("TEST: AddTagCategoryWorksAndReturnsTrue");
            int count = fix.db.GetAllTagCategories().Count;
            Assert.True(fix.db.AddTagCategory("Category1"));
            Assert.Equal(count + 1, fix.db.GetAllTagCategories().Count);
        }

        [Fact]
        public void AddTagCategoryWithDuplicateFails()
        {
            Log.Information("TEST: AddTagCategoryWithDuplicateFails");
            fix.db.AddTagCategory("Duplicate1");
            Assert.False(fix.db.AddTagCategory("Duplicate1"));
        }

        [Fact]
        public void AddTagWorksAndReturnsTrue()
        {
            Log.Information("TEST: AddTagWorksAndReturnsTrue");
            int count = fix.db.GetAllTags().Count;
            Assert.True(fix.db.AddTag("Tag1"));
            Assert.True(fix.db.AddTag("Tag2", "Tag2NewCategory"));
            fix.db.AddTagCategory("Tag3Category");
            Assert.True(fix.db.AddTag("Tag3", "Tag3Category"));
            Assert.Equal(count + 3, fix.db.GetAllTags().Count);
        }

        [Fact]
        public void AddTagWithDuplicateFails()
        {
            Log.Information("TEST: AddTagWithDuplicateFails");
            fix.db.AddTag("Duplicate");
            int count = fix.db.GetAllTags().Count;
            Assert.False(fix.db.AddTag("Duplicate"));
            Assert.Equal(count, fix.db.GetAllTags().Count);
        }

        [Fact]
        public void AddingTagsAndTagCategoriesPersistsInDB()
        {
            Log.Information("TEST: AddingTagsAndTagCategoriesPersistsInDB");
            fix.db.AddTag("TagR1");
            fix.db.AddTag("TagR2", "persist_category");
            fix.db.AddTag("TagR3", "persist_category");
            fix.db.AddTag("TagR4", "persist_category2");
            var tags = fix.db.GetAllTags();
            Assert.Contains(tags, (t) => t.Name == "TagR1");
            Assert.Contains(tags, (t) => t.Name == "TagR2" && t.Category == "persist_category");
            Assert.Contains(tags, (t) => t.Name == "TagR3" && t.Category == "persist_category");
            Assert.Contains(tags, (t) => t.Name == "TagR4" && t.Category == "persist_category2");
        }

        [Fact]
        public void CanAddAndGetTagsForSpecificFiles()
        {
            Log.Information("TEST: CanAddAndGetTagsForSpecificFiles");
            fix.db.AddFile(@"A:\tagged_file1", "bin", "aaa");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFullnameFilter(@"A:\tagged_file1");
            int id = fix.db.GetFileMetadataFiltered(filter)[0].ID;
            Assert.True(fix.db.AddTagToFile(id, "file_tag1"));
            var fileTags = fix.db.GetTagsForFile(id);
            Assert.Single(fileTags);
            Assert.Contains(fileTags, t => t.Name == "file_tag1");
        }

        [Fact]
        public void AddAndGetTagsWorksWithTagCategories()
        {
            Log.Information("TEST: AddAndGetTagsWorksWithTagCategories");
            fix.db.AddFile(@"A:\tagged_file2", "text", "fff");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFullnameFilter(@"A:\tagged_file2");
            int id = fix.db.GetFileMetadataFiltered(filter)[0].ID;
            Assert.True(fix.db.AddTagToFile(id, "file_tag_w_cat", "tag_cat1"));
            var fileTags = fix.db.GetTagsForFile(id);
            Assert.Single(fileTags);
            Assert.Contains(fileTags, t => t.Name == "file_tag_w_cat" && t.Category == "tag_cat1");
        }

        [Fact]
        public void AddTagToFileWithTagReuseUsesExistingTag()
        {
            Log.Information("TEST: AddTagToFileWithTagReuseUsesExistingTag");
            fix.db.AddTag("existing_tag");
            int count = fix.db.GetAllTags().Count;
            fix.db.AddFile(@"A:\tagged_file3", "text", "ccc");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFullnameFilter(@"A:\tagged_file3");
            int id = fix.db.GetFileMetadataFiltered(filter)[0].ID;
            fix.db.AddTagToFile(id, "existing_tag");
            var fileTags = fix.db.GetTagsForFile(id);
            Assert.Contains(fileTags, t => t.Name == "existing_tag");
            Assert.Equal(count, fix.db.GetAllTags().Count);
        }

        [Fact]
        public void AddTagToFileAgainWithSameTagReturnsFalse()
        {
            Log.Information("TEST: AddTagToFileAgainWithSameTagReturnsFalse");
            fix.db.AddFile(@"A:\file_tag_reuse", "text", "123");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFullnameFilter(@"A:\file_tag_reuse");
            int id = fix.db.GetFileMetadataFiltered(filter)[0].ID;
            fix.db.AddTagToFile(id, "reuse_tag");
            Assert.False(fix.db.AddTagToFile(id, "reuse_tag"));
        }

        [Fact]
        public void AddTagCategoriesWithDuplicateFails()
        {
            Log.Information("TEST: AddTagCategoriesWithDuplicateFails");
            fix.db.AddTagCategory("duplicate");
            int count = fix.db.GetAllTagCategories().Count;
            Assert.False(fix.db.AddTagCategory("duplicate"));
            Assert.Equal(count, fix.db.GetAllTagCategories().Count);
        }

        [Fact]
        public void DeleteTagCategoryRemovesAndReturnsTrue()
        {
            Log.Information("TEST: DeleteTagCategoryRemovesAndReturnsTrue");
            fix.db.AddTagCategory("delete");
            int id = fix.db.GetAllTagCategories().Find(tc => tc.Name == "delete").ID;
            int count = fix.db.GetAllTagCategories().Count;
            Assert.True(fix.db.DeleteTagCategory(id));
            Assert.Equal(count - 1, fix.db.GetAllTagCategories().Count);

        }

        [Fact]
        public void DeleteTagCategoryNonExistentReturnsFalse()
        {
            Log.Information("TEST: DeleteTagCategoryNonExistentReturnsFalse");
            int count = fix.db.GetAllTagCategories().Count;
            Assert.False(fix.db.DeleteTagCategory(-1));
            Assert.Equal(count, fix.db.GetAllTagCategories().Count);
        }

        [Fact]
        public void RemovingTagCategoryRemovesFromTag()
        {
            Log.Information("TEST: RemovingTagCategoryRemovesFromTag");
            fix.db.AddTag("tag_w_cat_rem", "rem_category");
            int catID = fix.db.GetAllTagCategories().Find(tc => tc.Name == "rem_category").ID;
            int tagID = fix.db.GetAllTags().Find(t => t.Name == "tag_w_cat_rem").ID;
            fix.db.DeleteTagCategory(catID);
            var tag = fix.db.GetAllTags().Find(t => t.Name == "tag_w_cat_rem");
            Assert.Null(tag.Category);
            Assert.Equal(-1, tag.CategoryID);
        }

        [Fact]
        public void DeleteTagRemovesAndReturnsTrue()
        {
            Log.Information("TEST: DeleteTagRemovesAndReturnsTrue");
            fix.db.AddTag("to_rem");
            var tags = fix.db.GetAllTags();
            int id = tags.Find(t => t.Name == "to_rem").ID;
            Assert.True(fix.db.DeleteTag(id));
            Assert.Equal(tags.Count - 1, fix.db.GetAllTags().Count);
        }

        [Fact]
        public void DeleteTagWithDuplicateFails()
        {
            Log.Information("TEST: DeleteTagWithDuplicateFails");
            Assert.False(fix.db.DeleteTag(-1));
        }

        [Fact]
        public void DeleteTagRemovesTagFromFile()
        {
            Log.Information("TEST: DeleteTagRemovesTagFromFile");
            fix.db.AddFile(@"A:\tagged_file4", "text", "aaa");
            int id = fix.db.GetFileMetadataFiltered(
                new FileSearchFilter()
                .SetFullnameFilter(@"A:\tagged_file4"))[0].ID;
            fix.db.AddTagToFile(id, "to_be_rem");
            int tagID = fix.db.GetAllTags().Find(t => t.Name == "to_be_rem").ID;
            var old_tags = fix.db.GetTagsForFile(id);
            fix.db.DeleteTag(tagID);
            Assert.Empty(fix.db.GetTagsForFile(id));
            Assert.Equal(old_tags.Count - 1, fix.db.GetTagsForFile(id).Count);
        }

        [Fact]
        public void AddCollectionWithNewNameReturnsTrue()
        {
            Log.Information("TEST: AddCollectionWithNewNameReturnsTrue");
            Assert.True(fix.db.AddCollection("new_collection", new List<int>()));
            Assert.True(fix.db.AddCollection("new_collection_null"));
        }

        [Fact]
        public void GetFileCollectionWorksWithEmptyCollection()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddCollection("empty");
            var collection = fix.db.GetFileCollection("empty");
            Assert.Equal("empty", collection.Name);
            Assert.Empty(collection.Files);
        }

        [Fact]
        public void AddCollectionWorksWithFiles()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddFile(@"F:\collection_file1", "text", "aaa");
            fix.db.AddFile(@"F:\collection_file2", "text", "aaa");
            fix.db.AddFile(@"F:\collection_file3", "text", "aaa");
            var filter = new FileSearchFilter();
            List<int> ids = new List<int>();
            ids.Add(fix.db.GetFileMetadataFiltered(filter.SetFilenameFilter("collection_file1"))[0].ID);
            ids.Add(fix.db.GetFileMetadataFiltered(filter.SetFilenameFilter("collection_file2"))[0].ID);
            ids.Add(fix.db.GetFileMetadataFiltered(filter.SetFilenameFilter("collection_file3"))[0].ID);
            Assert.True(fix.db.AddCollection("collection_w_files", ids));
            var collection = fix.db.GetFileCollection("collection_w_files");
            Assert.Equal("collection_w_files", collection.Name);
            var collectionFiles = collection.Files.ConvertAll(f => f.FileID);
            Assert.Subset(new HashSet<int>(ids), new HashSet<int>(collectionFiles));
            Assert.Equal(ids.Count, collectionFiles.Count);
        }

        [Fact]
        public void AddCollectionWithNonExistentFilesThrowsSQLiteException()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            List<int> unusedIDs = new List<int>() { 1000, 2000 };
            List<int> negIDs = new List<int>() { -1, 0 };
            Action act = () => { fix.db.AddCollection("bad_collection", unusedIDs); };
            Assert.Throws<SQLiteException>(act);
            act = () => { fix.db.AddCollection("bad_collection2", negIDs); };
            Assert.Throws<SQLiteException>(act);
        }

        [Fact]
        public void AddCollectionDuplicateNamesReturnsFalse()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddCollection("duplicate");
            Assert.False(fix.db.AddCollection("duplicate"));
        }

        [Fact]
        public void AddCollectionWithBadIDInFileListThrows()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            Assert.Throws<SQLiteException>(() => fix.db.AddCollection("bad_list", new List<int>() { 1, 2, -1 }));
            Assert.Null(fix.db.GetFileCollection("bad_list"));
        }

        [Fact]
        public void AddFilesToCollectionAddsToEnd()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddCollection("file_collection1");
            fix.db.AddFile(@"C:\Collection\file1", "text", "aaa");
            fix.db.AddFile(@"C:\Collection\file2", "text", "aaa");
            int id = fix.db.GetFileCollection("file_collection1").ID;
            FileSearchFilter filter = new FileSearchFilter();
            int fileID1 = fix.db.GetFileMetadataFiltered(filter.SetFullnameFilter(@"C:\Collection\file1"))[0].ID;
            int fileID2 = fix.db.GetFileMetadataFiltered(filter.SetFullnameFilter(@"C:\Collection\file2"))[0].ID;
            Assert.True(fix.db.AddFileToCollection(id, fileID1));
            Assert.True(fix.db.AddFileToCollection(id, fileID2));
            var collection = fix.db.GetFileCollection(id);
            Assert.Equal(2, collection.Files.Count);
            Assert.Equal(fileID1, collection.Files[0].FileID);
            Assert.Equal(fileID2, collection.Files[1].FileID);
            Assert.Equal(1, collection.Files[0].Position);
            Assert.Equal(2, collection.Files[1].Position);
        }

        [Fact]
        public void AddFilesToCollectionWorksInNonEndPositions()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddCollection("file_collection2");
            fix.db.AddFile(@"C:\Collection\file3", "text", "aaa");
            fix.db.AddFile(@"C:\Collection\file4", "text", "aaa");
            fix.db.AddFile(@"C:\Collection\file5", "text", "aaa");
            fix.db.AddFile(@"C:\Collection\file6", "text", "aaa");
            int id = fix.db.GetFileCollection("file_collection2").ID;
            FileSearchFilter filter = new FileSearchFilter();
            int fileID1 = fix.db.GetFileMetadataFiltered(filter.SetFullnameFilter(@"C:\Collection\file3"))[0].ID;
            int fileID2 = fix.db.GetFileMetadataFiltered(filter.SetFullnameFilter(@"C:\Collection\file4"))[0].ID;
            int fileID3 = fix.db.GetFileMetadataFiltered(filter.SetFullnameFilter(@"C:\Collection\file5"))[0].ID;
            int fileID4 = fix.db.GetFileMetadataFiltered(filter.SetFullnameFilter(@"C:\Collection\file6"))[0].ID;
            Assert.True(fix.db.AddFileToCollection(id, fileID2));
            Assert.True(fix.db.AddFileToCollection(id, fileID4));
            Assert.True(fix.db.AddFileToCollection(id, fileID1, 1));
            Assert.True(fix.db.AddFileToCollection(id, fileID3, 3));
            var collection = fix.db.GetFileCollection(id);
            Assert.Equal(4, collection.Files.Count);
            Assert.Equal(fileID1, collection.Files[0].FileID);
            Assert.Equal(fileID2, collection.Files[1].FileID);
            Assert.Equal(fileID3, collection.Files[2].FileID);
            Assert.Equal(fileID4, collection.Files[3].FileID);
            Assert.Equal(1, collection.Files[0].Position);
            Assert.Equal(2, collection.Files[1].Position);
            Assert.Equal(3, collection.Files[2].Position);
            Assert.Equal(4, collection.Files[3].Position);
        }

        [Fact]
        public void AddFilesToCollectionWithBadIDsFails()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddCollection("bad_add");
            Assert.False(fix.db.AddFileToCollection(fix.db.GetFileCollection("bad_add").ID, -1));
            Assert.Empty(fix.db.GetFileCollection("bad_add").Files);
        }

        [Fact]
        public void DeleteFileInCollectionWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddFile(@"C:\Collection\collection_file_rem", "text", "aaa");
            int fileID = fix.db.GetFileMetadataFiltered(
                new FileSearchFilter()
                .SetFullnameFilter(@"C:\Collection\collection_file_rem"))[0].ID;
            fix.db.AddCollection("collection_w_deleted", new List<int>() { fileID });
            int id = fix.db.GetFileCollection("collection_w_deleted").ID;
            Assert.True(fix.db.DeleteFileInCollection(id, fileID));
            Assert.Empty(fix.db.GetFileCollection("collection_w_deleted").Files);
        }

        [Fact]
        public void DeleteFileInCollectionReordersFiles()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddFile(@"C:\Collection\reorder1", "text", "aaa");
            fix.db.AddFile(@"C:\Collection\reorder2", "text", "aaa");
            fix.db.AddFile(@"C:\Collection\reorder3", "text", "aaa");
            var filter = new FileSearchFilter();
            int fileID1 = fix.db.GetFileMetadataFiltered(
                filter.SetFullnameFilter(@"C:\Collection\reorder1"))[0].ID;
            int fileID2 = fix.db.GetFileMetadataFiltered(
                filter.SetFullnameFilter(@"C:\Collection\reorder2"))[0].ID;
            int fileID3 = fix.db.GetFileMetadataFiltered(
                filter.SetFullnameFilter(@"C:\Collection\reorder3"))[0].ID;
            fix.db.AddCollection("collection_delete_reorder", new List<int>() { fileID1, fileID2, fileID3 });
            int id = fix.db.GetFileCollection("collection_delete_reorder").ID;
            Assert.True(fix.db.DeleteFileInCollection(id, fileID1));
            var collection = fix.db.GetFileCollection("collection_delete_reorder");
            Assert.Equal(1, collection.Files[0].Position);
            Assert.Equal(fileID2, collection.Files[0].FileID);
            Assert.Equal(2, collection.Files[1].Position);
            Assert.Equal(fileID3, collection.Files[1].FileID);
        }

        [Fact]
        public void UpdateCollectionNameWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddCollection("old_name");
            int id = fix.db.GetFileCollection("old_name").ID;
            Assert.True(fix.db.UpdateCollectionName("old_name", "new_name"));
            Assert.NotNull(fix.db.GetFileCollection("new_name"));
            Assert.Null(fix.db.GetFileCollection("old_name"));
            Assert.Equal("new_name", fix.db.GetFileCollection(id).Name);
            Assert.True(fix.db.UpdateCollectionName(id, "new_name2"));
            Assert.NotNull(fix.db.GetFileCollection("new_name2"));
        }

        [Fact]
        public void UpdateCollectionNameWithUsedNameFails()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddCollection("used_name");
            fix.db.AddCollection("temp");
            Assert.False(fix.db.UpdateCollectionName("temp", "used_name"));
        }

        [Fact]
        public void NetTest()
        {
            //fix.db = new FileDBManagerClass(TestLoader.GetNodeValue("TestDB"), fix.logger);
        }
    }
}
