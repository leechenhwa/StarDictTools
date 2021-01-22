using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StarDictTools
{
    public class StarDictFiles
    {
        /// <summary>
        /// meta infos for dict, a text file
        /// </summary>
        public string ifo { get; set; }

        /// <summary>
        /// index binary file for dict
        /// </summary>
        public string idx { get; set; }

        /// <summary>
        /// synon
        /// </summary>
        public string syn { get; set; }

        /// <summary>
        /// A zip of dict when downloading; unzipped before accessing.
        /// </summary>
        public string dict_dz { get; set; }

        /// <summary>
        /// the real dict binary file
        /// </summary>
        public string dict { get; set; }

        public static string ext_idx = ".idx";
        public static string ext_ifo = ".ifo";
        public static string ext_syn = ".syn";

        public static string ext_dict_dz = ".dict.dz";
        public static string ext_dict = ".dict";

        public static bool IsIfoFile(FileInfo fileInfo)
        {
            return fileInfo.Exists && fileInfo.FullName.EndsWith(ext_ifo);
        }
      
        public static bool IsIdxFile(FileInfo fileInfo)
        {
            return fileInfo.Exists && fileInfo.FullName.EndsWith(ext_idx);
        }

        public static bool IsSynFile(FileInfo fileInfo)
        {
            return fileInfo.Exists && fileInfo.FullName.EndsWith(ext_syn);
        }
        public static bool IsDictFile(FileInfo fileInfo)
        {
            return fileInfo.Exists && fileInfo.FullName.EndsWith(ext_dict);
        }
        public static bool IsDictDzFile(FileInfo fileInfo)
        {
            return fileInfo.Exists && fileInfo.FullName.EndsWith(ext_dict_dz);
        }

    }
}
