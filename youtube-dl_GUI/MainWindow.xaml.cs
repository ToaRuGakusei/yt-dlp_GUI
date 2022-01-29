using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace youtube_dl_GUI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        private string URl;
        public MainWindow()
        {
            InitializeComponent();
            updateChecker();
            cancel.IsEnabled = false;


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
            DateTime time1 = DateTime.Parse("2022/01/20 11:24:00");
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
            if (u.Text.Contains("https"))
            {
                que.Items.Add(new { url = u.Text, Size = "" });

            }
            run.IsEnabled = false;
            cancel.IsEnabled = true;
            int i = 0;

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
        private void Download()
        {
            StreamReader sm = new StreamReader(Path.GetTempPath() + "\\" + "Path.txt");
            string saveFolder = sm.ReadToEnd();
            //string command = @"D:\ユーザーデータ\Documents\repo\youtube-dl_GUI\youtube-dl_GUI\youtube-dl.exe";
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
                            si = new ProcessStartInfo(@".\yt-dlp.exe", $"--format bestvideo[ext=mp4]+bestaudio[ext=m4a] --embed-subs --embed-thumbnail --all-subs --merge-output-format {mp4_mkv} --all-subs --embed-subs --embed-thumbnail --xattrs --add-metadata -ciw -o \"{saveFolder}\\%(title)s\" {item} ");
                        }

                        i++;
                    }


                }
                else
                {
                    //Directory.CreateDirectory(Path.GetTempPath() + "\\" + "toggle.txt");
                    //Directory.CreateDirectory(Path.GetTempPath() + "\\" + "cmd.txt");
                    //StreamReader sm1 = new StreamReader(Path.GetTempPath() + "\\" + "toggle.txt");
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
                    //sm1.Close();


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
                                    Match GiB = Regex.Match(l, @"\d{0,999999}.\d{0,999999}(GiB)", RegexOptions.IgnoreCase);
                                    Match MiB = Regex.Match(l, @"\d{0,999999}.\d{0,999999}(MiB)+(?!/)", RegexOptions.IgnoreCase);
                                    string M = MiB.Value;
                                    string G = GiB.Value;
                                    if (M.Contains("MiB") && n == 0)
                                    {
                                        M = M.Replace(" at", "");
                                        foreach (var li in que.Items)
                                        {
                                            que.Items.RemoveAt(0);
                                            que.Items.Add(new { url = li, Size = M });
                                            break;
                                        }

                                        n++;
                                    }
                                    if (G.Contains("GiB") && n == 0)
                                    {
                                        G = G.Replace(" at", "");
                                        foreach (var li in que.Items)
                                        {
                                            que.Items.RemoveAt(0);
                                            que.Items.Add(new { url = li, Size = G });
                                            break;
                                        }

                                        n++;
                                    }
                                    label1.Content = ($"{l}");
                                }));
                            }
                            catch (Exception ex)
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
            //オーディオリソースを取り出す
            System.IO.Stream strm = Properties.Resources.end;
            //同期再生する
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(strm);
            player.Play();
            //後始末
            player.Dispose();
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
            MessageBox.Show("キャンセルされました。", "Infomation", MessageBoxButton.OK, MessageBoxImage.Information);
            run.IsEnabled = true;
        }

        private void SettingItem_Click(object sender, RoutedEventArgs e)
        {
            setting s = new setting();
            s.Show();
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
                que.Items.Add(u.Text);
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            que.Items.Clear();
        }

        private void que_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
