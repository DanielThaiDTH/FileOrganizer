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
using FileDBManager.Entities;
using System.Data;
using System.Data.SQLite;
using System.Configuration;

namespace FileDBManager.Test
{
    
    public class TestFixture : IDisposable
    {
        public FileDBManagerClass db = null;
        public Microsoft.Extensions.Logging.ILogger logger;
        
        public TestFixture()
        {
            string dbPath = ConfigurationManager.AppSettings.Get("TestDB");
            if (File.Exists(dbPath)) {
                File.Delete(dbPath);
            }
            string path = Path.Combine("logs", "log.log");
            if (!Directory.Exists("logs")) Directory.CreateDirectory("logs");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(path,  rollingInterval: RollingInterval.Day)
                .WriteTo.Debug()
                .CreateLogger();
            logger = new SerilogLoggerFactory(Log.Logger).CreateLogger<IServiceProvider>();
            Log.Information(Path.GetFullPath(dbPath));
            db = new FileDBManagerClass(dbPath, logger);
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
        public void GetFileMetadataWithFilenamesReturnsCorrectResults()
        {
            Log.Information("TEST: GetFileMetadataWithFilenamesReturnsCorrectResults");
            fix.db.AddFile(@"C:\Temp\file_test1", "image", "testHash", "alt.bmp");
            fix.db.AddFile(@"C:\Temp\file_test2", "audio", "testHash2", "alt.mp3");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFilenameFilter("file_test1", true);
            List<GetFileMetadataType> results = fix.db.GetFileMetadata(filter);
            Assert.Single(results);
            Assert.Equal("file_test1", results[0].Filename);
            Assert.Equal("alt.bmp", results[0].Altname);
            Assert.Equal(@"C:\Temp", results[0].Path);
            Assert.Equal("image", results[0].FileType);
            filter.Reset().SetFilenameFilter("file_test", false);
            results = fix.db.GetFileMetadata(filter);
            Assert.Equal(2, results.Count);
            filter.Reset().SetFilenameFilter("cannot_be_found", false);
            results = fix.db.GetFileMetadata(filter);
            Assert.Empty(results);
        }

        [Fact]
        public void GetFileMetadataWithAltnamesReturnsCorrectResults()
        {
            Log.Information("TEST: GetFileMetadataWithAltnamesReturnsCorrectResults");
            fix.db.AddFile(@"C:\Temp\alt_test1", "text", "aaa", "altname1");
            fix.db.AddFile(@"C:\Temp\alt_test2", "text", "aaa", "altname2");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetAltnameFilter("altname1", true);
            Assert.Single(fix.db.GetFileMetadata(filter));
            filter.Reset().SetAltnameFilter("altname", false);
            Assert.Equal(2, fix.db.GetFileMetadata(filter).Count);
            filter.Reset().SetAltnameFilter("cannot_be_found", false);
            Assert.Empty(fix.db.GetFileMetadata(filter));
        }

        [Fact]
        public void GetFileMetadataWithPathReturnsCorrectResults()
        {
            Log.Information("TEST: GetFileMetadataWithPathReturnsCorrectResults");
            fix.db.AddFile(@"Z:\pathA\pathB\file", "text", "aaa", "testfile");
            fix.db.AddFile(@"Z:\pathA\pathC\file", "text", "aaa", "testfile");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetPathFilter(@"Z:\pathA\pathB", true);
            Assert.Single(fix.db.GetFileMetadata(filter));
            filter.Reset().SetPathFilter("Z:\\pathA\\path", false);
            Assert.Equal(2, fix.db.GetFileMetadata(filter).Count);
            filter.Reset().SetPathFilter("cannot_be_found", false);
            Assert.Empty(fix.db.GetFileMetadata(filter));
        }

        [Fact]
        public void GetFileMetadataWithHashReturnsCorrectResults()
        {
            Log.Information("TEST: GetFileMetadataWithHashReturnsCorrectResults");
            fix.db.AddFile(@"C:\Temp\hash_test_file1", "text", "000", "");
            fix.db.AddFile(@"C:\Temp\hash_test_file2", "text", "100", "");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetHashFilter("000", true);
            Assert.Single(fix.db.GetFileMetadata(filter));
            filter.Reset().SetHashFilter("00", false);
            Assert.Equal(2, fix.db.GetFileMetadata(filter).Count);
            filter.Reset().SetHashFilter("cannot_be_found", false);
            Assert.Empty(fix.db.GetFileMetadata(filter));
        }

        [Fact]
        public void GetFileMetadataWithFileTypeReturnsCorrectResults()
        {
            Log.Information("TEST: GetFileMetadataWithFileTypeReturnsCorrectResults");
            fix.db.AddFile(@"C:\Temp\type_file1", "type1", "aaa", "");
            fix.db.AddFile(@"C:\Temp\type_file2", "type2", "aaa", "");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFileTypeFilter("type1", true);
            Assert.Single(fix.db.GetFileMetadata(filter));
            filter.Reset().SetFileTypeFilter("type", false);
            Assert.Equal(2, fix.db.GetFileMetadata(filter).Count);
            filter.Reset().SetFileTypeFilter("cannot_be_found", false);
            Assert.Empty(fix.db.GetFileMetadata(filter));
        }

        [Fact]
        public void GetFileMetadataWithNonExactFullnameFilterWorks()
        {
            Log.Information("TEST: GetFileMetadataWithNonExactFullnameFilterWorks");
            fix.db.AddFile(@"S:\Temp\unique_file_inexact", "text", "a1a", "unique_alt");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFullnameFilter(@"\unique_file_in", false);
            Assert.Single(fix.db.GetFileMetadata(filter));
        }

        [Fact]
        public void GetFileMetadataWithTagIDsReturnsCorrectResults()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var filter = new FileSearchFilter();
            fix.db.AddFile(@"C:\Tag\tagged_file1", "text", "aaa");
            int id1 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Tag\tagged_file1"))[0].ID;
            fix.db.AddFile(@"C:\Tag\tagged_file2", "text", "aaa");
            int id2 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Tag\tagged_file2"))[0].ID;
            fix.db.AddFile(@"C:\Tag\tagged_file3", "text", "aaa");
            int id3 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Tag\tagged_file3"))[0].ID;
            fix.db.AddTagToFile(id1, "ftag1");
            fix.db.AddTagToFile(id1, "ftag3");
            fix.db.AddTagToFile(id2, "ftag2");
            fix.db.AddTagToFile(id3, "ftag3");
            fix.db.AddTagToFile(id3, "ftag2");
            var tags = fix.db.GetAllTags();
            int tagid1 = tags.Find(t => t.Name == "ftag1").ID;
            int tagid2 = tags.Find(t => t.Name == "ftag2").ID;
            int tagid3 = tags.Find(t => t.Name == "ftag3").ID;
            filter.Reset().SetTagFilter(new List<int>() { tagid1 });
            Assert.Equal("tagged_file1", fix.db.GetFileMetadata(filter)[0].Filename);
            filter.Reset().SetTagFilter(new List<int>() { tagid2 });
            var result = fix.db.GetFileMetadata(filter);
            Assert.Contains(result, (t) => t.Filename == "tagged_file2");
            Assert.Contains(result, (t) => t.Filename == "tagged_file3");
            Assert.Equal(2, result.Count);
            filter.Reset().SetTagFilter(new List<int>() { tagid1, tagid3 });
            result = fix.db.GetFileMetadata(filter);
            Assert.Contains(result, (t) => t.Filename == "tagged_file1");
            Assert.Contains(result, (t) => t.Filename == "tagged_file3");
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetFileMetadataWithTagNamesReturnsCorrectResults()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var filter = new FileSearchFilter();
            fix.db.AddFile(@"C:\Tag\tagged_file4", "text", "aaa");
            int id1 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Tag\tagged_file4"))[0].ID;
            fix.db.AddFile(@"C:\Tag\tagged_file5", "text", "aaa");
            int id2 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Tag\tagged_file5"))[0].ID;
            fix.db.AddFile(@"C:\Tag\tagged_file6", "text", "aaa");
            int id3 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Tag\tagged_file6"))[0].ID;
            fix.db.AddTagToFile(id1, "f2tag1");
            fix.db.AddTagToFile(id1, "f2tag3");
            fix.db.AddTagToFile(id2, "f2tag2");
            fix.db.AddTagToFile(id3, "f2tag3");
            fix.db.AddTagToFile(id3, "f2tag2");
            filter.Reset().SetTagFilter(new List<string>() { "f2tag1" });
            Assert.Equal("tagged_file4", fix.db.GetFileMetadata(filter)[0].Filename);
            filter.Reset().SetTagFilter(new List<string>() { "f2tag2" });
            var result = fix.db.GetFileMetadata(filter);
            Assert.Contains(result, (t) => t.Filename == "tagged_file5");
            Assert.Contains(result, (t) => t.Filename == "tagged_file6");
            Assert.Equal(2, result.Count);
            filter.Reset().SetTagFilter(new List<string>() { "f2tag1", "f2tag3" });
            result = fix.db.GetFileMetadata(filter);
            Assert.Contains(result, (t) => t.Filename == "tagged_file4");
            Assert.Contains(result, (t) => t.Filename == "tagged_file6");
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetFileMetadataWithTagNamesIgnoresCase()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var filter = new FileSearchFilter();
            fix.db.AddFile(@"C:\Tag\case_tagged_file1", "text", "aaa");
            int id1 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Tag\case_tagged_file1"))[0].ID;
            fix.db.AddFile(@"C:\Tag\case_tagged_file2", "text", "aaa");
            int id2 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Tag\case_tagged_file2"))[0].ID;
            fix.db.AddFile(@"C:\Tag\case_tagged_file3", "text", "aaa");
            int id3 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Tag\case_tagged_file3"))[0].ID;
            fix.db.AddTagToFile(id1, "casetag");
            fix.db.AddTagToFile(id2, "caseTag");
            fix.db.AddTagToFile(id3, "Casetag");
            filter.Reset().SetTagFilter(new List<string>() { "casetag" });
            var result = fix.db.GetFileMetadata(filter);
            Assert.Equal(3, result.Count);
            Assert.Contains(result, (t) => t.Filename == "case_tagged_file1");
            Assert.Contains(result, (t) => t.Filename == "case_tagged_file2");
            Assert.Contains(result, (t) => t.Filename == "case_tagged_file3");
            filter.Reset().SetTagFilter(new List<string>() { "CASETAG" });
            result = fix.db.GetFileMetadata(filter);
            Assert.Equal(3, result.Count);
            Assert.Contains(result, (t) => t.Filename == "case_tagged_file1");
            Assert.Contains(result, (t) => t.Filename == "case_tagged_file2");
            Assert.Contains(result, (t) => t.Filename == "case_tagged_file3");
        }

        [Fact]
        public void GetFileMetadataWithInexactTagNamesWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var filter = new FileSearchFilter();
            fix.db.AddFile(@"C:\Tag\inexact_tag1", "text", "aaa");
            int id1 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Tag\inexact_tag1"))[0].ID;
            fix.db.AddFile(@"C:\Tag\inexact_tag2", "text", "aaa");
            int id2 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Tag\inexact_tag2"))[0].ID;
            fix.db.AddFile(@"C:\Tag\inexact_tag3", "text", "aaa");
            int id3 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Tag\inexact_tag3"))[0].ID;
            fix.db.AddTagToFile(id1, "like_tag!1");
            fix.db.AddTagToFile(id2, "like_tag!2");
            fix.db.AddTagToFile(id3, "tag!_like3");
            
            filter.Reset().SetTagFilter(new List<string> { "tag!" }, false);
            var result = fix.db.GetFileMetadata(filter);
            Assert.Contains(result, (t) => t.ID == id1);
            Assert.Contains(result, (t) => t.ID == id2);
            Assert.Contains(result, (t) => t.ID == id3);

            //filter.Reset().SetExcludeTagFilter(new List<string> { "like_" }, false);
        }

        [Fact]
        public void GetFileMetadataWithEmptyTagListHasNoEffect()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var filter = new FileSearchFilter();
            var baseResults = fix.db.GetAllFileMetadata();
            var results1 = fix.db.GetFileMetadata(filter.SetTagFilter(new List<int>()));
            var results2 = fix.db.GetFileMetadata(filter.SetTagFilter(new List<string>()));
            Assert.Equal(baseResults.Count, results1.Count);
            Assert.Equal(baseResults.Count, results2.Count);
        }

        [Fact]
        public void GetFileMetadataWithExcludeTagsWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddFile(@"C:\Tag\unfound_tags", "text", "aaa");
            var filter = new FileSearchFilter().SetFilenameFilter("unfound_tags");
            int id = fix.db.GetFileMetadata(filter)[0].ID;
            fix.db.AddTagToFile(id, "exclude_tag1");
            fix.db.AddTag("exclude_tag2");
            filter.Reset().SetExcludeTagFilter(new List<string>(){ "exclude_tag1" });
            var result = fix.db.GetFileMetadata(filter);
            Assert.DoesNotContain(result, (f) => f.Filename == "unfound_tags");
            filter.Reset().SetExcludeTagFilter(new List<string>() { "exclude_tag1", "exclude_tag2" });
            result = fix.db.GetFileMetadata(filter);
            Assert.Contains(result, (f) => f.Filename == "unfound_tags");
            filter.Reset().SetExcludeTagFilter(new List<string>() { "exclude_tag1", "exclude_tag2" }, true, true);
            result = fix.db.GetFileMetadata(filter);
            Assert.DoesNotContain(result, (f) => f.Filename == "unfound_tags");
            filter.Reset().SetExcludeTagFilter(new List<string>() { "exclude_tag2" });
            result = fix.db.GetFileMetadata(filter);
            Assert.Contains(result, (f) => f.Filename == "unfound_tags");
        }

        [Fact]
        public void GetFileMetadataWithTagsAndOptionWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var filter = new FileSearchFilter();
            fix.db.AddFile(@"C:\Tag\tagged_file7", "text", "aaa");
            int id1 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Tag\tagged_file7"))[0].ID;
            fix.db.AddFile(@"C:\Tag\tagged_file8", "text", "aaa");
            int id2 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Tag\tagged_file8"))[0].ID;
            fix.db.AddTagToFile(id1, "f3tag1");
            fix.db.AddTagToFile(id1, "f3tag2");
            fix.db.AddTagToFile(id2, "f3tag2");
            filter.Reset().SetTagFilter(new List<string>() { "f3tag1", "f3tag2" }, true, true);
            var result = fix.db.GetFileMetadata(filter);
            Assert.Single(result);
            Assert.Equal(id1, result[0].ID);
        }


        [Fact]
        public void FileWithCreatedDatePersistsCorrectly()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            DateTime fileTime = DateTime.Now;
            fix.db.AddFile(@"C:\Time\timed_file", "bin", "aaa", "", 10, new DateTimeOptional(fileTime));
            var file = fix.db.GetFileMetadata(new FileSearchFilter().SetFilenameFilter("timed_file"))[0];
            DateTime roundedTime = DateTimeOptional.RoundToUnixPrecision(fileTime);
            Assert.Equal(roundedTime, file.Created);
        }

        [Fact]
        public void CustomFileFilterWorksWithDates()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            DateTime fileTime = DateTime.Now.AddDays(-10);
            fix.db.AddFile(@"C:\Time\timed_file_old", "bin", "aaa", "", 10, new DateTimeOptional(fileTime));
            Func<List<GetFileMetadataType>, List<GetFileMetadataType>> customFilter = (res) =>
            {
                return res.FindAll(f =>
                {
                    DateTime now = DateTime.Now;
                    DateTime pastLimit = new DateTime(2000, 1, 1);
                    return f.Created < now && f.Created > pastLimit;
                });
            };
            var filter = new FileSearchFilter().SetCustom(customFilter);
            Assert.Single(fix.db.GetFileMetadata(filter));
        }

        [Fact]
        public void UnicodeFilenamesWork()
        {
            Assert.True(fix.db.AddFile(@"U:\画家\天風かや", "jpg", "aaa", "香風かや"));
            var filter = new FileSearchFilter();
            filter.SetFilenameFilter("天風かや");
            Assert.Single(fix.db.GetFileMetadata(filter));
            filter.Reset().SetFilenameFilter("風か", false);
            Assert.Single(fix.db.GetFileMetadata(filter));
            filter.Reset().SetAltnameFilter("香風かや", false);
            Assert.Single(fix.db.GetFileMetadata(filter));
            filter.Reset().SetPathFilter(@"U:\画家");
            Assert.Single(fix.db.GetFileMetadata(filter));
            filter.Reset().SetPathFilter("画", false);
            Assert.Single(fix.db.GetFileMetadata(filter));
            filter.Reset().SetFullnameFilter(@"U:\画家\天風かや");
            Assert.Single(fix.db.GetFileMetadata(filter));
            filter.Reset().SetFullnameFilter(@"家\天風か", false);
            Assert.Single(fix.db.GetFileMetadata(filter));
            Assert.Equal("天風かや", fix.db.GetFileMetadata(filter)[0].Filename);
        }

        [Fact]
        public void GetFileMetadataWithAllFiltersWorks()
        {
            Log.Information("TEST: GetFileMetadataWithAllFiltersWorks");
            var created = new DateTimeOptional(DateTime.Now.AddDays(-1));
            fix.db.AddFile(@"S:\Temp\unique_file", "text", "a1a", "unique_alt", 100, created);
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFilenameFilter("unique_file", true);
            var info = fix.db.GetFileMetadata(filter)[0];
            fix.db.AddTagToFile(info.ID, "all_filter_tag1");
            fix.db.AddTagToFile(info.ID, "all_filter_tag2");
            fix.db.AddTag("all_filter_tag3");
            var tags = fix.db.GetAllTags();
            int tagID1 = tags.Find(t => t.Name == "all_filter_tag1").ID;
            int tagID2 = tags.Find(t => t.Name == "all_filter_tag2").ID;
            int tagID3 = tags.Find(t => t.Name == "all_filter_tag3").ID;
            Func<List<GetFileMetadataType>, List<GetFileMetadataType>> timeFilter = (files) =>
            {
                return files.FindAll((f) => f.Created < created.Date);
            };

            filter.Reset()
                .SetIDFilter(info.ID)
                .SetPathIDFilter(info.PathID)
                .SetFileTypeIDFilter(info.FileTypeID)
                .SetFullnameFilter(@"S:\Temp\unique_file", true)
                .SetFilenameFilter("unique_file", true)
                .SetAltnameFilter("unique_alt", true)
                .SetHashFilter("a1a", true)
                .SetFileTypeFilter("text", true)
                .SetPathFilter(@"S:\Temp", true)
                .SetSizeFilter(1000, true)
                .SetTagFilter(new List<int>() { tagID1, tagID2 }, true)
                .SetExcludeTagFilter(new List<int>() { tagID3 })
                .SetCustom(timeFilter);
            Assert.Single(fix.db.GetFileMetadata(filter));
        }

        [Fact]
        public void GetFileMetadataWithSubFiltersWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddFile(@"S:\Temp\sub_filter1", "text", "a1a", "sub_alt", 10000);
            fix.db.AddFile(@"S:\Temp\sub_filter2", "text", "bbb", "sub_alt", 20000);
            var filter1 = new FileSearchFilter().SetAltnameFilter("sub_alt");
            var filter2 = new FileSearchFilter().SetSizeFilter(15000, false);
            var filter = new FileSearchFilter().AddSubfilter(filter1).AddSubfilter(filter2);
            var results = fix.db.GetFileMetadata(filter);
            Assert.Single(results);
            Assert.Contains(results, r => r.Filename == "sub_filter2");
            filter2.IsOr = true;
            filter.Reset().AddSubfilter(filter1).AddSubfilter(filter2);
            results = fix.db.GetFileMetadata(filter);
            Assert.Equal(2, results.Count);
            Assert.Contains(results, r => r.Filename == "sub_filter2");
            Assert.Contains(results, r => r.Filename == "sub_filter1");
        }

        [Fact]
        public void FileSearchFilterUsingCustomFilterCorrectValue()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            FileSearchFilter filter = new FileSearchFilter();
            Func<List<GetFileMetadataType>, List<GetFileMetadataType>> customFilter = (results) =>
            {
                return results;
            };
            Assert.False(filter.UsingCustomFilter);
            filter.SetCustom(customFilter);
            Assert.True(filter.UsingCustomFilter);
        }

        [Fact]
        public void GetFileMetadataWithCustomFilterWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            FileSearchFilter filter = new FileSearchFilter();
            Func<List<GetFileMetadataType>, List<GetFileMetadataType>> customFilter = (r) =>
            {
                var newResults = r.FindAll(f => f.Filename.StartsWith("match"));
                return newResults;
            };
            filter.SetCustom(customFilter);
            fix.db.AddFile(@"F:\matched\matched_file", "text", "aaa");
            fix.db.AddFile(@"F:\other\matching", "text", "aaa");
            fix.db.AddFile(@"F:\match\unmatched", "text", "aaa");
            var results = fix.db.GetFileMetadata(filter);
            Assert.Equal(2, results.Count);
            Assert.Contains(results, f => f.Filename == "matched_file");
            Assert.Contains(results, f => f.Filename == "matching");
            Assert.DoesNotContain(results, f => f.Filename == "unmatched");
        }

        [Fact]
        public void GetFileMetadataWithNotSetWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddFile(@"N:\not\file1", "text", "nnn");
            fix.db.AddFile(@"N:\not\file2", "text", "aaa");
            fix.db.AddFile(@"N:\not\file3", "binary", "nnn");
            FileSearchFilter filter1 = new FileSearchFilter().SetPathFilter(@"N:\not").SetFileTypeFilter("text");
            FileSearchFilter filter2 = new FileSearchFilter().SetNot(true).SetHashFilter("aaa");
            FileSearchFilter filter = new FileSearchFilter().AddSubfilter(filter1).AddSubfilter(filter2);
            var res = fix.db.GetFileMetadata(filter);
            Assert.Single(res);
            Assert.Contains(res, f => f.Filename == "file1");
        }

        [Fact]
        public void DeleteFileMetadataRemovesFile()
        {
            Log.Information("TEST: DeleteFileMetadataRemovesFile");
            fix.db.AddFile(@"C:\Temp\delete_file", "bin", "aaa");
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFilenameFilter("delete_file", true);
            var info = fix.db.GetFileMetadata(filter)[0];
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
            FileMetadata updateValues = new FileMetadata();
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFilenameFilter("file_to_update", true);
            updateValues.SetFilename("updated_name")
                        .SetFileType("bin")
                        .SetHash("bbb")
                        .SetAltname("second_alt");
            Assert.Empty(fix.db.GetFileMetadata(FileSearchFilter.FromMetadata(updateValues)));
            Assert.True(fix.db.UpdateFileMetadata(updateValues, filter));
            Assert.Single(fix.db.GetFileMetadata(FileSearchFilter.FromMetadata(updateValues)));
        }

        [Fact]
        public void UpdateFileMetadataReturnsFalseUsingFilterWithNoMatch()
        {
            Log.Information("TEST: UpdateFileMetadataReturnsFalseUsingFilterWithNoMatch");
            FileMetadata updateValues = new FileMetadata();
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFilenameFilter("cannot_be_found", true);
            updateValues.SetFilename("unused");
            Assert.False(fix.db.UpdateFileMetadata(updateValues, filter));
        }

        [Fact]
        public void UpdateFileMetadataReturnsFalseWithNoValues()
        {
            Log.Information("TEST: UpdateFileMetadataReturnsFalseWithNoValues");
            fix.db.AddFile(@"C:\Temp\file_to_not_update", "text", "aaa", "first_alt");
            FileMetadata updateValues = new FileMetadata();
            FileSearchFilter filter = new FileSearchFilter();
            filter.SetFilenameFilter("file_to_not_update", true);
            Assert.False(fix.db.UpdateFileMetadata(updateValues, filter));
        }

        [Fact]
        public void UpdateFileMetadataReturnsFalseWithNoFilters()
        {
            Log.Information("TEST: UpdateFileMetadataReturnsFalseWithNoFilters");
            fix.db.AddFile(@"C:\Temp\file_to_not_update2", "text", "aaa", "first_alt");
            FileMetadata updateValues = new FileMetadata();
            FileSearchFilter filter = new FileSearchFilter();
            updateValues.SetFilename("unused");
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
            int id = fix.db.GetFileMetadata(filter)[0].ID;
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
            int id = fix.db.GetFileMetadata(filter)[0].ID;
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
            int id = fix.db.GetFileMetadata(filter)[0].ID;
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
            int id = fix.db.GetFileMetadata(filter)[0].ID;
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
        public void UpdateTagCategoryNameWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddTagCategory("old_name");
            var categories = fix.db.GetAllTagCategories();
            int id = categories.Find(tc => tc.Name == "old_name").ID;
            Assert.True(fix.db.UpdateTagCategoryName(id, "new_name"));
            categories = fix.db.GetAllTagCategories();
            Assert.True(categories.Exists(tc => tc.Name == "new_name"));
            Assert.False(categories.Exists(tc => tc.Name == "old_name"));
        }

        [Fact]
        public void UpdateTagCategoryNameWithUsedNameFails()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddTagCategory("used_name");
            fix.db.AddTagCategory("to_be_renamed");
            var categories = fix.db.GetAllTagCategories();
            int id = categories.Find(tc => tc.Name == "to_be_renamed").ID;
            Assert.False(fix.db.UpdateTagCategoryName(id, "used_name"));
        }

        [Fact]
        public void UpdateTagNameWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddTag("old_tag_name");
            Assert.True(fix.db.UpdateTagName("new_tag_name", "old_tag_name"));
            var tags = fix.db.GetAllTags();
            Assert.Contains(tags, t => t.Name == "new_tag_name");
            Assert.DoesNotContain(tags, t => t.Name == "old_tag_name");
        }

        [Fact]
        public void UpdateTagNameFailsWithDuplicateName()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddTag("tag_to_rename");
            fix.db.AddTag("used_tag_name");
            Assert.False(fix.db.UpdateTagName("used_tag_name", "tag_to_rename"));
        }

        [Fact]
        public void UpdateTagDescriptionWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddTag("tag_w_desc");
            var tags = fix.db.GetAllTags();
            int id = tags.Find(t => t.Name == "tag_w_desc").ID;
            Assert.True(fix.db.UpdateTagDescription(id, "Description of the tag.\n New line"));
            tags = fix.db.GetAllTags();
            Assert.Contains(tags, t => t.ID == id && t.Description == "Description of the tag.\n New line");
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
            int id = fix.db.GetFileMetadata(
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
        public void DeleteTagFromFileOnlyRemovesTagFromFile()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddFile(@"A:\del_tagged_file_1", "text", "aaa");
            fix.db.AddFile(@"A:\del_tagged_file_2", "text", "aaa");
            fix.db.AddTag("file_del_tag");
            var filter = new FileSearchFilter();
            int id1 = fix.db.GetFileMetadata(filter.SetFilenameFilter("del_tagged_file_1"))[0].ID;
            int id2 = fix.db.GetFileMetadata(filter.SetFilenameFilter("del_tagged_file_2"))[0].ID;
            int tagID = fix.db.GetAllTags().Find(t => t.Name == "file_del_tag").ID;
            fix.db.AddTagToFile(id1, "file_del_tag");
            fix.db.AddTagToFile(id2, "file_del_tag");
            Assert.True(fix.db.DeleteTagFromFile(id1, tagID));
            var tags1 = fix.db.GetTagsForFile(id1);
            Assert.Empty(tags1);
            var tags2 = fix.db.GetTagsForFile(id2);
            Assert.Contains(tags2, (t) => t.ID == tagID );
            Assert.Contains(fix.db.GetAllTags(), (t) => t.ID == tagID);
        }

        [Fact]
        public void UpdateTagCategoryChangesCategory()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddTagCategory("tc1");
            fix.db.AddTagCategory("tc2");
            fix.db.AddTag("changedCategory", "tc1");
            int tcID = fix.db.GetAllTagCategories().Find(tc => tc.Name == "tc2").ID;
            Assert.True(fix.db.UpdateTagCategory("changedCategory", tcID));
            var tag = fix.db.GetAllTags().Find(t => t.Name == "changedCategory");
            Assert.Equal(tcID, tag.CategoryID);
            Assert.Equal("tc2", tag.Category);
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
            ids.Add(fix.db.GetFileMetadata(filter.SetFilenameFilter("collection_file1"))[0].ID);
            ids.Add(fix.db.GetFileMetadata(filter.SetFilenameFilter("collection_file2"))[0].ID);
            ids.Add(fix.db.GetFileMetadata(filter.SetFilenameFilter("collection_file3"))[0].ID);
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
            int fileID1 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Collection\file1"))[0].ID;
            int fileID2 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Collection\file2"))[0].ID;
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
            int fileID1 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Collection\file3"))[0].ID;
            int fileID2 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Collection\file4"))[0].ID;
            int fileID3 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Collection\file5"))[0].ID;
            int fileID4 = fix.db.GetFileMetadata(filter.SetFullnameFilter(@"C:\Collection\file6"))[0].ID;
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
        public void UpdateFilePositionInCollectionWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddFile(@"C:\Collection\reorderfile1", "text", "aaa");
            var filter = new FileSearchFilter().SetFullnameFilter(@"C:\Collection\reorderfile", false);
            var ids = fix.db.GetFileMetadata(filter).ConvertAll(f => f.ID);
            fix.db.AddCollection("reordered_collection", ids);
            int collectionID = fix.db.GetFileCollection("reordered_collection").ID;
            Assert.True(fix.db.UpdateFilePositionInCollection(collectionID, ids[0], 100));
            Assert.Equal(100, fix.db.GetFileCollection("reordered_collection").Files[0].Position);
        }

        [Fact]
        public void UpdateFilePositionInCollectionWithUsedIdFails()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddFile(@"C:\Collection\position_used1", "text", "aaa");
            fix.db.AddFile(@"C:\Collection\position_used2", "text", "aaa");
            var filter = new FileSearchFilter().SetFullnameFilter(@"C:\Collection\position_used", false);
            var ids = fix.db.GetFileMetadata(filter).ConvertAll(f => f.ID);
            fix.db.AddCollection("used_pos_collection", ids);
            int collectionID = fix.db.GetFileCollection("used_pos_collection").ID;
            Assert.False(fix.db.UpdateFilePositionInCollection(collectionID, ids[1], 1));
            Assert.Equal(1, fix.db.GetFileCollection("used_pos_collection").Files[0].Position);
            Assert.Equal(2, fix.db.GetFileCollection("used_pos_collection").Files[1].Position);
        }

        [Fact]
        public void DeleteFileInCollectionWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddFile(@"C:\Collection\collection_file_rem", "text", "aaa");
            int fileID = fix.db.GetFileMetadata(
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
            int fileID1 = fix.db.GetFileMetadata(
                filter.SetFullnameFilter(@"C:\Collection\reorder1"))[0].ID;
            int fileID2 = fix.db.GetFileMetadata(
                filter.SetFullnameFilter(@"C:\Collection\reorder2"))[0].ID;
            int fileID3 = fix.db.GetFileMetadata(
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
        public void DeleteCollectionWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddCollection("deleted_collection");
            fix.db.AddCollection("deleted_collection2");
            Assert.True(fix.db.DeleteCollection("deleted_collection"));
            int id = fix.db.GetFileCollection("deleted_collection2").ID;
            Assert.True(fix.db.DeleteCollection(id));
            Assert.Null(fix.db.GetFileCollection("deleted_collection"));
            Assert.Null(fix.db.GetFileCollection(id));
        }

        [Fact]
        public void DeleteCollectionOnNonexistentCollectionFails()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            Assert.False(fix.db.DeleteCollection("not_found"));
            Assert.False(fix.db.DeleteCollection(1000));
        }

        [Fact]
        public void FileCollectionSearchWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddCollection("collection_search_1");
            fix.db.AddCollection("collection_search_2");
            fix.db.AddCollection("collection_search_3");
            Assert.Equal(3, fix.db.FileCollectionSearch("search").Count);
            Assert.Equal(3, fix.db.FileCollectionSearch("collection_search_").Count);
            Assert.Single(fix.db.FileCollectionSearch("search_2"));
        }

        [Fact]
        public void FileCollectionSearchIncludesFileIDs()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddFile(@"C:\in_collection_1", "text", "aaa");
            fix.db.AddFile(@"C:\in_collection_2", "text", "aaa");
            fix.db.AddFile(@"C:\in_collection_3", "text", "aaa");
            var filter = new FileSearchFilter().SetFullnameFilter(@"C:\in_collection", false);
            var fileIDs = fix.db.GetFileMetadata(filter).ConvertAll(f => f.ID);
            fix.db.AddCollection("collection_w_f_1", fileIDs);
            fix.db.AddCollection("collection_w_f_2", fileIDs);
            var collections = fix.db.FileCollectionSearch("collection_w_f_");
            Assert.Equal(2, collections.Count);
            Assert.All(collections[0].Files, (fc) => Assert.Contains(fc.FileID, fileIDs));
            Assert.All(collections[1].Files, (fc) => Assert.Contains(fc.FileID, fileIDs));
        }

        [Fact]
        public void ChangePathChangesForAllAssociatedFiles()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.db.AddFile(@"O:\oldpath\path_file1", "bin", "aaa");
            fix.db.AddFile(@"O:\oldpath\path_file2", "bin", "aaa");
            var info = fix.db.GetFileMetadata(new FileSearchFilter().SetPathFilter(@"O:\oldpath"))[0];
            int pathID = info.PathID;
            Assert.True(fix.db.ChangePath(pathID, @"N:\newpath"));
            Assert.Empty(fix.db.GetFileMetadata(new FileSearchFilter().SetPathFilter(@"O:\oldpath")));
            Assert.Equal(2, fix.db.GetFileMetadata(new FileSearchFilter().SetPathFilter(@"N:\newpath")).Count);
        }
    }
}
