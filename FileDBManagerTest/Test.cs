using System;
using System.IO;
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
                .MinimumLevel.Debug()
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
        public void NetTest()
        {
            //fix.db = new FileDBManagerClass(TestLoader.GetNodeValue("TestDB"), fix.logger);
        }
    }
}
