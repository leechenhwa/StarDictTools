using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Data;

namespace StarDictTools
{
    class ViewModels
    {
    }

    public class MyViewModelBase : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }

        // Example
        //private string name;
        //public string PersonName
        //{
        //    get { return name; }
        //    set
        //    {
        //        name = value;
        //        // Call OnPropertyChanged whenever the property is updated
        //        OnPropertyChanged();
        //    }
        //}

        // Example shorter version
        //private string name;
        //public string PersonName { get => name; set { name = value; OnPropertyChanged(); } }
    }

    public class MainViewModel : MyViewModelBase
    {
        public static MainViewModel Instance { get; private set; }

        private string dbPath;
        public string DbPath { get => dbPath; set { dbPath = value; loadDb(); OnPropertyChanged(); } }

        private string searchWord;
        public string SearchWord { get => searchWord; set { searchWord = value; OnPropertyChanged(); } }

        private string infoContent;
        public string InfoContent { get => infoContent; set { infoContent = value; OnPropertyChanged(); } }

        private WordEntry currentWord;
        public WordEntry CurrentWord { get => currentWord; set { currentWord = value; OnPropertyChanged(); } }

        public ObservableCollection<WordEntry> Dict { get; private set; } = new ObservableCollection<WordEntry>();
       
        public List<WordEntry> Info { get; private set; } = new List<WordEntry>();

        private SQLiteDBHelper db = new SQLiteDBHelper();

        public MainViewModel()
        {
            if(Instance == null)
            {
                Instance = this;
            }
        }

        private void loadDb()
        {
            if (!File.Exists(DbPath)) return;
            db.InitDb(DbPath);

            Info.Clear();
            Dict.Clear();

            Info.AddRange(db.ReadAllIfo());
            InfoContent = MakeInfoContent(Info);

            var dict = db.ReadDictWords();
            dict.ForEach(d => Dict.Add(d));
        }

        private string contentHead = "<!DOCTYPE html><html><head><meta charset=\"UTF-8\"></head><body>";        
        private string contentFoot = "</body></html>";

        private string contentHead1 = "<!DOCTYPE html><html><head><meta charset=\"UTF-8\">";
        private string contentHead2 = "</head><body>";
        private string contentHeadScriptFix = "<script>/*Disable Javascript error in WPF WebBrowser control*/function noError(){return true;} window.onerror=noError;</script>";

        private string MakeInfoContent(List<WordEntry> info)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(contentHead);
            sb.Append("<main class=\"dict-entry\">");
            sb.Append("<dl>");

            info.ForEach(f =>
            {
                sb.Append($"<dt>{f.word}</dt>");
                sb.Append($"<dd>{f.content}</dd>");
            });

            sb.Append("</dl>");
            sb.Append("</main>");
            sb.Append(contentFoot);
            return sb.ToString();
        }

        public string MakeContent(WordEntry wordEntry)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(contentHead1);
            sb.Append(contentHeadScriptFix);
            sb.Append(contentHead2);
            sb.Append("<main class=\"dict-entry\">");

            if (string.IsNullOrEmpty(wordEntry.content))
            {
                //try read db
                var entry = db.ReadDictEntry(wordEntry.word);
                if (wordEntry.CompareTo(entry) != 0 && !string.IsNullOrEmpty(entry.content))
                {
                    wordEntry.content = entry.content;
                }
            }

            sb.Append(wordEntry.content);
            sb.Append("</main>");            
            sb.Append(contentFoot);
            return sb.ToString();
        }

        public WordEntry Search(string searchKey)
        {
            if (string.IsNullOrWhiteSpace(searchKey)) return null;

            foreach(var entry in Dict)
            {
                if (entry.word.StartsWith(searchKey))
                {
                    return entry;
                }
            }

            return null;
        }

        public string MakeJson(WordEntry wordEntry)
        {
            return $"{{\r\n word:\r\n {wordEntry.word},\r\n entry:\r\n {wordEntry.content}  }}";
        }

        public void SaveInfo()
        {
            db.UpdateIfoEntries(Info);
        }
    }
}
