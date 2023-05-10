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

namespace FileOrganizerCore.Test
{
    public class CoreTestFixture : IDisposable
    {
        public FileOrganizer core;
        public Microsoft.Extensions.Logging.ILogger logger;
        public string root;
        public FileTypeDeterminer det;

        public CoreTestFixture()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("log.log", rollingInterval: RollingInterval.Day)
                .WriteTo.Debug()
                .CreateLogger();
            logger = new SerilogLoggerFactory(Log.Logger).CreateLogger<IServiceProvider>();
            var configLoader = new ConfigLoader("config.xml", logger);
            if (File.Exists(configLoader.GetNodeValue("DB"))) {
                File.Delete(configLoader.GetNodeValue("DB"));
            }
            core = new FileOrganizer(logger);
            det = new FileTypeDeterminer();
            root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", "");
            var result = core.StartUp();
            if (!result.Result) {
                logger.LogError("Unable to start up core: " + result.GetErrorMessage(0));
            }
        }

        public void Dispose()
        {
            Log.Information("Disposing");
            Log.CloseAndFlush();
            core.ClearSymLinks();
            core.Stop();
        }
    }

    [CollectionDefinition("Core")]
    public class CoreCollection : ICollectionFixture<CoreTestFixture>
    {

    }

    [Collection("Core")]
    public class CoreTest
    {
        CoreTestFixture fix;

        public CoreTest(CoreTestFixture fix)
        {
            this.fix = fix;
        }

        [Fact]
        public void StartUpLoadsTagsCorrectly()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var result = fix.core.StartUp();
            Assert.True(result.Result);
        }

        [Fact]
        public void SetSymLinkFolderInvalidReturnsError()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var result = fix.core.SetSymLinkFolder("nonexistent");
            Assert.False(result.Result);
            Assert.Single(result.Messages);
            Assert.Equal(ErrorType.Path, result.GetError(0));
            Assert.Contains("symlink", fix.core.GetSymLinksRoot());
        }

        [Fact]
        public void SetSymLinkFolderValidWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var result = fix.core.SetSymLinkFolder("symlink2");
            Assert.True(result.Result);
            Assert.Contains("symlink2", fix.core.GetSymLinksRoot());
            fix.core.SetSymLinkFolder("symlink");
        }

        [Fact]
        public void CreateSymLinksFilenamesValidFilesWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var files = new List<string>() { Path.Combine(fix.root, "config.xml"),
                Path.Combine(fix.root, "Serilog.xml") };
            var res = fix.core.CreateSymLinksFilenames(files);
            Assert.True(res.Result);
            Assert.Equal(0, res.Count);
            Assert.True(File.Exists(Path.Combine(fix.root, "symlink", "config.xml")));
            Assert.True(File.Exists(Path.Combine(fix.root, "symlink", "Serilog.xml")));
            fix.core.ClearSymLinks();
        }

        [Fact]
        public void CreateSymLinksFilenamesInvalidFails()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var files = new List<string>() { "bad1", "bad2" };
            var res = fix.core.CreateSymLinksFilenames(files);
            Assert.False(res.Result);
            Assert.Equal(2, res.Count);
            Assert.Equal(ErrorType.SymLinkCreate, res.GetError(0));
            Assert.Equal(ErrorType.SymLinkCreate, res.GetError(1));
            Assert.False(File.Exists(Path.Combine(fix.root, "symlink", "bad1")));
            Assert.False(File.Exists(Path.Combine(fix.root, "symlink", "bad2")));
        }

        [Fact]
        public void CreateSymLinksFilenamesGoodAndBadReturnsFalseCreatesGood()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var files = new List<string>() { "bad1", Path.Combine(fix.root, "config.xml"), "bad2" };
            var res = fix.core.CreateSymLinksFilenames(files);
            Assert.False(res.Result);
            Assert.Equal(2, res.Count);
            Assert.Equal(ErrorType.SymLinkCreate, res.GetError(0));
            Assert.Equal(ErrorType.SymLinkCreate, res.GetError(1));
            Assert.True(File.Exists(Path.Combine(fix.root, "symlink", "config.xml")));
            Assert.False(File.Exists(Path.Combine(fix.root, "symlink", "bad1")));
            Assert.False(File.Exists(Path.Combine(fix.root, "symlink", "bad2")));
            fix.core.ClearSymLinks();
        }

        [Fact]
        public void AddFileAddsWithCorrectData()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            string fullname = Path.Combine(fix.root, "config.xml");
            var res = fix.core.AddFile(fullname);
            Assert.True(res.Result);
            var filter = new FileSearchFilter().SetFullnameFilter(fullname);
            var file = fix.core.GetFileData(filter).Result;
            Assert.Single(file);
            Assert.Equal(Utilities.Hasher.HashFile(fullname, res), file[0].Hash);
            Assert.Equal(fullname, file[0].Fullname);
            Assert.Equal(fix.det.FromExt("xml"), file[0].FileType);
            Assert.Equal(new FileInfo(fullname).Length, file[0].Size);
            fix.core.DeleteFile(fullname);
        }

        [Fact]
        public void CreateSymLinksFromActiveFilesWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var files = new List<string>() { 
                Path.Combine(fix.root, "config.xml"), 
                Path.Combine(fix.root, "Serilog.xml") 
            };
            var addRes = fix.core.AddFiles(files);
            Assert.All(addRes.Result, r => Assert.True(r));
            var filedata = fix.core.GetFileData().Result;
            var res = fix.core.CreateSymLinksFromActiveFiles();
            Assert.True(File.Exists(Path.Combine(fix.root, "symlink", "config.xml")));
            Assert.True(File.Exists(Path.Combine(fix.root, "symlink", "Serilog.xml")));
            fix.core.ClearSymLinks();
            foreach (var f in filedata) {
                fix.core.DeleteFile(f.ID);
            }
        }

        [Fact]
        public void CreateSymLinksFromActiveFilesReturnsErrorWithNoActiveFiles()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.core.GetFileData();
            var res = fix.core.CreateSymLinksFromActiveFiles();
            Assert.False(res.Result);
            Assert.Equal(ErrorType.Misc, res.GetError(0));
        }

        [Fact]
        public void DeleteFileNonexistentFails()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var res = fix.core.DeleteFile(-1);
            Assert.False(res.Result);
            Assert.Equal(ErrorType.SQL, res.GetError(0));
        }

        [Fact]
        public void GetTagCategoriesUpdatesTagCategories()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            int count = fix.core.TagCategories.Count;
            var res = fix.core.AddTagCategory("test1");
            Assert.True(res.Result);
            Assert.Equal(count + 1, fix.core.TagCategories.Count);
            Assert.Contains(fix.core.TagCategories, tc => tc.Name == "test1");
        }

        [Fact]
        public void UpdateTagCategoryNameRenamesAndRefreshes()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.core.AddTagCategory("old_category");
            int count = fix.core.TagCategories.Count;
            var res = fix.core.UpdateTagCategoryName("old_category", "new_category");
            Assert.True(res.Result);
            Assert.Equal(count, fix.core.TagCategories.Count);
            Assert.Contains(fix.core.TagCategories, tc => tc.Name == "new_category");
        }

        [Fact]
        public void GetFileDataFromIDsWorksWithIDList()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var files = new List<string>() {
                Path.Combine(fix.root, "config.xml"),
                Path.Combine(fix.root, "Serilog.xml")
            };
            fix.core.AddFiles(files);
            var fileData = fix.core.GetFileData().Result;
            var idList = fileData.ConvertAll(fd => fd.ID);
            var res = fix.core.GetFileData();
            Assert.False(res.HasError());
            Assert.Contains(res.Result, fd => fd.Filename == "config.xml");
            Assert.Contains(res.Result, fd => fd.Filename == "Serilog.xml");
            Assert.Equal(2, res.Result.Count);
            Assert.Subset(
                new HashSet<FileDBManager.GetFileMetadataType>(res.Result.ToArray()), 
                new HashSet<FileDBManager.GetFileMetadataType>(fix.core.ActiveFiles.ToArray())
                );
            fix.core.DeleteFile(Path.Combine(fix.root, "config.xml"));
            fix.core.DeleteFile(Path.Combine(fix.root, "Serilog.xml"));
        }

        [Fact]
        public void DeleteFileRemovesFromDB()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            var files = new List<string>() {
                Path.Combine(fix.root, "config.xml"),
                Path.Combine(fix.root, "Serilog.xml")
            };
            fix.core.AddFiles(files);
            Assert.True(fix.core.DeleteFile(Path.Combine(fix.root, "config.xml")).Result);
            Assert.True(fix.core.DeleteFile(Path.Combine(fix.root, "Serilog.xml")).Result);
            Assert.Empty(fix.core.GetFileData().Result);
            fix.core.AddFiles(files);
            var fileData = fix.core.GetFileData().Result;
            var idList = fileData.ConvertAll(fd => fd.ID);
            foreach (int id in idList) {
                Assert.True(fix.core.DeleteFile(id).Result);
            }
            Assert.Empty(fix.core.GetFileData().Result);
        }

        [Fact]
        public void UpdateTagCategoryWithNegative1RemovesCategoryFromTag()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.core.AddTag("tag_w_category", "test");
            int tagID = fix.core.AllTags.Find(t => t.Name == "tag_w_category").ID;
            Assert.True(fix.core.UpdateTagCategory(tagID, -1).Result);
            Assert.Null(fix.core.AllTags.Find(t => t.Name == "tag_w_category").Category);
            Assert.Equal(-1, fix.core.AllTags.Find(t => t.Name == "tag_w_category").CategoryID);
        }

        [Fact]
        public void SwitchFilePositionInCollectionWorks()
        {
            Log.Information($"TEST: {MethodBase.GetCurrentMethod().Name}");
            fix.core.AddFiles(new List<string> { 
                Path.Combine(fix.root, "FileManagerDB.dll.config"), 
                Path.Combine(fix.root, "FileOrganizerCore.dll.config"), 
                Path.Combine(fix.root, "FileOrganizerCoreTest.dll.config") 
            });
            var filter = new FileSearchFilter().SetFilenameFilter("dll.config", false);
            var ids = fix.core.GetFileData(filter).Result.ConvertAll(f => f.ID);
            fix.core.AddCollection("reordered_collection", ids);
            var collectionID = fix.core.GetFileCollection("reordered_collection").Result.ID;
            Assert.True(fix.core.SwitchFilePositionInCollection(collectionID, ids[0], ids[2]).Result);
            var collection = fix.core.GetFileCollection("reordered_collection").Result;
            Assert.Equal(3, collection.Files.Find(f => f.FileID == ids[0]).Position);
            Assert.Equal(1, collection.Files.Find(f => f.FileID == ids[2]).Position);
        }
    }
}
