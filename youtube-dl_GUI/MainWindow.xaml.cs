
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using Microsoft.Web.WebView2.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows;
using System.Windows.Controls;

namespace youtube_dl_GUI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        private string URl;
        public Match match;
        public System.Net.WebClient wc = new System.Net.WebClient();
        public System.IO.StreamReader Sm;
        public HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
        public HtmlAgilityPack.HtmlDocument Doc = new HtmlAgilityPack.HtmlDocument();
        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
            updateChecker();
            cancel.IsEnabled = false;
            load();
            



        }
        private async Task InitializeAsync()
        {

            await wv.EnsureCoreWebView2Async(null); // CoreWebView2初期化待ち
            wv.CoreWebView2.NewWindowRequested += NewWindowRequested;
        }
        private void NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            //新しいウィンドウを開かなくする
            e.Handled = true;

            //元々のWebView2でリンク先を開く
            wv.CoreWebView2.Navigate(e.Uri);
        }
        private void updateChecker()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            WebClient wc = new WebClient();
            StreamReader sm = new StreamReader(wc.OpenRead("https://toaru-web.net/2022/01/06/yt-dlp_gui/"));
            string html = sm.ReadToEnd();

            HtmlAgilityPack.HtmlDocument Doc = new HtmlAgilityPack.HtmlDocument();
            Doc.LoadHtml(html);
            var update = Doc.DocumentNode.SelectSingleNode("//p[@class=\"update\"]");
            string url = update.InnerText;
            string pattern = "[正式版（）更新日]";
            string replacement = "";
            Regex regEx = new Regex(pattern);
            string sanitized = Regex.Replace(regEx.Replace(url, replacement), @"\s+", " ");
            DateTime time1 = DateTime.Parse("2022/02/17 18:19:00");
            DateTime time2 = DateTime.Parse(sanitized);
            if (time1.Date < time2.Date)
            {
                MessageBoxResult msResult = MessageBox.Show("新しいバージョンがあります！\n確認しますか？", "アップデート", MessageBoxButton.YesNo);
                if (msResult == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("https://toaru-web.net/2022/01/06/yt-dlp_gui/");

                }
            }
            else
            {

            }



        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            mp4_mkv();
            soundonoff();
            if (u.Text.Contains("https"))
            {
                que.Items.Add(new { url = u.Text, Size = "" ,ETA = ""});

            }
            
            run.IsEnabled = false;
            cancel.IsEnabled = true;

            Task task = Task.Run(() =>
            {
                Download();
            });



        }

        
        private void list()
        {
            Task task = Task.Run(() =>
            {
                if (que.Items.Count > 0)
                {
                    Download();
                }
            });
        }
        private ProcessStartInfo si;
        private string cmd;
        public int i = 0;
        public int n = 0;
        public string title;
        private void Download()
        {
            StreamReader sm = new StreamReader(Path.GetTempPath() + "\\" + "Path.txt");
            string saveFolder = sm.ReadToEnd();
            StreamReader sm3 = new StreamReader(Path.GetTempPath() + "\\" + "switch.txt");
            string mp4_mkv = sm3.ReadToEnd();
            sm.Close();
            sm3.Close();
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (que.Items.Count > 0)
                {
                    int i = 0;
                    foreach (var item in que.Items)
                    {
                        if (i == 0)
                        {
                            //string t = item.ToString();
                            u.Text = DataBinder.Eval(item, "url").ToString();
                            si = new ProcessStartInfo(@".\yt-dlp.exe", $"--format bestvideo[ext=mp4]+bestaudio[ext=m4a] --embed-subs --embed-thumbnail --all-subs --merge-output-format {mp4_mkv} --all-subs --embed-subs --embed-thumbnail --xattrs --add-metadata -ciw -o \"{saveFolder}\\%(title)s\" {item} ");
                        }

                        i++;
                    }


                }
                else
                {
                    if (File.Exists(Path.GetTempPath() + "\\" + "cmd.txt"))
                    {
                        StreamReader sm2 = new StreamReader(Path.GetTempPath() + "\\" + "cmd.txt");

                        //string toggle = sm1.ReadToEnd();
                        cmd = sm2.ReadToEnd();
                        sm2.Close();
                    }

                    if (File.Exists(Path.GetTempPath() + "\\" + "toggle.txt"))
                    {
                        si = new ProcessStartInfo(@".\yt-dlp.exe", $"{cmd} {URl} ");
                    }
                    else
                    {
                        si = new ProcessStartInfo(@".\yt-dlp.exe", $"--format bestvideo[ext=mp4]+bestaudio[ext=m4a] --embed-subs --embed-thumbnail --all-subs --merge-output-format {mp4_mkv} --all-subs --embed-subs --embed-thumbnail --xattrs --add-metadata -ciw -o \"{saveFolder}\\%(title)s\" {URl} ");
                    }

                }

            }));

            // ウィンドウ表示を完全に消したい場合
            si.CreateNoWindow = true;
            si.RedirectStandardError = true;
            si.RedirectStandardOutput = true;
            si.UseShellExecute = false;
            using (var proc = new Process())
            using (var ctoken = new CancellationTokenSource())
            {

                proc.EnableRaisingEvents = true;
                proc.StartInfo = si;
                // コールバックの設定
                proc.Exited += (s, ev) =>
                {
                    Console.WriteLine($"exited");
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        run.IsEnabled = true;
                        prog.Value = 0;
                        prog_label.Content = "0%";
                        if (que.Items.Count > 0)
                        {
                            que.Items.RemoveAt(0);
                            list();
                        }
                        if (que.Items.Count == 0)
                        {
                            soundplayer();
                            MessageBox.Show("ダウンロードが終了しました。", "終了", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                    }));
                    // プロセスが終了すると呼ばれる
                    ctoken.Cancel();
                };
                // プロセスの開始
                proc.Start();
                Task.WaitAll(
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            var l = proc.StandardOutput.ReadLine();
                            if (l == null)
                            {
                                break;
                            }
                            try
                            {
                                this.Dispatcher.Invoke((Action)(() =>
                                {
                                    string youtubelive = "";
                                    if (l.Contains("youtube_live_chat"))
                                    {
                                        youtubelive = l.ToString();
                                    }
                                    Match GiB = Regex.Match(l, @"\d{0,999999}.\d{0,999999}(GiB)", RegexOptions.IgnoreCase);
                                    Match MiB = Regex.Match(l, @"\d{0,999999}.\d{0,999999}(MiB)+(?!/)", RegexOptions.IgnoreCase);
                                    Match ETA = Regex.Match(l, @"ETA \d{0,999999}\d{0,999999}:\d{0,999999}\d{0,999999}", RegexOptions.IgnoreCase);
                                    object M = MiB.Value;
                                    object G = GiB.Value;
                                    

                                    if (l.Contains("[download] Destination"))
                                    {

                                        title = l.Replace("[download] Destination: ", "");
                                        title = System.IO.Path.GetFileNameWithoutExtension(title);
                                        title = System.IO.Path.GetFileNameWithoutExtension(title);
                                        //title = title.Replace(".webp", "");
                                    }
                                    
                                    if (MiB.Value.Contains("MiB") && n == 0 && !youtubelive.Contains("youtube_live_chat"))
                                    {
                                        List<object> lis = new List<object>();
                                        que.Items.RemoveAt(0);
                                        foreach (var _item in que.Items)
                                        {

                                            lis.Add(_item);
                                            Debug.WriteLine($"list\n{_item}");
                                        }
                                        que.Items.Clear();

                                        que.Items.Add((new { url = title, Size = M, ETA = ETA.Value }));
                                        foreach (var _item in lis)
                                        {
                                            Debug.WriteLine(_item);
                                            que.Items.Add(_item);
                                        }




                                        n++;
                                    }
                                    else if (GiB.Value.Contains("GiB") && n == 0 && !youtubelive.Contains("youtube_live_chat"))
                                    {
                                        List<object> lis = new List<object>();
                                        que.Items.RemoveAt(0);
                                        foreach (var _item in que.Items)
                                        {

                                            lis.Add(_item);
                                            Debug.WriteLine($"list\n{_item}");
                                        }
                                        que.Items.Clear();

                                        que.Items.Add((new { url = title, Size = G, ETA = ETA.Value }));
                                         foreach (var _item in lis)
                                         {
                                            Debug.WriteLine(_item);
                                            que.Items.Add(_item);
                                         }


                                        n++;
                                    }



                                    Debug.WriteLine(l);
                                    string per = l.Substring(0, l.IndexOf("%") + 1);
                                    if (per.Contains("%"))
                                    {
                                        per = per.Replace("[download] ", "");
                                        per = per.Replace("%", "");
                                        double perc = double.Parse(per);
                                        prog.Maximum = 100;
                                        prog.Minimum = 0;
                                        prog.Value = perc;
                                        prog_label.Content = perc + "%";
                                    }

                                    label1.Content = ($"{l}");
                                }));
                            }
                            catch (Exception)
                            {

                            }
                        }
                        n = 0;

                    }),
                    Task.Run(() =>
                    {
                        ctoken.Token.WaitHandle.WaitOne();
                        proc.WaitForExit();
                    })
                );
            }





        }

        private void soundplayer()
        {
            if(toggle1.IsOn == true)
            {
                //オーディオリソースを取り出す
                System.IO.Stream strm = Properties.Resources.end;
                //同期再生する
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(strm);
                player.Play();
                //後始末
                player.Dispose();
            }
            else
            {

            }
            ;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process[] ps =
System.Diagnostics.Process.GetProcessesByName("yt-dlp");

            foreach (System.Diagnostics.Process p in ps)
            {
                //プロセスを強制的に終了させる
                p.Kill();
            }
            que.Items.Clear();
            MessageBox.Show("キャンセルされました。", "Infomation", MessageBoxButton.OK, MessageBoxImage.Information);
            run.IsEnabled = true;
        }


        private void HelpItem_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://toaru-web.net/2022/01/06/yt-dlp_gui/";
            OpenUrl(url);
        }

        private Process OpenUrl(string url)
        {
            return Process.Start(url);
        }

        private void Q_Click(object sender, RoutedEventArgs e)
        {
            if (u.Text.Contains("https"))
            {
                que.Items.Add(new { url = u.Text, Size = "" });

            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            que.Items.Clear();
        }

        private void que_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
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
                load2();
            }
        }
        private void load2()
        {

           

        }
        private void load()
        {
            if (!File.Exists(Path.GetTempPath() + "\\" + "sound.txt"))
            {
                FileStream fs = File.Create(Path.GetTempPath() + "\\" + "sound.txt");
                fs.Close();
            }

            StreamReader so = new StreamReader(Path.GetTempPath() + "\\" + "sound.txt");
                string sound = so.ReadToEnd();
                if (sound == "on")
                {
                    toggle1.IsOn = true;
                }
                else if (sound == "off")
                {
                    toggle1.IsOn = false;
                }
                so.Close();

                if (!File.Exists(Path.GetTempPath() + "\\" + "Path.txt"))
                {
                    FileStream fs = File.Create(Path.GetTempPath() + "\\" + "Path.txt");
                    fs.Close();
                }
                StreamReader sr = new StreamReader(Path.GetTempPath() + "\\" + "Path.txt");
                string path = sr.ReadToEnd();
                save.Text = path;
                sr.Close();

                if (!File.Exists(Path.GetTempPath() + "\\" + "switch.txt"))
                {
                    FileStream fs = File.Create(Path.GetTempPath() + "\\" + "switch.txt");
                    fs.Close();
                }
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

                if (!File.Exists(Path.GetTempPath() + "\\" + "toggle.txt"))
                {
                    FileStream fs = File.Create(Path.GetTempPath() + "\\" + "toggle.txt");
                    fs.Close();
                }
                StreamReader sr1 = new StreamReader(Path.GetTempPath() + "\\" + "toggle.txt");

                string toggle = sr1.ReadToEnd();
                sr1.Close();
                if (!File.Exists(Path.GetTempPath() + "\\" + "cmd.txt"))
                {
                    FileStream fs = File.Create(Path.GetTempPath() + "\\" + "cmd.txt");
                    fs.Close();
                }


            
            



        }
        private void mp4_mkv()
        {
            

            StreamWriter sm2 = new StreamWriter(Path.GetTempPath() + "\\" + "switch.txt", false);

            if (mp4.IsChecked == true)
            {
                sm2.Write("mp4");
            }
            else if (mkv.IsChecked == true)
            {
                sm2.Write("mkv");
            }

            sm2.Close();

        }

        private void soundonoff()
        {

            try
            {
                StreamWriter soundWriter = new StreamWriter(Path.GetTempPath() + "\\" + "sound.txt", false);

                if (toggle1.IsOn == true)
                {
                    soundWriter.Write("on");
                }
                else if (toggle1.IsOn == false)
                {
                    soundWriter.Write("off");
                }

                soundWriter.Close();
            }catch(Exception)
            {

            }            

        }

        private void mp4_Checked(object sender, RoutedEventArgs e)
        {
           
        }

        private void mkv_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void advance_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void Grid_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        private void toggle1_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
          
        }

        private void toggle1_Toggled(object sender, RoutedEventArgs e)
        {
            soundonoff();
        }

        private void video_Click(object sender, RoutedEventArgs e)
        {
            u.Text = wv.CoreWebView2.Source;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            wv.GoBack();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            wv.CoreWebView2.Stop();//中止
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            wv.Reload();//再読み込み
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            wv.GoForward();//進む

        }
    }
}
