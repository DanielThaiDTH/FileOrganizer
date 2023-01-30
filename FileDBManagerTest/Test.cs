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

namespace FileDBManager.Test
{
    public static class TestLoader
    {
        public static string GetNodeValue(string nodeName)
        {
            var xml = new XPathDocument(@"..\..\..\TestData.xml");
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
            Assert.True(fix.db.AddFile(@"C:\Temp\file1.txt", "text", "aaa", "testfile"));
        }

        [Fact]
        public void GetAllFileMetadataReturnsAddedFiles()
        {
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
            List<GetFileMetadataType> results = fix.db.GetAllFileMetadata();
            foreach (var res in results) {
                res.Equals(fix.db.GetFileMetadata(res.ID));
            }
        }

        [Fact]
        public void NetTest()
        {
            fix.db = new FileDBManagerClass(TestLoader.GetNodeValue("TestDB"), fix.logger);
        }
    }
}
