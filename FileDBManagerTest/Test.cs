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
            string path = Path.Combine("logs", "log.log");
            if (!Directory.Exists("logs")) Directory.CreateDirectory("logs");
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(path, rollingInterval: RollingInterval.Day)
                .WriteTo.Debug()
                .CreateLogger();
            logger = new SerilogLoggerFactory(Log.Logger).CreateLogger<IServiceProvider>();
            db = new FileDBManagerClass(TestLoader.GetNodeValue("TestDB"), logger);
        }

        public void Dispose()
        {
            Log.CloseAndFlush();
            db.CloseConnection();
        }
    }

    public class Test : IClassFixture<TestFixture>
    {
        TestFixture fix;
        public Test(TestFixture fix)
        {
            this.fix = fix;
        }

        [Fact]
        public void GetAllFileMetadataReturnsNothingInEmptyDB()
        {
            Program.Main(new string[] { });
        }

        [Fact]
        public void NetTest()
        {
            fix.db = new FileDBManagerClass(TestLoader.GetNodeValue("TestDB"), fix.logger);
        }
    }
}
