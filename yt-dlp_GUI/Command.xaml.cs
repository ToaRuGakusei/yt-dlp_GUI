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
            if (!File.Exists(System.IO.Path.GetTempPath() + "\\" + "cmd_check.txt"))
            {
                File.Create(System.IO.Path.GetTempPath() + "\\" + "cmd_check.txt");
            }
            Load();
        }
         private void Load()
        {
            
            StreamReader load = new StreamReader(System.IO.Path.GetTempPath() + "\\" + "cmd_check.txt");
            string check = load.ReadToEnd();
            load.Close();
            if(check == "true")
            {
                cmd.IsChecked = true;
            }

            if(cmd.IsChecked == false)
            {
                textbox.IsEnabled = false;
            }
            
            StreamReader cmdd = new StreamReader(System.IO.Path.GetTempPath() + "\\" + "cmd.txt");
            string l = cmdd.ReadToEnd();
            string box = l.ToString().Replace("System.Windows.Controls.TextBox", "");
            cmdd.Close();
            textbox.Text = box;
        }
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter sm = new StreamWriter(System.IO.Path.GetTempPath() + "\\" + "cmd.txt", false);
            string box = textbox.ToString().Replace("System.Windows.Controls.TextBox: ", "");
            sm.Write(box);
            sm.Close();
            this.Close();
        }

        private void cmd_Unchecked(object sender, RoutedEventArgs e)
        {
            textbox.IsEnabled = false;
            StreamWriter sm = new StreamWriter(System.IO.Path.GetTempPath() + "\\" + "cmd_check.txt", false);
            sm.Write("false");
            sm.Close();
        }

        private void cmd_Checked(object sender, RoutedEventArgs e)
        {
            textbox.IsEnabled = true;
            StreamWriter sm = new StreamWriter(System.IO.Path.GetTempPath() + "\\" + "cmd_check.txt", false);
            sm.Write("true");
            sm.Close();
        }
    }
}
