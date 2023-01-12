using System;
using System.IO;
using Xunit;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SymLinkMaker.Test
{
    public class TestFixture : IDisposable
    {
        string root = @"C:\Temp\SymLinks";
        HashSet<string> seen;
        public TestLogger logger;
        public TestFixture()
        {
            seen = new HashSet<string>();
            logger = new TestLogger("logs", LogLevel.Information);
        }

        public void Dispose()
        {
            SymLinkMaker sym = new SymLinkMaker(root, logger);
            foreach (string p in seen) {
                sym.ClearLink(p);
            }
            logger.Finish();
        }

        public void AddSeen(string path)
        {
            seen.Add(path);
        }

        public void RemoveSeen(string path)
        {
            seen.Remove(path);
        }
    }

    public class Test : IClassFixture<TestFixture>
    {
        TestFixture fix;
        string root = @"C:\Temp\SymLinks";
        string source = @"C:\Temp\SymLinksSource\test.txt";

        public Test(TestFixture fix)
        {
            this.fix = fix;
        }

        [Fact]
        public void ConstructorTest()
        {
            SymLinkMaker sym = new SymLinkMaker(root, fix.logger);
            Assert.NotNull(sym);
            Assert.IsType<SymLinkMaker>(sym);
        }

        [Fact]
        public void ConstructorRelativeRootThrows()
        {
            Assert.Throws<ArgumentException>(() => new SymLinkMaker(".", fix.logger));
        }

        [Fact]
        public void ConstructorForbiddenRootsThrows()
        {
            Assert.Throws<ArgumentException>(() => new SymLinkMaker(@"C:\", fix.logger));
            Assert.Throws<ArgumentException>(() => new SymLinkMaker(@"C:\Windows", fix.logger));
            Assert.Throws<ArgumentException>(() => new SymLinkMaker(@"C:\Temp\system32", fix.logger));
        }

        [Fact]
        public void RootCorrectlyAssigned()
        {
            SymLinkMaker sym = new SymLinkMaker(root, fix.logger);
            Assert.Equal(root, sym.Root);
        }

        [Fact]
        public void MakeCreatesLinkAndReturnsTrue()
        {
            SymLinkMaker sym = new SymLinkMaker(root, fix.logger);
            Assert.True(sym.Make("test.txt", source, false));
            fix.AddSeen("test.txt");
            string[] lines = File.ReadAllLines(Path.Combine(root, "test.txt"));
            string[] origLines = File.ReadAllLines(source);

            Assert.Equal(lines.Length, origLines.Length);
            for (int i = 0; i < lines.Length; i++) {
                Assert.Equal(lines[i], origLines[i]);
            }
        }

        [Fact]
        public void MakeWithNullPathOrSourceReturnsFalse()
        {
            SymLinkMaker sym = new SymLinkMaker(root, fix.logger);
            Assert.False(sym.Make(null, source, false));
            Assert.False(sym.Make("fail.txt", null, false));
        }

        [Fact]
        public void IsSymLinkReturnsFalseForInvalidPaths()
        {
            SymLinkMaker sym = new SymLinkMaker(root, fix.logger);
            Assert.False(sym.IsSymLink(@"C:\"));
            Assert.False(sym.IsSymLink(null));
        }

        [Fact]
        public void IsSymLinkReturnsFalseForFilesOrDir()
        {
            SymLinkMaker sym = new SymLinkMaker(root, fix.logger);
            Assert.False(sym.IsSymLink("real.txt"));
            Assert.False(sym.IsSymLink("real"));
        }

        [Fact]
        public void IsSymLinkReturnsTrueWithSymLink()
        {
            SymLinkMaker sym = new SymLinkMaker(root, fix.logger);
            sym.Make("symtrue.txt", source, false);
            fix.AddSeen("symtrue.txt");
            Assert.True(sym.IsSymLink("symtrue.txt"));
        }

        [Fact]
        public void GetSymLinksReturnsArrayofSymLinks()
        {
            SymLinkMaker sym = new SymLinkMaker(root, fix.logger);
            sym.Make("x.txt", source, false);
            sym.Make("y.txt", source, false);
            sym.Make("z.txt", source, false);
            string[] symLinks = sym.GetSymLinks();
            Assert.Equal(3, symLinks.Length);
            Assert.Contains("x.txt", symLinks);
            Assert.Contains("y.txt", symLinks);
            Assert.Contains("z.txt", symLinks);
            fix.AddSeen("x.txt");
            fix.AddSeen("y.txt");
            fix.AddSeen("z.txt");
        }

        [Fact]
        public void ClearExistingDoesNotDeleteRealFile()
        {
            SymLinkMaker sym = new SymLinkMaker(root, fix.logger);
            sym.ClearExisting();
            Assert.True(File.Exists(Path.Combine(root, "real.txt")));
        }

        [Fact]
        public void ClearExistingClearsCreatedSymLinksandReturnsCorrectDeleteCount()
        {
            SymLinkMaker sym = new SymLinkMaker(root, fix.logger);
            sym.Make("a.txt", source, false);
            sym.Make("b.txt", source, false);
            sym.Make("c.txt", source, false);
            Assert.True(File.Exists(Path.Combine(root, "a.txt")));
            Assert.Equal(3, sym.ClearExisting());
            Assert.False(File.Exists(Path.Combine(root, "a.txt")));
            Assert.False(File.Exists(Path.Combine(root, "b.txt")));
            Assert.False(File.Exists(Path.Combine(root, "c.txt")));
        }

        [Fact]
        public void ClearLinkOnRealFileReturnsFalse()
        {
            SymLinkMaker sym = new SymLinkMaker(root, fix.logger);
            Assert.True(File.Exists(Path.Combine(root, "real.txt")));
            Assert.False(sym.ClearLink("real.txt"));
            Assert.True(File.Exists(Path.Combine(root, "real.txt")));
        }

        [Fact]
        public void ClearLinkOnSymLinkReturnsTrue()
        {
            SymLinkMaker sym = new SymLinkMaker(root, fix.logger);
            sym.Make("temp.txt", source, false);
            Assert.True(sym.ClearLink("temp.txt"));
            Assert.False(File.Exists(Path.Combine(root, "temp.txt")));
        }
    }
}
