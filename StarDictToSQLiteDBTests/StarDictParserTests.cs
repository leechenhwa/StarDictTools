using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarDictTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StarDictTools.Tests
{
    [TestClass()]
    public class StarDictParserTests
    {
        [TestMethod()]
        public void ParseFilesTest()
        {
            var path = @"E:\0src\dictionary\stardict-dicts\spanish\stardict-es-es_Moliner-2.4.2\es-es_Moliner.idx";
            var files = StarDictParser.ParseFiles(path);
            Assert.IsTrue(File.Exists(files.dict_dz));
            Assert.IsTrue(File.Exists(files.dict));
        }

        [TestMethod()]
        public void ParseIfoTest()
        {
            var path = @"E:\0src\dictionary\stardict-dicts\spanish\stardict-es-es_Moliner-2.4.2\es-es_Moliner.idx";
            var files = StarDictParser.ParseFiles(path);
            var entries = StarDictParser.ParseIfo(files);
            Assert.IsTrue(entries.Count > 0);
        }

        [TestMethod()]
        public void ParseIdxTest()
        {
            var path = @"E:\0src\dictionary\stardict-dicts\spanish\stardict-es-es_Moliner-2.4.2\es-es_Moliner.idx";
            var files = StarDictParser.ParseFiles(path);
            var entries = StarDictParser.ParseIdx(files);
            Assert.IsTrue(entries.Count > 0);

            var dicts = StarDictParser.ParseDict(files, entries);
            Assert.IsTrue(dicts.Count > 0);
        }

        [TestMethod()]
        public void ConvertToDbTest()
        {
            //TODO: duplicates
            var path = @"E:\0src\dictionary\stardict-dicts\spanish\stardict-es-en_Babylon-2.4.2\Spanish-English_Babylon.idx";
            var dbPath = StarDictParser.ConvertToDb(path);
            Assert.IsTrue(File.Exists(dbPath));
        }
    }
}