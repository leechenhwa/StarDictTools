using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StarDictTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DisplayHelp();
        }

        private void DisplayHelp()
        {
            if (!string.IsNullOrEmpty(this.viewModel.DbPath)) return;

            string usage = @"<h1>How to use</h1>
            <ol>
            <li>Drag a StarDict DB file to the toolbar to load it.</li>
            <li>or Drag a StarDict IDX file to the toolbar to convert it to SQLite DB or load it if already converted.</li>
            </ol>
                ";
            string usage_zh = @"<h1>用法说明</h1>
            <ol>
            <li>拖放一个 StarDict DB 文件到工具栏上即可。</li>
            <li>或拖放一个 StarDict IDX 文件到工具栏上， 即可转换成 SQLite DB 文件或调入已经转换好的。</li>
            </ol>
                ";
            WordEntry help = new WordEntry { word = "help", content = usage + usage_zh };
            this.contentView.NavigateToString(this.viewModel.MakeContent(help));
        }

        private void ToolBar_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);

                var content = files[0];
                FileInfo fileInfo = new FileInfo(content);
                if (fileInfo != null)
                {
                    string extension = System.IO.Path.GetExtension(content);
                    switch (extension)
                    {
                        case ".db":
                            {// If is db file
                                this.viewModel.DbPath = content;
                            }
                            break;
                        case ".idx":
                            {
                                var dbPath = content.Substring(0, content.Length - 4) + ".db";
                                if (!File.Exists(dbPath))
                                {
                                    dbPath = StarDictParser.ConvertToDb(content);
                                }

                                //recheck
                                if (!File.Exists(dbPath))
                                {
                                    MessageBox.Show($"Please check if db file exists : {System.IO.Path.GetFileName(dbPath)} \r\nor\r\nTry to repair the dict file name.", "Error");
                                }

                                this.viewModel.DbPath = dbPath;
                            }
                            break;
                    }

                }

            }
        }

        private void infoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.viewModel.InfoContent)) return;
            this.contentView.NavigateToString(this.viewModel.InfoContent);
        }

        private void wordList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = this.wordList.SelectedItem as WordEntry;
            this.contentView.NavigateToString(this.viewModel.MakeContent(selectedItem));
            this.wordLabel.Text = selectedItem.word;
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            WordEntry result = viewModel.Search(this.searchBox.Text);
            if (result == null) return;

            var selectedItem = this.wordList.SelectedItem as WordEntry;
            var pos = result.CompareTo(selectedItem);
            if ( pos != 0)
            {
                this.wordList.SelectedItem = result;

                //little trick
                var viewIndex = this.wordList.SelectedIndex + (pos > 0 ? 1 : -1) * 5;
                if (viewIndex < 0) viewIndex = 0;
                if (viewIndex >= this.viewModel.Dict.Count) viewIndex = this.viewModel.Dict.Count - 1;
                var viewItem = this.viewModel.Dict[viewIndex];
                this.wordList.ScrollIntoView(viewItem);
            }
        }

        private void jsonBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = this.wordList.SelectedItem as WordEntry;
            if (selectedItem == null) return;

            var json = this.viewModel.MakeJson(selectedItem);
            MessageBox.Show(json, $"JSON string of {selectedItem.word}");
        }

        private void infoEditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.viewModel.DbPath))
            {
                MessageBox.Show("Please select a DB file first!", "Error");
                return;
            }

            DictInfoDialog dialog = new DictInfoDialog();
            var rez = dialog.ShowDialog();
            if(rez.GetValueOrDefault())
            {
                this.viewModel.SaveInfo();
            }
        }

        private void rawCopyBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = this.wordList.SelectedItem as WordEntry;
            if (selectedItem == null) return;

            var txt = selectedItem.word + "\r\n\r\n" + selectedItem.content;
            Clipboard.SetText(txt);
        }
    }
}
