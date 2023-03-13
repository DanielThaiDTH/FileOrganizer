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

namespace FileDBManager.Test
{
    public class FileSearchFilterTest
    {
        [Fact]
        public void ResetMakesEmpty()
        {
            var filter = new FileSearchFilter().SetAltnameFilter("test").Reset();
            Assert.True(filter.IsEmpty);
        }

        [Fact]
        public void FilenameFiltersBuildProperly()
        {
            string exact = "WHERE (Filename = ?)";
            string approx = "WHERE (Filename LIKE ?)";
            var filter = new FileSearchFilter().SetFilenameFilter("test");
            string where = "";
            List<object> values = new List<object>();
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(exact, where);
            where = "";
            filter.Reset().SetFilenameFilter("test", false);
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(approx, where);
            Assert.Contains("%test%", values);
        }

        [Fact]
        public void IDFiltersBuildProperly()
        {
            string idWhere = "WHERE (Files.ID = ?)";
            string pathIDWhere = "WHERE (PathID = ?)";
            var filter = new FileSearchFilter().SetIDFilter(1);
            string where = "";
            var values = new List<object>();
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(idWhere, where);
            filter.Reset().SetPathIDFilter(1);
            where = "";
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(pathIDWhere, where);
        }

        [Fact]
        public void FullnameFiltersBuildProperly()
        {
            string exact = "WHERE (Path || '\\' || Filename = ?)";
            string approx = "WHERE (Path || '\\' || Filename LIKE ?)";
            var filter = new FileSearchFilter().SetFullnameFilter("test");
            string where = "";
            List<object> values = new List<object>();
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(exact, where);
            where = "";
            filter.Reset().SetFullnameFilter("test", false);
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(approx, where);
            Assert.Contains("%test%", values);
        }

        [Fact]
        public void SizeFiltersBuildProperly()
        {
            string lesser = "WHERE (Size <= ?)";
            string greater = "WHERE (Size >= ?)";
            var filter = new FileSearchFilter().SetSizeFilter(100, true);
            string where = "";
            var values = new List<object>();
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(lesser, where);
            where = "";
            filter.Reset().SetSizeFilter(100, false);
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(greater, where);
        }

    }
}
