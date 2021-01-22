using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace StarDictTools
{
    public class WordEntry : IComparable<WordEntry>
    {
        public string word { get; set; } = "";
        public string content { get; set; } = "";

        public override string ToString()
        {
            return word;
        }

        public static bool IsNullOrEmpty(WordEntry wordEntry)
        {
            return wordEntry == null 
                || string.IsNullOrEmpty(wordEntry.word) 
                || string.IsNullOrEmpty(wordEntry.content);
        }

        public int CompareTo([AllowNull] WordEntry other)
        {
            if (other == null) return 1;
            if (word != other.word) return word.CompareTo(other.word);

            return content.CompareTo(other.content);
        }
    }

    public class SQLiteDBHelper : IDisposable
    {
        public string ConnectionString { get; private set; }
        public string DbFilePath { get; private set; }

        SqliteConnection conn;
        public void InitDb(string filepath)
        {
            //if (File.Exists(filepath))
            //{
            //    throw new IOException($"File exists : {filepath} ");
            //}

            DbFilePath = Path.GetFullPath(filepath);
            ConnectionString = $"Data Source={DbFilePath}";

            conn = new SqliteConnection(ConnectionString);
            {
                conn.Open();

                using var command = conn.CreateCommand();
                // Supposed no duplicated keys
                command.CommandText = @"create table IF NOT EXISTS dict (word text PRIMARY KEY, content text)";
                command.ExecuteNonQuery();

                //command.CommandText = @"create index IDX_Dict on dict (word)";
                //command.ExecuteNonQuery();

                command.CommandText = @"create table IF NOT EXISTS ifo (word varchar(250) PRIMARY KEY, content text)";
                command.ExecuteNonQuery();
            }
        }

        public void InsertDictEntries(List<WordEntry> entries)
        {
            var trans = conn.BeginTransaction();
            entries.ForEach(dict => InsertDictEntry(dict.word, dict.content));
            trans.Commit();
        }

        public void InsertDictEntry(string word, string content)
        {
            if (string.IsNullOrWhiteSpace(word)) return;

            using var command = conn.CreateCommand();
            command.CommandText = @"insert into dict (word, content) values ($word, $content)";
            command.Parameters.AddWithValue("$word", word);
            command.Parameters.AddWithValue("$content", content);
            command.ExecuteNonQuery();
        }

        public WordEntry ReadDictEntry(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return null;

            WordEntry result = null;

            using var command = conn.CreateCommand();
            command.CommandText = @"select word, content from dict WHERE word=$word";
            command.Parameters.AddWithValue("$word", word);
            using var reader = command.ExecuteReader();
            //First
            if(reader.Read())
            {                
                result = new WordEntry { word = word};
                result.content = reader.GetString(1);
            }

            return result;
        }

        public void InsertIfoEntry(string word, string content)
        {
            if (string.IsNullOrWhiteSpace(word)) return;

            using var command = conn.CreateCommand();
            command.CommandText = @"insert into ifo (word, content) values ($word, $content)";
            command.Parameters.AddWithValue("$word", word);
            command.Parameters.AddWithValue("$content", content);
            command.ExecuteNonQuery();
        }

        public WordEntry ReadIfoEntry(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return null;

            WordEntry result = null;

            using var command = conn.CreateCommand();
            command.CommandText = @"select word, content from ifo WHERE word=$word";
            command.Parameters.AddWithValue("$word", word);
            using var reader = command.ExecuteReader();
            //First
            if (reader.Read())
            {
                result = new WordEntry { word = word };
                result.content = reader.GetString(1);
            }

            return result;
        }

        public void UpdateIfoEntry(string word, string content)
        {
            if (string.IsNullOrWhiteSpace(word)) return;

            using var command = conn.CreateCommand();
            command.CommandText = @"update ifo set content=$content where word=$word)";
            command.Parameters.AddWithValue("$word", word);
            command.Parameters.AddWithValue("$content", content);
            command.ExecuteNonQuery();
        }

        public bool IsIfoEntryExists(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return true;

            using var command = conn.CreateCommand();
            command.CommandText = @"select count(*) from ifo WHERE word=$word";
            command.Parameters.AddWithValue("$word", word);
            using var reader = command.ExecuteReader();
            //First
            if (reader.Read())
            {
                var count = reader.GetInt16(0);
                return count > 0;
            }

            return false;
        }

        public void UpdateIfoEntries(List<WordEntry> infos)
        {
            if (infos.Count < 1) return;

            foreach(var info in infos)
            {
                if (IsIfoEntryExists(info.word))
                {
                    UpdateIfoEntry(info.word, info.content);
                }
                else
                {
                    InsertIfoEntry(info.word, info.content);
                }
            }
        }

        public List<WordEntry> ReadDictWords()
        {
            List<WordEntry> result = new List<WordEntry>();

            using var command = conn.CreateCommand();
            command.CommandText = @"select word from dict WHERE 1=1";            
            using var reader = command.ExecuteReader();
            //All
            while (reader.Read())
            {
                var word = reader.GetString(0);
                result.Add(new WordEntry { word = word });
            }

            return result;
        }

        public List<WordEntry> ReadAllIfo()
        {
            List<WordEntry> result = new List<WordEntry>();

            using var command = conn.CreateCommand();
            command.CommandText = @"select word, content from ifo WHERE 1=1";
            using var reader = command.ExecuteReader();
            //All
            while (reader.Read())
            {
                var word = reader.GetString(0);
                var content = reader.GetString(1);
                result.Add(new WordEntry { word = word, content = content });
            }

            return result;
        }

        public static string ParseDbFilePath(string starDictIdxFilepath)
        {
            return starDictIdxFilepath.Substring(0, starDictIdxFilepath.Length - 4) + ".db";
        }

        public void Dispose()
        {
            conn.Close();
            conn.Dispose();
        }
    }
}
