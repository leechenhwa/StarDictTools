using Microsoft.VisualStudio.TestTools.UnitTesting;
using StarDictTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarDictTools.Tests
{
    [TestClass()]
    public class SQLiteDBConverterTests
    {
        [TestMethod()]
        public void InitDbTest()
        {
            var filepath = @"E:\0src\dictionary\stardict-dicts\spanish\stardict-es-es_Moliner-2.4.2\es-es_Moliner.idx";
            var files = StarDictParser.ParseFiles(filepath);

            var dbFilePath = SQLiteDBHelper.ParseDbFilePath(files.idx);
            using SQLiteDBHelper converter = new SQLiteDBHelper();
            converter.InitDb(dbFilePath);

            var word1 = new WordEntry { word = "hello", content = "a welcome!" };
            converter.InsertDictEntry(word1.word, word1.content);
            var w1 = converter.ReadDictEntry(word1.word);
            Assert.IsTrue(word1.CompareTo(w1) == 0);

            var info1 = new WordEntry { word = "name", content = "a magic thing" };
            converter.InsertIfoEntry(info1.word, info1.content);
            var i1 = converter.ReadIfoEntry(info1.word);
            Assert.IsTrue(info1.CompareTo(i1) == 0);
        }
    }
}