using System;
using System.Collections.Generic;

namespace StarDictTools
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Please supply a StarDict idx file path!");
                return;
            }

            foreach (var arg in args)
            {
                Console.WriteLine(arg);
            }

            var filepath = args[0];
            var files = StarDictParser.ParseFiles(filepath);
            List<WordEntry> infos = StarDictParser.ParseIfo(files);

            List<IdxEntry> indexs = StarDictParser.ParseIdx(files);
            List<WordEntry> dicts = StarDictParser.ParseDict(files, indexs);

            var dbFilePath = SQLiteDBHelper.ParseDbFilePath(files.idx);
            using (SQLiteDBHelper converter = new SQLiteDBHelper())
            {
                converter.InitDb(dbFilePath);

                //transfer entries to db
                // 1. ifo
                infos.ForEach(info => converter.InsertIfoEntry(info.word, info.content));

                // 2. dict
                converter.InsertDictEntries(dicts);
            }

            Console.WriteLine("Done!");

            Console.Write("Press any key to continue ...");
            Console.ReadKey();
        }
    }
}
