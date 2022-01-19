using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows;

namespace youtube_dl_GUI
{
    /// <summary>
    /// setting.xaml の相互作用ロジック
    /// </summary>
    public partial class setting : Window
    {
        public setting()
        {
            InitializeComponent();
            load();
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true

            };
            // ダイアログを表示
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                StreamWriter sm = new StreamWriter(Path.GetTempPath() + "\\" + "Path.txt", false);
                sm.Write($"{dialog.FileName}");
                sm.Close();
                save.Text = "";
                load();
            }

        }
        private void load()
        {
            try
            {
                StreamReader sr = new StreamReader(Path.GetTempPath() + "\\" + "Path.txt");
                string path = sr.ReadToEnd();
                save.Text = path;
                sr.Close();

                StreamReader sr2 = new StreamReader(Path.GetTempPath() + "\\" + "switch.txt");
                string mkv_mp4 = sr2.ReadToEnd();
                if (mkv_mp4 == "mp4")
                {
                    mp4.IsChecked = true;
                }
                else if (mkv_mp4 == "mkv")
                {
                    mkv.IsChecked = true;
                }
                sr2.Close();

                StreamReader sr1 = new StreamReader(Path.GetTempPath() + "\\" + "toggle.txt");

                string toggle = sr1.ReadToEnd();
                sr1.Close();
                if (toggle != "false")
                {
                    option.IsEnabled = true;
                    Toggle1.IsOn = true;
                    StreamReader st = new StreamReader(Path.GetTempPath() + "\\" + "cmd.txt");
                    string cmd = st.ReadToEnd();
                    option.Text = cmd;
                    st.Close();
                }
                else
                {
                    option.IsEnabled = false;
                    Toggle1.IsOn = false;
                    //StreamReader st = new StreamReader(Path.GetTempPath() + "\\" + "cmd.txt");
                    //string cmd = st.ReadToEnd();
                    //option.Text = cmd;
                    //st.Close();
                }

            }
            catch(Exception)
            {

            }
            

            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(Toggle1.IsOn == true)
            {
                StreamWriter sm1 = new StreamWriter(Path.GetTempPath() + "\\" + "cmd.txt", false);
                string cmd = option.Text;
                sm1.Write(cmd);
                sm1.Close();
            }
            
            StreamWriter sm2 = new StreamWriter(Path.GetTempPath() + "\\" + "switch.txt", false);
            
            if(mp4.IsChecked == true)
            {
                sm2.Write("mp4");
            }else if(mkv.IsChecked == true)
            {
                sm2.Write("mkv");
            }
            
            sm2.Close();
            this.Close();
        }



        private void Toggle1_Toggled(object sender, RoutedEventArgs e)
        {
            if (Toggle1.IsOn == true)
            {
                option.IsEnabled = true;
                StreamWriter sm = new StreamWriter(Path.GetTempPath() + "\\" + "toggle.txt", false);
                sm.Write("true");
                sm.Close();
            }
            else
            {
                option.IsEnabled = false;

                File.Delete(Path.GetTempPath() + "\\" + "toggle.txt");
            }
        }
    }
}
