using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FetchMobamasPRA
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // AnyCPU対応したい場合は↓を参考に処理を追加
            // https://www.valuestar.work/news/archives/26

            Application.Run(new MainForm());
        }

        #region ログ出力
        /// <summary>
        /// ログ出力
        /// <param name="message">ログ テキスト</param>
        /// </summary>
        public static string WriteLog(string message)
        {
            var writeFolder = System.IO.Path.Combine(Application.StartupPath, "FetchLog");

            // 存在確認
            if (System.IO.Directory.Exists(writeFolder) == false)
            {
                // フォルダの作成
                System.IO.Directory.CreateDirectory(writeFolder);
            }
            // パス作成
            var logFilePath = System.IO.Path.Combine(writeFolder, $"{DateTime.Today:yyyyMMdd}_FetchMobamasPRA.txt");

            using (var sw = new System.IO.StreamWriter(logFilePath, true))
            {
                var lineCount = message.Count(c => c == '\n') + 1;

                if (lineCount == 1)
                {
                    // 単一行のログ出力
                    sw.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} {message}");
                }
                else
                {
                    // 複数行のログ出力
                    sw.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} ");
                    // ログ出力
                    sw.WriteLine(message);
                }
            }
            // ログ削除
            Cleanup(writeFolder);

            return logFilePath;
        }

        /// <summary>
        /// ログ削除（任意のフォルダ、任意のファイル名）
        /// <param name="message">ログ テキスト</param>
        /// </summary>
        public static void Cleanup(string writeFolder)
        {
            // 存在確認
            if (System.IO.Directory.Exists(writeFolder) == true)
            {
                var removeList = new List<System.IO.FileInfo>();

                // ファイル列挙（txt）
                var logFiles = System.IO.Directory.EnumerateFiles(writeFolder, "*.txt");

                foreach (var file in logFiles)
                {
                    var fi = new System.IO.FileInfo(file);

                    // 経過時間を取得
                    var ts = DateTime.Now - (fi?.LastWriteTime ?? DateTime.Now);
                    // 有効期限 経過
                    if (ts.TotalHours > 720)
                    {
                        // リストへ追加
                        removeList.Add(fi);
                    }
                }
                // ファイル削除
                foreach (var fi in removeList)
                {
                    try
                    {
                        // 実行
                        fi.Delete();
                    }
                    catch (System.Exception e)
                    {
                        // 例外は無視
                        System.Diagnostics.Trace.WriteLine(e.Message);
                    }
                }
            }
        }
        #endregion

        #region 例外処理
        /// <summary>
        /// 通常の例外
        /// <param name="e">例外 オブジェクト</param>
        /// </summary>
        public static string WriteExceptionLog(System.Exception e)
        {
            // ファイル出力
            var logFilePath = WriteLog(e.ToString());
            // メール送信
            SmtpMail.Send(Application.ProductName, e.ToString(), logFilePath);

            return logFilePath;
        }
        #endregion
    }
}
