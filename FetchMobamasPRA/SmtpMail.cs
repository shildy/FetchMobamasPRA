using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FetchMobamasPRA
{
    public sealed class SmtpMail
    {
        #region 定数
        // メール送信の待機時間
        public const int MAIL_TIMEOUT = 120000;
        #endregion

        #region メール送信
        /// <summary>
        /// メール送信（既定の設定を利用）
        /// </summary>
        /// <param name="subject">件名</param>
        /// <param name="body">本文</param>
        /// <param name="files">添付ファイル（タブ区切り）</param>
        public static void Send(string subject, string body, string files)
        {
            // メール送信
            var enc = Encoding.GetEncoding("UTF-8");

            // 難読化されたパスワード取得
            var password = Properties.Settings.Default.SMTPPassword;

            if (password.Length > 0)
            {
                // デコード
                password = enc.GetString(Convert.FromBase64String(password));
            }

            var server = Properties.Settings.Default.SMTPServer;
            var port = Properties.Settings.Default.SMTPPort;
            var ssl = Properties.Settings.Default.SMTPSSL;
            var user = Properties.Settings.Default.SMTPUserID;
            var fromAddress = Properties.Settings.Default.SMTPFrom;
            var displayName = Properties.Settings.Default.SMTPDisplay;
            var toAddress = Properties.Settings.Default.SMTPTo;

            Send(server, port, ssl, user, password, fromAddress, displayName, toAddress, subject, body, files);
        }

        /// <summary>
        /// メール送信
        /// </summary>
        /// <param name="server">サーバ</param>
        /// <param name="port">ポート</param>
        /// <param name="user">ユーザID</param>
        /// <param name="password">パスワード</param>
        /// <param name="ssl">SSL</param>
        /// <param name="fromAddress">差出人</param>
        /// <param name="toAddress">宛先（カンマ区切り）</param>
        /// <param name="subject">件名</param>
        /// <param name="body">本文</param>
        /// <param name="files">添付ファイル（タブ区切り）</param>
        public static void Send(string server, int port, bool ssl, string user, string password, string fromAddress, string displayName, string toAddress, string subject, string body, string files)
        {
            try
            {
                // メッセージ
                using (var mailMessage = new System.Net.Mail.MailMessage())
                {
                    // エンコード指定（iso-2022-jp = 日本語[JIS]）
                    var encoding = Encoding.GetEncoding(50220);
                    // 件名
                    if (InstalledDotNet45OrLater() == true)
                    {
                        // .NET Framework 4.5 or Later
                        mailMessage.Subject = EncodingMailHeader(EncodingMailHeader(subject, encoding), encoding);
                    }
                    else
                    {
                        // .NET Framework 4.0 or Later
                        mailMessage.Subject = EncodingMailHeader(subject, encoding);
                    }
                    // 本文（通常）
                    // mailMessage.Body = body;
                    // mailMessage.BodyEncoding = encoding;
                    // 本文（7-bit）
                    // プレーンテキストのAlternateViewを作成
                    var htmlView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(body, encoding, System.Net.Mime.MediaTypeNames.Text.Plain);
                    // TransferEncoding.SevenBitを指定
                    htmlView.TransferEncoding = System.Net.Mime.TransferEncoding.SevenBit;
                    // AlternateViewを追加
                    mailMessage.AlternateViews.Add(htmlView);
                    // 差出人
                    mailMessage.From = new System.Net.Mail.MailAddress(fromAddress, displayName);
                    // 宛先
                    var tos = toAddress.Split(',');

                    for (var i = 0; i < tos.Length; i++)
                    {
                        if (tos[i].Length > 0)
                        {
                            mailMessage.To.Add(new System.Net.Mail.MailAddress(tos[i]));
                        }
                    }
                    // 添付ファイル
                    var fls = files.Split('\t');

                    for (var i = 0; i < fls.Length; i++)
                    {
                        if (fls[i].Length > 0)
                        {
                            if (System.IO.File.Exists(fls[i]) == true)
                            {
                                var attachment = new System.Net.Mail.Attachment(fls[i]);
                                attachment.NameEncoding = encoding;
                                mailMessage.Attachments.Add(attachment);
                            }
                        }
                    }
                    // Message-Id（追加しないとGoogleに受信拒否される）
                    using (var process = System.Diagnostics.Process.GetCurrentProcess())
                    {
                        mailMessage.Headers.Add("Message-Id", "<" + DateTime.Now.ToString("yyyyMMddHHmmss.ffff") + "." + process.Id.ToString() + "@mail.server.jp>");
                    }
                    // X-Mailer
                    mailMessage.Headers.Add("X-Mailer", "Fetch.Mobamas.PRA.Mail");
                    // SMTP Client
                    using (var smtpClient = new System.Net.Mail.SmtpClient())
                    {
                        // サーバとポート
                        smtpClient.Host = server;
                        smtpClient.Port = port;
                        smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                        smtpClient.Timeout = MAIL_TIMEOUT;
                        // ユーザIDとパスワード
                        if (user.Length > 0)
                        {
                            smtpClient.Credentials = new System.Net.NetworkCredential(user, password);
                        }
                        // SSL
                        smtpClient.EnableSsl = ssl;
                        // 送信
                        smtpClient.Send(mailMessage);
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// メールヘッダのエンコード
        /// </summary>
        /// <param name="source">文字列</param>
        /// <param name="encoding">エンコーディング</param>
        /// <returns></returns>
        private static string EncodingMailHeader(string source, System.Text.Encoding encoding)
        {
            // BASE64
            var result = System.Convert.ToBase64String(encoding.GetBytes(source));
            // RFC2047形式
            result = string.Format("=?{0}?B?{1}?=", encoding.BodyName, result);

            return result;
        }
        #endregion

        #region .NET Framework
        /// <summary>
        /// インストールされている.NET Frameworkが4.5以降か調べる
        /// </summary>
        /// <returns>true：4.5以降、false：4.0以前</returns>
        public static bool InstalledDotNet45OrLater()
        {
            var result = false;

            // レジストリ キー
            using (var ndpKey = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                // レジストリ 値
                var releaseKey = Convert.ToInt32(ndpKey.GetValue("Release"));
                // チェック
                result = InstalledDotNet45OrLater(releaseKey);
            }

            return result;
        }

        /// <summary>
        /// インストールされている.NET Frameworkが4.5以降か調べる
        /// </summary>
        /// <param name="releaseKey">Releaseの値</param>
        /// <returns>true：4.5以降、false：4.0以前</returns>
        private static bool InstalledDotNet45OrLater(int releaseKey)
        {
            // 4.6 or later
            if (releaseKey >= 393295)
            {
                return true;
            }
            // 4.5.2 or later
            if (releaseKey >= 379893)
            {
                return true;
            }
            // 4.5.1 or later
            if (releaseKey >= 378675)
            {
                return true;
            }
            // 4.5 or later
            if (releaseKey >= 378389)
            {
                return true;
            }
            // This line should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return false;
        }
        #endregion
    }
}
