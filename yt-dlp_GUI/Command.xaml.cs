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
using System.Windows.Shapes;

namespace yt_dlp_GUI
{
    /// <summary>
    /// Command.xaml の相互作用ロジック
    /// </summary>
    public partial class Command : Window
    {
        public Command()
        {
            InitializeComponent();
            Load();
        }
         private void Load()
        {
            StreamReader cmd = new StreamReader(System.IO.Path.GetTempPath() + "\\" + "cmd.txt");
            string l = cmd.ReadToEnd();
            textbox.Text = l;
        }
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter sm = new StreamWriter(System.IO.Path.GetTempPath() + "\\" + "cmd.txt", false);
            string box = textbox.ToString().Replace("System.Windows.Controls.TextBox: ", "");
            sm.Write(box);
            sm.Close();
            this.Close();
        }
    }
}
