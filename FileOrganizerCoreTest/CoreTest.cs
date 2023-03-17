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
    }
}
