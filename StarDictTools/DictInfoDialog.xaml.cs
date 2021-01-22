using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StarDictTools
{
    /// <summary>
    /// DictInfoDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DictInfoDialog : Window
    {
        public DictInfoDialog()
        {
            InitializeComponent();

            if(MainViewModel.Instance != null)
            {
                DataContext = MainViewModel.Instance;
            }
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
