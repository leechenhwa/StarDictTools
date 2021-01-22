using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace StarDictTools
{
    public class StarDictParser
    {
        public static string ConvertToDb(string oneStarDictFilePath)
        {
            var filepath = oneStarDictFilePath;
            var files = ParseFiles(filepath);
            List<WordEntry> infos = ParseIfo(files);

            List<IdxEntry> indexs = ParseIdx(files);
            List<WordEntry> dicts = ParseDict(files, indexs);

            var dbFilePath = SQLiteDBHelper.ParseDbFilePath(files.idx);
            using SQLiteDBHelper converter = new SQLiteDBHelper();
            converter.InitDb(dbFilePath);

            //transfer entries to db
            // 1. ifo
            infos.ForEach(info => converter.InsertIfoEntry(info.word, info.content));

            // 2. dict
            //clear duplicates
            HashSet<string> keys = new HashSet<string>();
            List<WordEntry> duplicates = new List<WordEntry>();
            foreach(var d in dicts)
            {
                if (!keys.Contains(d.word))
                {
                    keys.Add(d.word);
                }
                else
                {
                    duplicates.Add(d);
                }
            }
            foreach(var d in duplicates)
            {
                dicts.Remove(d);
            }

            converter.InsertDictEntries(dicts);

            return dbFilePath;
        }

        public static StarDictFiles ParseFiles(string oneStarDictFilePath)
        {
            var files = new StarDictFiles();

            var file = new FileInfo(oneStarDictFilePath);
            if(file.Exists)
            {
                string filename;
                if (StarDictFiles.IsIdxFile(file) || StarDictFiles.IsIfoFile(file) 
                    || StarDictFiles.IsSynFile(file) || StarDictFiles.IsDictFile(file))
                {
                    filename = Path.GetFileNameWithoutExtension(file.Name);
                }
                else if (StarDictFiles.IsDictDzFile(file))
                {
                    filename = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file.Name));
                } else
                {
                    throw new ArgumentException("Please select one StarDict file, eg. *.idx, *.ifo, *.syn, or *.dict, *.dict.dz");
                }

                if(!string.IsNullOrEmpty(filename))
                {
                    files.idx = Path.Combine(file.DirectoryName, filename + StarDictFiles.ext_idx);
                    files.ifo = Path.Combine(file.DirectoryName, filename + StarDictFiles.ext_ifo);
                    files.syn = Path.Combine(file.DirectoryName, filename + StarDictFiles.ext_syn);

                    files.dict_dz = Path.Combine(file.DirectoryName, filename + StarDictFiles.ext_dict_dz);
                    files.dict = Path.Combine(file.DirectoryName, filename + StarDictFiles.ext_dict);

                   if(!(File.Exists(files.dict_dz) || File.Exists(files.dict)))
                    {
                        throw new ArgumentException("StarDict data files not found eg. *.dict or *.dict.dz");
                    }

                    PrepareDict(files);
                }
            }

            return files;
        }

        public static List<WordEntry> ParseDict(StarDictFiles files, List<IdxEntry> indexs)
        {
            List<WordEntry> result = new List<WordEntry>();
            using BinaryReader reader = new BinaryReader(File.OpenRead(files.dict));
            indexs.ForEach(index =>
            {
                reader.BaseStream.Seek(index.offset, SeekOrigin.Begin);
                byte[] dictBytes = reader.ReadBytes((int)index.length);
                var content = Encoding.UTF8.GetString(dictBytes);
                result.Add(new WordEntry { word = index.word, content = content });
            });

            return result;
        }

        public static List<IdxEntry> ParseIdx(StarDictFiles files)
        {
            List<IdxEntry> result = new List<IdxEntry>();
            using BinaryReader reader = new BinaryReader(File.OpenRead(files.idx));

            byte temp;
            int index = 0;
            byte[] buffer = new byte[1024];            
            bool start = true;
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                temp = reader.ReadByte();
                buffer[index++] = temp;

                if (temp == 0 && !start)
                {                    
                    var offset = htonl(reader.ReadUInt32());
                    var length = htonl(reader.ReadUInt32());

                    // make word
                    byte[] wb = new byte[--index];
                    for (int i = 0; i < index; i++)
                    {
                        wb[i] = buffer[i];
                    }
                    var word = Encoding.UTF8.GetString(wb);
                    result.Add(new IdxEntry { word = word, offset = offset, length = length });
                   
                    index = 0;
                    start = true;
                }
                else if (temp != 0)
                {
                    start = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 大小端转换. <br/>
        /// The htonl function takes a 32-bit number in host byte order and 
        /// returns a 32-bit number in the network byte order used in TCP/IP 
        /// networks (the AF_INET or AF_INET6 address family).
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private static UInt32 htonl(UInt32 src)
        {
            byte[] bytes = BitConverter.GetBytes(src);
            byte temp = bytes[0];
            bytes[0] = bytes[3];
            bytes[3] = temp;

            temp = bytes[1];
            bytes[1] = bytes[2];
            bytes[2] = temp;
            return BitConverter.ToUInt32(bytes, 0);
        }



        public static List<WordEntry> ParseIfo(StarDictFiles files)
        {
            List<WordEntry> result = new List<WordEntry>();
            var infoFilePath = files.ifo;

            var lines = File.ReadAllLines(infoFilePath);
            foreach(var line in lines)
            {
                var index = line.IndexOf('=');
                if (index < 0) continue;

                var word = line.Substring(0, index);
                var content = line.Substring(index + 1);
                result.Add(new WordEntry { word = word, content = content });
            }

            return result;
        }

        public static void PrepareDict(StarDictFiles files)
        {
            if (!File.Exists(files.dict_dz))
            {
                return;
            }

            if (File.Exists(files.dict))
            {
                return;
            }

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                //Note: For Windows
                var tool = Path.GetFullPath(@"tools\7z.exe");
                var process = Process.Start(tool, $" e \"{files.dict_dz}\" -o\"{Path.GetDirectoryName(files.dict_dz)}\"");
                process.WaitForExit();
            }
            else
            {
                var targetDir = Path.GetDirectoryName(files.dict_dz);
                // SharpCompress: Use ReaderFactory to autodetect archive type and Open the entry stream
                using Stream stream = File.OpenRead(files.dict_dz);
                using var reader = ReaderFactory.Open(stream);
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        reader.WriteEntryToDirectory(targetDir, new SharpCompress.Common.ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
        }
    }

    /*
        struct worditem_t {
	        std::string word;
	        guint32 offset;
	        guint32 size;
        };

        struct synitem_t {
	        std::string word;
	        guint32 index;
        };
     */
    public class IdxEntry
    {
        public string word { get; set; }

        public uint offset { get; set; }
        public uint length { get; set; }

        public override string ToString()
        {
            return $"Idx_{word}_{offset}_{length}";
        }
    }
}
