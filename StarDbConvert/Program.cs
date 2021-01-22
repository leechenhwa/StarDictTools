using StarDictTools;
using System;
using System.Collections.Generic;

namespace StarDbConvert
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please supply a StarDict idx file path!");
                return;
            }

            foreach (var arg in args)
            {
                Console.WriteLine(arg);
            }

            var filepath = args[0];
            StarDictParser.ConvertToDb(filepath);

            Console.WriteLine("Done!");

            Console.Write("Press any key to continue ...");
            Console.ReadKey();
        }
    }
}
