using Microsoft.Web.WebView2.Core;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
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
            //新しいタブで開かないようにする。
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
            try
            {
                System.Diagnostics.FileVersionInfo vi =
System.Diagnostics.FileVersionInfo.GetVersionInfo(
 @".\yt-dlp.exe");

                string ver = vi.ProductVersion;
                if (ver == "2022.02.04 on Python 3.8.10" || ver == "2022.01.21 on Python 3.8.10" || ver == "2022.02.03 on Python 3.8.10" || ver == "2022.02.04 on Python 3.8.10" || ver == "2022.03.08.1 on Python 3.8.10" || ver == "2022.03.08 on Python 3.8.10")
                {
                    string[] Urls = new string[] { "https://github.com/yt-dlp/yt-dlp/releases/download/2022.04.08/yt-dlp.exe" };
                    foreach (string _url in Urls)
                    {
                        downloadFileAsync(_url, $".\\yt-dlp.exe");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }


            //自分のサイトから日時を引っ張ってくる
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
            DateTime time1 = DateTime.Parse("2022/04/24 19:04:00");
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
            if (u.Text == "")
            {
                MessageBox.Show("URLを入力してください。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //ffmpegとyt-dlpをダウンロードする。
            if (!File.Exists(@".\yt-dlp.exe") && !File.Exists(@".\ffmpeg.exe") && !File.Exists(@".\ffplay.exe") && !File.Exists(@".\ffprobe.exe"))
            {
                var ms = MessageBox.Show("ffmpegとyt-dlpをダウンロードしますか？", "ダウンロード", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (ms == MessageBoxResult.Yes)
                {
                    int i = 0;
                    string[] Urls = new string[] { "https://github.com/yt-dlp/yt-dlp/releases/download/2022.03.08.1/yt-dlp.exe", "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-lgpl.zip" };
                    foreach (string url in Urls)
                    {
                        if (i == 0)
                        {
                            downloadFileAsync(url, $".\\yt-dlp.exe");

                        }
                        else
                        {
                            downloadFileAsync(url, $".\\ffmpeg.zip");
                        }
                        i++;
                    }
                }
                else
                {
                    MessageBox.Show("キャンセルされました。");
                    return;
                }

            }
            else
            {

                mp4_mkv();
                soundonoff();
                //
                if (u.Text.Contains("https"))
                {
                    que.Items.Add(new { url = u.Text, Size = "", ETA = "" });

                }

                run.IsEnabled = false;
                cancel.IsEnabled = true;

                Task task = Task.Run(() =>
                {
                    Download();
                });
            }




        }


        private void list()
        {
            //queからuriを取得する。
            Task task = Task.Run(() =>
            {
                if (que.Items.Count > 0)
                {
                    Download();
                }
            });
        }
        private ProcessStartInfo si;
        private string CommandLine;
        public int i = 0;
        public string cmd_check;
        public int n = 0;
        public string title;
        public string ex;
        private void Download()
        {
            //txtから設定を取得する。
            StreamReader sm = new StreamReader(Path.GetTempPath() + "\\" + "Path.txt");
            string saveFolder = sm.ReadToEnd();
            StreamReader sm3 = new StreamReader(Path.GetTempPath() + "\\" + "switch.txt");
            string mp4_mkv = sm3.ReadToEnd();
            StreamReader sm4 = new StreamReader(Path.GetTempPath() + "\\" + "cmd_check.txt");
            cmd_check = sm4.ReadToEnd();
            sm4.Close();


            sm.Close();
            sm3.Close();

            this.Dispatcher.Invoke((Action)(() =>
            {
                //
                if (que.Items.Count > 0)
                {
                    int i = 0;
                    foreach (var item in que.Items)
                    {
                        string _u = DataBinder.Eval(item, "url").ToString();
                        if (mp4_h264.IsSelected == false && mkv_h264.IsSelected == false && i == 0 && mp4_mkv != "mp3" && mp4_mkv != "m4a" && mp4_mkv != "flac")
                        {
                            u.Text = DataBinder.Eval(item, "url").ToString();
                            si = new ProcessStartInfo(@".\yt-dlp.exe", $"--format bestvideo+251/bestvideo+bestaudio/best --embed-subs --embed-thumbnail --all-subs --merge-output-format {mp4_mkv} --all-subs --embed-subs --embed-thumbnail --xattrs --add-metadata -i -ciw -o \"{saveFolder}\\%(title)s\" {_u} ");
                        }
                        else if (i == 0 && mp4_h264.IsSelected == true || mkv_h264.IsSelected == true && mp4_mkv == "mp4" || mp4_mkv == "mkv")
                        {
                            si = new ProcessStartInfo(@".\yt-dlp.exe", $" --format bestvideo[ext=mp4]+bestaudio[ext=m4a] --embed-subs --embed-thumbnail --all-subs --merge-output-format {mp4_mkv} --all-subs --embed-subs --embed-thumbnail --xattrs --add-metadata -i -ciw -S vcodec:h264 -o \"{saveFolder}\\%(title)s\" {_u} ");
                        }
                        else if (i == 0 && mp4_mkv == "mp3" || mp4_mkv == "m4a" || mp4_mkv == "flac")
                        {
                            string op = $"--ignore-errors --format bestaudio --extract-audio --audio-format {mp4_mkv} --audio-quality 160K --output \"{saveFolder}\\%(title)s.%(ext)s\" -i {_u}";
                            si = new ProcessStartInfo(@".\yt-dlp.exe", $"{op}");
                        }
                        if (cmd_check == "true")
                        {
                            StreamReader sm2 = new StreamReader(Path.GetTempPath() + "\\" + "cmd.txt");
                            CommandLine = sm2.ReadToEnd();
                            sm2.Close();
                            si = new ProcessStartInfo(@".\yt-dlp.exe", $"{CommandLine} {_u} ");
                        }



                        i++;
                    }


                }
                else
                {


                }

            }));

            // ウィンドウ表示を完全に消したい場合
            si.CreateNoWindow = true;
            si.RedirectStandardError = false;
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
                            if (c == true)
                            {
                                MessageBox.Show("キャンセルされました。", "キャンセル", MessageBoxButton.OK, MessageBoxImage.Information);
                                c = false;
                                cancel.IsEnabled = false;
                            }
                            else
                            {
                                MessageBox.Show("ダウンロードが終了しました。", "終了", MessageBoxButton.OK, MessageBoxImage.Information);
                                cancel.IsEnabled = false;
                            }
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
                                    Match ETA = Regex.Match(l, @"ETA \d{0,999999}\d{0,999999}:\d{0,999999}\d{0,999999}:\d{0,999999}\d{0,999999}", RegexOptions.IgnoreCase);
                                    ETA = Regex.Match(l, @"ETA \d{0,999999}\d{0,999999}:\d{0,999999}\d{0,999999}", RegexOptions.IgnoreCase);
                                    if (ETA == null)
                                    {
                                        ETA = Regex.Match(l, @"ETA \d{0,999999}\d{0,999999}:\d{0,999999}\d{0,999999}:\d{0,999999}\d{0,999999}", RegexOptions.IgnoreCase);
                                        ETA = Regex.Match(l, @"ETA \d{0,999999}\d{0,999999}:\d{0,999999}\d{0,999999}", RegexOptions.IgnoreCase);
                                    }
                                    object M = MiB.Value;
                                    object G = GiB.Value;
                                    if (l.Contains("[download] Destination"))
                                    {

                                        title = l.Replace("[download] Destination: ", "");
                                        ex = Path.GetExtension(l);
                                        title = System.IO.Path.GetFileNameWithoutExtension(title);
                                        title = System.IO.Path.GetFileNameWithoutExtension(title);
                                    }

                                    if (MiB.Value.Contains("MiB") && n == 0 && !u.Text.Contains("playlist"))
                                    {
                                        List<object> lis = new List<object>();
                                        que.Items.RemoveAt(0);
                                        foreach (var _item in que.Items)
                                        {

                                            lis.Add(_item);
                                            Debug.WriteLine($"list\n{_item}");
                                        }
                                        que.Items.Clear();

                                        que.Items.Add((new { url = title, Size = M, ETA = ETA.Value, extension = ex }));
                                        foreach (var _item in lis)
                                        {
                                            Debug.WriteLine(_item);
                                            que.Items.Add(_item);
                                        }




                                        n++;
                                    }
                                    else if (GiB.Value.Contains("GiB") && n == 0 && !u.Text.Contains("playlist"))
                                    {
                                        List<object> lis = new List<object>();
                                        que.Items.RemoveAt(0);
                                        foreach (var _item in que.Items)
                                        {

                                            lis.Add(_item);
                                            Debug.WriteLine($"list\n{_item}");
                                        }
                                        que.Items.Clear();

                                        que.Items.Add((new { url = title, Size = G, ETA = ETA.Value, extension = ex }));
                                        foreach (var _item in lis)
                                        {
                                            Debug.WriteLine(_item);
                                            que.Items.Add(_item);
                                        }


                                        n++;
                                    }

                                    if (MiB.Value.Contains("MiB") && u.Text.Contains("playlist"))
                                    {
                                        List<object> lis = new List<object>();
                                        que.Items.RemoveAt(0);
                                        foreach (var _item in que.Items)
                                        {

                                            lis.Add(_item);
                                            Debug.WriteLine($"list\n{_item}");
                                        }
                                        que.Items.Clear();

                                        que.Items.Add((new { url = title, Size = M, ETA = ETA.Value, extension = ex }));
                                        foreach (var _item in lis)
                                        {
                                            Debug.WriteLine(_item);
                                            que.Items.Add(_item);
                                        }




                                        n++;
                                    }
                                    else if (GiB.Value.Contains("GiB") && u.Text.Contains("playlist"))
                                    {
                                        List<object> lis = new List<object>();
                                        que.Items.RemoveAt(0);
                                        foreach (var _item in que.Items)
                                        {

                                            lis.Add(_item);
                                            Debug.WriteLine($"list\n{_item}");
                                        }
                                        que.Items.Clear();

                                        que.Items.Add((new { url = title, Size = G, ETA = ETA.Value, extension = ex }));
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
            if (toggle1.IsOn == true)
            {
                //オーディオリソースを取り出す
                System.IO.Stream strm = yt_dlp_GUI.Properties.Resources.end;
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
        private bool c = false;
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
            c = true;
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
            }
        }
        private void load()
        {
            //xmlに書き換え予定

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
            if (!File.Exists(Path.GetTempPath() + "\\" + "cmd_check.txt"))
            {
                FileStream fs = File.Create(Path.GetTempPath() + "\\" + "cmd_check.txt");
                fs.Close();
            }
            StreamReader sr2 = new StreamReader(Path.GetTempPath() + "\\" + "switch.txt");
            string mkv_mp4 = sr2.ReadToEnd();
            if (mkv_mp4 == "mp4")
            {
                mp4.IsSelected = true;
            }
            else if (mkv_mp4 == "mkv")
            {
                mkv.IsSelected = true;
            }
            else if (mkv_mp4 == "mp3")
            {
                mp3.IsSelected = true;
            }
            else if (mkv_mp4 == "m4a")
            {
                m4a.IsSelected = true;
            }
            else if (mkv_mp4 == "flac")
            {
                f.IsSelected = true;
            }
            else if (mkv_mp4 == "webm")
            {
                webm.IsSelected = true;

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

            //xmlに書き換え予定
            StreamWriter sm2 = new StreamWriter(Path.GetTempPath() + "\\" + "switch.txt", false);

            if (mp4.IsSelected == true)
            {
                sm2.Write("mp4");
            }
            else if (mkv.IsSelected == true)
            {
                sm2.Write("mkv");
            }
            else if (mp3.IsSelected == true)
            {
                sm2.Write("mp3");

            }
            else if (m4a.IsSelected == true)
            {
                sm2.Write("m4a");
            }
            else if (f.IsSelected == true)
            {
                sm2.Write("flac");

            }
            else if (webm.IsSelected == true)
            {
                sm2.Write("webm");

            }
            else if (mp4_h264.IsSelected == true)
            {
                sm2.Write("mp4");
            }
            else if (mkv_h264.IsSelected == true)
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
            }
            catch (Exception)
            {

            }

        }
        private async void downloadFileAsync(string uri, string outputPath)
        {

            var client = new HttpClient();
            HttpResponseMessage res = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            using (var fileStream = File.Create(outputPath))
            {
                using (var httpStream = await res.Content.ReadAsStreamAsync())
                {
                    httpStream.CopyTo(fileStream);
                    fileStream.Flush();
                    n++;
                }
            }


            if (n == 2)
            {
                //ZIP書庫を開く
                using (ZipArchive a = ZipFile.OpenRead(@".\ffmpeg.zip"))
                {
                    for (int i = 0; i <= 3; i++)
                    {
                        ZipArchiveEntry e;
                        if (i == 0)
                        {
                            e = a.GetEntry("ffmpeg-master-latest-win64-lgpl/bin/ffmpeg.exe");
                            if (e == null)
                            {
                                //見つからなかった時
                                Console.WriteLine("ffmpeg が見つかりませんでした。");
                            }
                            else
                            {
                                //2番目の引数をTrueにすると、上書きする
                                e.ExtractToFile(@".\ffmpeg.exe", true);
                            }
                        }
                        else if (i == 1)
                        {
                            e = a.GetEntry("ffmpeg-master-latest-win64-lgpl/bin/ffprobe.exe");
                            if (e == null)
                            {
                                //見つからなかった時
                                Console.WriteLine("ffmpeg が見つかりませんでした。");
                            }
                            else
                            {
                                //2番目の引数をTrueにすると、上書きする
                                e.ExtractToFile(@".\ffprobe.exe", true);
                            }
                        }
                        else
                        {
                            e = a.GetEntry("ffmpeg-master-latest-win64-lgpl/bin/ffplay.exe");
                            if (e == null)
                            {
                                //見つからなかった時
                                Console.WriteLine("ffmpeg が見つかりませんでした。");
                            }
                            else
                            {
                                //2番目の引数をTrueにすると、上書きする
                                e.ExtractToFile(@".\ffplay.exe", true);
                            }

                        }

                    }


                }
                mp4_mkv();
                soundonoff();
                if (u.Text.Contains("https"))
                {
                    que.Items.Add(new { url = u.Text, Size = "", ETA = "" });

                }

                run.IsEnabled = false;
                cancel.IsEnabled = true;

                Task task = Task.Run(() =>
                {
                    Download();
                });

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

        private void cmd_Checked(object sender, RoutedEventArgs e)
        {
            save.IsEnabled = false;
            Combo.IsEnabled = false;
            yt_dlp_GUI.Command command = new yt_dlp_GUI.Command();
            command.Owner = this;
            command.Show();
        }

        private void cmd_Unchecked(object sender, RoutedEventArgs e)
        {
            save.IsEnabled = true;
            Combo.IsEnabled = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            yt_dlp_GUI.Command setting = new yt_dlp_GUI.Command();
            setting.Owner = this;
            setting.Show();
        }
    }
}
