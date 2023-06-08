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
            string approx = "WHERE (LOWER(Filename) GLOB ?)";
            var filter = new FileSearchFilter().SetFilenameFilter("test");
            string where = "";
            List<object> values = new List<object>();
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(exact, where);
            where = "";
            filter.Reset().SetFilenameFilter("test", false);
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(approx, where);
            Assert.Contains("*test*", values);
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
            string approx = "WHERE (LOWER(Path || '\\' || Filename) GLOB ?)";
            var filter = new FileSearchFilter().SetFullnameFilter("test");
            string where = "";
            List<object> values = new List<object>();
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(exact, where);
            where = "";
            filter.Reset().SetFullnameFilter("test", false);
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(approx, where);
            Assert.Contains("*test*", values);
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

        [Fact]
        public void SubFiltersBuildProperly()
        {
            string expected = "WHERE ( ( (Filename = ?) AND (PathID = ?)) OR ( (Path = ?) OR (Size >= ?)))";
            var filter1 = new FileSearchFilter().SetFilenameFilter("f");
            var filter2 = new FileSearchFilter().SetPathIDFilter(1);
            var filter3 = new FileSearchFilter().SetPathFilter("p");
            var filter4 = new FileSearchFilter().SetSizeFilter(100, false);
            var filterA = new FileSearchFilter().AddSubfilter(filter1).AddSubfilter(filter2);
            var filterB = new FileSearchFilter().AddSubfilter(filter3).AddSubfilter(filter4);
            filter4.IsOr = true;
            filterB.IsOr = true;
            var filter = new FileSearchFilter().AddSubfilter(filterA).AddSubfilter(filterB);
            string where = "";
            var values = new List<object>();
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(expected, where);
        }

        [Fact]
        public void FilterIsOrAppliedProperly()
        {
            string expected = "WHERE ( (Filename = ? AND Path = ?) OR (PathID = ?))";
            var filter1 = new FileSearchFilter().SetFilenameFilter("f")
                                .SetPathFilter("p").SetOr(true);
            var filter2 = new FileSearchFilter().SetPathIDFilter(1).SetOr(true);
            var filter = new FileSearchFilter().AddSubfilter(filter1).AddSubfilter(filter2);
            string where = "";
            var values = new List<object>();
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(expected, where);
        }

        [Fact]
        public void TagFiltersBuildProperly()
        {
            string expected = "WHERE (? IN (SELECT TagID FROM FileTagAssociations WHERE FileID=Files.ID) " +
                "AND ? IN (SELECT TagID FROM FileTagAssociations WHERE FileID=Files.ID))";
            string expectedWithOther = "WHERE (Path = ? AND " +
                "(? IN (SELECT TagID FROM FileTagAssociations WHERE FileID=Files.ID) AND " +
                "? IN (SELECT TagID FROM FileTagAssociations WHERE FileID=Files.ID)))";
            var filter = new FileSearchFilter().SetTagFilter(new List<int>() { 1, 2 }, true).SetOr(true);
            string where = "";
            var values = new List<object>();
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(expected, where);
            where = "";
            filter.SetPathFilter("p");
            filter.BuildWhereStatementPart(ref where, ref values);
            Assert.Equal(expectedWithOther, where);
        }
    }
}
