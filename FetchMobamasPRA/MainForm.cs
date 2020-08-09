using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using CefSharp;
using CefSharp.WinForms;
using FetchMobamasPRA.CinderellaAPI;
using FetchMobamasPRA.Data;

namespace FetchMobamasPRA
{
    /// <summary>
    /// モバマスのデータ取得 メインフォーム
    /// </summary>
    /// <remarks>自分が所属するプロダクションのプロデューサー情報のみ取得</remarks>
    public partial class MainForm : Form
    {
        #region 定数
        private enum FetchMode {
            NotFetch = 0,
            FetchMember,
            FetchPRA,
            FetchThisWeek,
            FetchEventRank,
        };
        // 待機時間
        private const int ACCESS_WAIT = 1250;
        private const int CINDERELLA_API_WAIT = 750;
        // モバマスのトップページ
        private readonly string START_URL = "http://sp.pf.mbga.jp/12008305/";
        // プロダクションの社員一覧
        private readonly string KNIGHTS_MEMBER = "http://sp.pf.mbga.jp/12008305/?guid=ON&url=http://125.6.169.35/idolmaster/knights/knights_member";
        private readonly string KNIGHTS_MEMBER_FIRST = "?l_frm=Knights_knights_top_for_member_1&rnd=";
        private readonly string KNIGHTS_MEMBER_NEXT = "?next=1?l_frm=Knights_knights_top_for_member_1&rnd=";
        private readonly string KNIGHTS_MEMBER_FILE = "knights_member.xml";
        // knights_member.xml 要素名
        private readonly string MEMBER_XML_ROOT = "KnightsMember";
        private readonly string MEMBER_XML_ELEMENT = "Member";
        private readonly string MEMBER_XML_ID = "memberId";
        private readonly string MEMBER_XML_NAME = "memberName";
        private readonly string MEMBER_XML_TYPE = "memberType";
        // 自分の成績
        //private readonly string RECORDS_MYSELF = "http://sp.pf.mbga.jp/12008305/?guid=ON&url=http:%2F%2F125.6.169.35%2Fidolmaster%2Fp_ranking_award%2Frecords_for_user";
        private readonly string RECORDS_MYSELF = "http://sp.pf.mbga.jp/12008305/?guid=ON&url=http://125.6.169.35/idolmaster/p_ranking_award/records_for_user";
        // 自分以外の成績
        //private readonly string RECORDS_OTHER = "http://sp.pf.mbga.jp/12008305/?guid=ON&url=http:%2F%2F125.6.169.35%2Fidolmaster%2Fp_ranking_award%2Frecords_for_other";
        private readonly string RECORDS_OTHER = "http://sp.pf.mbga.jp/12008305/?guid=ON&url=http://125.6.169.35/idolmaster/p_ranking_award/records_for_other";
        // プロダクションの社員 検索キー
        private readonly string MEMBER_COUNT_KEY = "<span class=\"blue\">所属社員:</span>";
        private readonly string MEMBER_INFO_KEY = "http://sp.pf.mbga.jp/12008305/?guid=ON&url=http:%2F%2F125.6.169.35%2Fidolmaster%2Fprofile%2Fshow%2F";
        // 今週の PRA
        private readonly string THISWEEK_RECORDS = "http://sp.pf.mbga.jp/12008305/?guid=ON&url=http://125.6.169.35/idolmaster/p_ranking_award/ranking_for_user/0/2";
        private readonly string THISWEEK_RECORDS_FIRST = "?l_frm=p_ranking_award_ranking_for_user_1&rnd=";
        private readonly string THISWEEK_RECORDS_NEXT = "?l_frm=p_ranking_award_ranking_for_user_2&rnd=";
        #endregion

        #region プロパティ
        #region オーバーライド
        /// <summary>
        /// ShowWithoutActivation プロパティ
        /// </summary>
        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }
        #endregion
        #endregion

        #region メンバー変数
        // モード
        private FetchMode fetchMode = FetchMode.NotFetch;
        // Chromium
        public ChromiumWebBrowser chromeBrowser = null;
        // 自分のモバゲーID
        private string myMobageID = string.Empty;
        // 社員一覧 作成用
        private int? knightsMemberCount = null;
        private int knightsMemberPage = 1;
        // 社員一覧
        private List<KnightsMemberData> knightsMemberList = new List<KnightsMemberData>();
        private int knightsMemberIndex = 0;
        // 解析
        private bool scrapingProgress = false;
        private Random random = new Random();
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            // CefSharpの初期化
            InitializeChromium();
        }

        #region イベントハンドラー
        /// <summary>
        /// フォームロード イベント
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">イベント引数</param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            // コマンドライン引数の取得
            var args = Environment.GetCommandLineArgs();

            foreach (var argValue in args)
            {
                // オプション指定の文字列を検索
                if (argValue.ToLower().Equals("FetchMember".ToLower()) == true)
                {
                    // 社員一覧
                    fetchMode = FetchMode.FetchMember;
                    // ログ出力
                    Program.WriteLog("社員一覧を作成します。");
                }
                else if (argValue.ToLower().Equals("FetchPRA".ToLower()) == true)
                {
                    // PRA（過去 3ヶ月）
                    fetchMode = FetchMode.FetchPRA;
                    // ログ出力
                    Program.WriteLog("PRA（過去 3ヶ月）を取得します。");
                }
                else if (argValue.ToLower().Equals("FetchThisWeek".ToLower()) == true)
                {
                    // PRA（今週）
                    fetchMode = FetchMode.FetchThisWeek;
                    // ログ出力
                    Program.WriteLog("PRA（今週）を取得します。");
                }
                else if (argValue.ToLower().Equals("FetchEventRank".ToLower()) == true)
                {
                    // PRA（今週）
                    fetchMode = FetchMode.FetchEventRank;
                    // ログ出力
                    Program.WriteLog("イベント順位を取得します。");
                }
                else if (argValue.ToLower().IndexOf("-ID:".ToLower()) == 0)
                {
                    // 自分のモバゲーID
                    myMobageID = argValue.Substring("-ID:".Length);
                    // ログ出力
                    Program.WriteLog($"自分のモバゲーID: {myMobageID}");
                }
            }
            // フォームの配置
            RestoreFormSettings(Properties.Settings.Default.MainBounds, Properties.Settings.Default.MainWindowState);
        }

        /// <summary>
        /// フォーム初回表示
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">イベント引数</param>
        private void MainForm_Shown(object sender, EventArgs e)
        {
            // 自分のプロセスを取得
            using (var process = System.Diagnostics.Process.GetCurrentProcess())
            {
                try
                {
                    // プロセスをアクティブにする
                    Microsoft.VisualBasic.Interaction.AppActivate(process.Id);
                }
                catch (Exception ex)
                {
                    Program.WriteExceptionLog(ex);
                }
            }
        }

        /// <summary>
        /// フォームが閉じる直前
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">イベント引数</param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // フォームの設定を保存
            if (this.WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.MainBounds = this.Bounds;
            }
            else
            {
                Properties.Settings.Default.MainBounds = this.RestoreBounds;
            }
            // フォームの状態を保存
            if (this.WindowState != FormWindowState.Minimized)
            {
                Properties.Settings.Default.MainWindowState = this.WindowState;
            }
            else
            {
                Properties.Settings.Default.MainWindowState = FormWindowState.Normal;
            }
            // 書き込み
            Properties.Settings.Default.Save();
            // ブラウザーを破棄
            chromeBrowser.Dispose();
            // CefSharpの終了
            Cef.Shutdown();
        }

        /// <summary>
        /// タイマー イベント
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">イベント引数</param>
        private void timerDocumentCompleted_Tick(object sender, EventArgs e)
        {
            try
            {
                // 読み込み完了・解析中
                if (timerDocumentCompleted.Enabled == true && scrapingProgress == true)
                {
                    // タイマー停止
                    timerDocumentCompleted.Enabled = false;
                    // モバゲーIDが設定済み
                    if (myMobageID.Length > 0)
                    {
                        // 待機
                        System.Threading.Thread.Sleep(ACCESS_WAIT);
                        // モードで分岐
                        switch (fetchMode)
                        {
                            case FetchMode.FetchMember:
                                // 社員一覧
                                FetchMember();

                                break;
                            case FetchMode.FetchPRA:
                                // PRA（過去 3ヶ月）
                                FetchPRA();

                                break;
                            case FetchMode.FetchThisWeek:
                                // PRA（今週）
                                FetchThisWeek();

                                break;
                            case FetchMode.FetchEventRank:
                                // イベント順位
                                FetchEventRank();

                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.WriteExceptionLog(ex);
            }
        }

        #region 社員一覧
        /// <summary>
        /// 社員一覧の取得
        /// </summary>
        private async void FetchMember()
        {
            // トップページの場合
            var url = new Uri(textUrl.Text).ToString();

            if (url.Equals(START_URL) == true)
            {
                // 待機
                System.Threading.Thread.Sleep(ACCESS_WAIT);
                // 社員一覧を読み込む
                var memberUrl = new Uri(KNIGHTS_MEMBER + KNIGHTS_MEMBER_FIRST + GetRndParameter());

                LoadUrl(memberUrl.ToString());
            }
            else if (url.StartsWith(KNIGHTS_MEMBER) == true)
            {
                // HTMLソース取得
                var source = await chromeBrowser.GetBrowser().MainFrame.GetSourceAsync();

                try
                {
                    // 出力
                    DisplayOutput("社員一覧を解析中...");
                    // 解析
                    if (CreateKnightsMember(knightsMemberPage, source) == true)
                    {
                        // 次のページへ
                        knightsMemberPage++;
                        // パラメーター
                        var memberCount = 5 * knightsMemberPage;

                        if (memberCount <= knightsMemberCount)
                        {
                            memberCount -= 10;

                            if (memberCount >= 0)
                            {
                                // 待機
                                System.Threading.Thread.Sleep(ACCESS_WAIT);
                                // 社員一覧を読み込む
                                var memberUrl = new Uri(KNIGHTS_MEMBER + $"/{memberCount}" + KNIGHTS_MEMBER_NEXT + GetRndParameter());

                                LoadUrl(memberUrl.ToString());
                            }
                        }
                        else
                        {
                            // XMLファイルへ書き込み
                            var filePath = System.IO.Path.Combine(Application.StartupPath, KNIGHTS_MEMBER_FILE);

                            DisplayOutput($"書き込み中: {filePath}");

                            if (WriteKnightsMember(filePath) == true)
                            {
                                // ログ出力
                                Program.WriteLog("社員一覧を更新しました。");
                            }
                            else
                            {
                                // ログ出力
                                Program.WriteLog("社員一覧の更新に失敗しました。");
                            }
                            // 閉じる
                            this.InvokeOnUiThreadIfRequired(() => this.Close());
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ログ出力
                    Program.WriteExceptionLog(ex);
                    // ソース出力
                    WriteFetchSource(url, source);
                }
            }
            // 処理完了
            scrapingProgress = false;
        }

        /// <summary>
        /// 社員一覧の書き込み
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool WriteKnightsMember(string filePath)
        {
            var existsMemberList = new List<KnightsMemberData>();

            // 既にファイルがある場合
            if (System.IO.File.Exists(filePath) == true)
            {
                // 既存データを読み込む
                var xd = XDocument.Load(filePath);
                // 社員一覧を取得
                var rt = xd.Element(MEMBER_XML_ROOT);
                var members = from e in rt.Elements() select e;
                // 社員データを作成
                foreach (var member in members)
                {
                    var data = new KnightsMemberData();

                    // モバゲーID
                    data.ID = member.Attribute(MEMBER_XML_ID).Value;
                    // プロデューサー名
                    data.Name = member.Attribute(MEMBER_XML_NAME).Value;
                    // プロデューサー種類
                    data.ProducerType = int.Parse(member.Attribute(MEMBER_XML_TYPE).Value);

                    existsMemberList.Add(data);
                }
            }
            // 既存と一致する社員を抽出
            foreach (var member in knightsMemberList)
            {
                // 検索
                var exists = existsMemberList.FirstOrDefault(d => d.ID.Equals(member.ID) == true);

                if (exists != null)
                {
                    // 取得データ（更新対象）
                    member.Exists = true;
                    // 既存データ（削除対象外）
                    exists.Exists = true;
                    // プロデューサー種類を初期化（社員）
                    member.ProducerType = KnightsMemberData.TYPE_EMPLOYEE;
                }
            }
            // 既存にあり取得にない社員
            var deleteMemberList = existsMemberList.Where(d => d.Exists == false);

            if (deleteMemberList.Count() > 0)
            {
                // プロデューサー種類の変更
                foreach(var member in deleteMemberList)
                {
                    // 社員の場合
                    if (member.ProducerType == KnightsMemberData.TYPE_EMPLOYEE)
                    {
                        // 退職者
                        member.ProducerType = KnightsMemberData.TYPE_RETIREE;
                    }
                }
                // 追加
                knightsMemberList.AddRange(deleteMemberList);
            }

            try
            {
                var memberElements = new List<XElement>();

                // 各社員のエレメント作成
                foreach (var member in knightsMemberList)
                {
                    var element = new XElement(MEMBER_XML_ELEMENT, 
                                        new XAttribute(MEMBER_XML_ID, member.ID), 
                                        new XAttribute(MEMBER_XML_NAME, member.Name), 
                                        new XAttribute(MEMBER_XML_TYPE, member.ProducerType));
                    // 追加
                    memberElements.Add(element);
                }
                // ソート
                memberElements.Sort(new ElementComparer());
                // XMLエレメント作成
                var xe = new XElement(MEMBER_XML_ROOT, memberElements.ToArray());
                // XMLドキュメント作成
                var xd = new XDocument(xe);
                // 保存
                xd.Save(filePath);
            }
            catch (Exception ex)
            {
                // ログ出力
                Program.WriteExceptionLog(ex);

                return false;
            }

            return true;
        }

        #region ソート
        /// <summary>
        /// 並び替え処理
        /// </summary>
        private class ElementComparer : IComparer<XElement>
        {
            /// <summary>
            /// 並び替え
            /// </summary>
            /// <param name="x">比較元</param>
            /// <param name="y">比較先</param>
            /// <returns>CompareToと同じ</returns>
            public int Compare(XElement x, XElement y)
            {
                var result = 0;

                // NULLチェック
                if (x == null)
                {
                    if (y == null)
                    {
                        result = 0;
                    }
                    else
                    {
                        result = -1;
                    }
                }
                else
                {
                    if (y == null)
                    {
                        result = 1;
                    }
                    else
                    {
                        // モバゲーID (昇順)
                        result = x.Attribute("memberId").Value.CompareTo(y.Attribute("memberId").Value.ToString());
                    }
                }

                return result;
            }
        }
        #endregion
        #endregion

        #region PRA（過去 3ヶ月）
        /// <summary>
        /// PRA（過去 3ヶ月）
        /// </summary>
        private async void FetchPRA()
        {
            // トップページの場合
            var url = new Uri(textUrl.Text).ToString();

            if (url.Equals(START_URL) == true)
            {
                var filePath = System.IO.Path.Combine(Application.StartupPath, KNIGHTS_MEMBER_FILE);

                // 既にファイルがある場合
                if (System.IO.File.Exists(filePath) == true)
                {
                    // 既存データを読み込む
                    var xd = XDocument.Load(filePath);
                    // 社員一覧を取得
                    var rt = xd.Element(MEMBER_XML_ROOT);
                    var members = from e in rt.Elements() select e;
                    // 社員データを作成
                    foreach (var member in members)
                    {
                        var data = new KnightsMemberData();

                        // モバゲーID
                        data.ID = member.Attribute(MEMBER_XML_ID).Value;
                        // プロデューサー名
                        data.Name = member.Attribute(MEMBER_XML_NAME).Value;
                        // プロデューサー種類
                        data.ProducerType = int.Parse(member.Attribute(MEMBER_XML_TYPE).Value);
                        // 過去 6週間分 PRAのURL
                        var startDate = DateTime.Today.AddDays((6.0d * 7.0d) * -1.0d);
                        var loopMonth = new DateTime(startDate.Year, startDate.Month, 1);
                        var endMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        var loop = 0;

                        while (loopMonth <= endMonth)
                        {
                            var month = DateTime.Today.AddMonths(loop * -1);
                            var awardUrl = string.Empty;

                            if (data.ID.Equals(myMobageID) == true)
                            {
                                // 自分用 URL
                                awardUrl = $"http://sp.pf.mbga.jp/12008305/?guid=ON&url=http://125.6.169.35/idolmaster/p_ranking_award/records_for_user/{month:yyyy-MM}?l_frm=p_ranking_award_records_for_user_month_1&rnd={GetRndParameter()}";
                            }
                            else
                            {
                                // 自分以外 URL
                                awardUrl = $"http://sp.pf.mbga.jp/12008305/?guid=ON&url=http://125.6.169.35/idolmaster/p_ranking_award/records_for_other/{month:yyyy-MM}?other_id={data.ID}&l_frm=p_ranking_award_records_for_other_month_1&rnd={GetRndParameter()}";
                            }

                            data.AddRankingAwardUrl(awardUrl);
                            // 次の月へ
                            loopMonth = loopMonth.AddMonths(1);
                            // 次のインデックスへ
                            loop++;
                        }
                        // 退職者でない場合
                        if (data.ProducerType != KnightsMemberData.TYPE_RETIREE)
                        {
                            // 追加
                            knightsMemberList.Add(data);
                        }
                    }
                    // 最初の社員へ
                    knightsMemberIndex = 0;

                    LoadRecords(knightsMemberIndex);
                }
            }
            else if (url.StartsWith(RECORDS_MYSELF) == true)
            {
                // HTMLソース取得
                var source = await chromeBrowser.GetBrowser().MainFrame.GetSourceAsync();

                try
                {
                    // 出力
                    DisplayOutput("自分の成績表を解析中...");
                    // 解析
                    var nextMemberIndex = knightsMemberIndex;

                    if (CreateRecordsMyself(out nextMemberIndex, knightsMemberIndex, source) == true)
                    {
                        // 次の社員へ
                        knightsMemberIndex = nextMemberIndex;

                        if (knightsMemberIndex < knightsMemberList.Count)
                        {
                            // 成績を読み込む
                            LoadRecords(knightsMemberIndex);
                        }
                        else
                        {
                            // 出力
                            DisplayOutput("サーバー更新中: PRA（過去 5週間）");

                            // サーバーへ反映
                            if (UpdatePRA() == true)
                            {
                                // 閉じる
                                this.InvokeOnUiThreadIfRequired(() => this.Close());
                                // ログ出力
                                Program.WriteLog("PRA（過去 5週間）を更新しました。");
                            }
                            else
                            {
                                // ログ出力
                                Program.WriteLog("PRA（過去 5週間）の更新に失敗しました。");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ログ出力
                    Program.WriteExceptionLog(ex);
                    // ソース出力
                    WriteFetchSource(url, source);
                }
            }
            else if (url.StartsWith(RECORDS_OTHER) == true)
            {
                // HTMLソース取得
                var source = await chromeBrowser.GetBrowser().MainFrame.GetSourceAsync();

                try
                {
                    // 出力
                    DisplayOutput("社員の成績表を解析中...");
                    // 解析
                    var nextMemberIndex = knightsMemberIndex;

                    if (CreateRecordsOther(out nextMemberIndex, knightsMemberIndex, source) == true)
                    {
                        // 次の社員へ
                        knightsMemberIndex = nextMemberIndex;

                        if (knightsMemberIndex < knightsMemberList.Count)
                        {
                            // 成績を読み込む
                            LoadRecords(knightsMemberIndex);
                        }
                        else
                        {
                            DisplayOutput("サーバー更新中: PRA（過去 3ヶ月）");

                            // サーバーへ反映
                            if (UpdatePRA() == true)
                            {
                                // ログ出力
                                Program.WriteLog("PRA（過去 3ヶ月）を更新しました。");
                            }
                            else
                            {
                                // ログ出力
                                Program.WriteLog("PRA（過去 3ヶ月）の更新に失敗しました。");
                            }
                            // 閉じる
                            this.InvokeOnUiThreadIfRequired(() => this.Close());
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ログ出力
                    Program.WriteExceptionLog(ex);
                    // ソース出力
                    WriteFetchSource(url, source);
                }
            }
            // 処理完了
            scrapingProgress = false;
        }

        /// <summary>
        /// サーバーの Membersテーブルを更新
        /// </summary>
        /// <returns></returns>
        private bool UpdatePRA()
        {
            try
            {
                // 全社員
                foreach (var member in knightsMemberList)
                {
                    if (member.UpdatePRA() == false)
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Program.WriteExceptionLog(e);

                return false;
            }

            return true;
        }
        #endregion

        #region PRA（今週）
        /// <summary>
        /// PRA（今週）
        /// </summary>
        private async void FetchThisWeek()
        {
            // トップページの場合
            var url = new Uri(textUrl.Text).ToString();

            if (url.Equals(START_URL) == true)
            {
                // 待機
                System.Threading.Thread.Sleep(ACCESS_WAIT);
                // 今週の PRAを読み込む
                var memberUrl = new Uri(THISWEEK_RECORDS + THISWEEK_RECORDS_FIRST + GetRndParameter());

                LoadUrl(memberUrl.ToString());
            }
            else if (url.StartsWith(THISWEEK_RECORDS) == true)
            {
                // HTMLソース取得
                var source = await chromeBrowser.GetBrowser().MainFrame.GetSourceAsync();

                try
                {
                    // 出力
                    DisplayOutput("個人週間を解析中...");
                    // 解析
                    if (CreateThisWeek(knightsMemberPage, source) == true)
                    {
                        // 次のページへ
                        knightsMemberPage++;
                        // パラメーター
                        var memberCount = 5 * knightsMemberPage;

                        if (memberCount <= knightsMemberCount)
                        {
                            memberCount -= 5;

                            if (memberCount >= 0)
                            {
                                // 待機
                                System.Threading.Thread.Sleep(ACCESS_WAIT);
                                // 個人週間を読み込む
                                var memberUrl = new Uri(THISWEEK_RECORDS + $"/{memberCount}" + THISWEEK_RECORDS_NEXT + GetRndParameter());

                                LoadUrl(memberUrl.ToString());
                            }
                        }
                        else
                        {
                            DisplayOutput("サーバー更新中: PRA（今週）");

                            // 既存にあり取得にない社員のプロデューサー種類を変更（XMLファイルへは保存しない）
                            var deleteMemberList = knightsMemberList.Where(d => d.Exists == false);

                            foreach (var member in deleteMemberList)
                            {
                                // 社員の場合
                                if (member.ProducerType == KnightsMemberData.TYPE_EMPLOYEE)
                                {
                                    // 退職者
                                    member.ProducerType = KnightsMemberData.TYPE_RETIREE;
                                }
                            }
                            // サーバーへ反映
                            if (UpdateThisWeek() == true)
                            {
                                // ログ出力
                                Program.WriteLog("PRA（今週）を更新しました。");
                            }
                            else
                            {
                                // ログ出力
                                Program.WriteLog("PRA（今週）の更新に失敗しました。");
                            }
                            // 閉じる
                            this.InvokeOnUiThreadIfRequired(() => this.Close());
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ログ出力
                    Program.WriteExceptionLog(ex);
                    // ソース出力
                    WriteFetchSource(url, source);
                }
            }
            // 処理完了
            scrapingProgress = false;
        }

        /// <summary>
        /// サーバーの Membersテーブルを更新
        /// </summary>
        /// <returns></returns>
        private bool UpdateThisWeek()
        {
            try
            {
                // 全社員
                foreach (var member in knightsMemberList)
                {
                    if (member.UpdateThisWeek() == false)
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Program.WriteExceptionLog(e);

                return false;
            }

            return true;
        }
        #endregion

        #region イベント順位
        /// <summary>
        /// イベント順位
        /// </summary>
        private void FetchEventRank()
        {
            var filePath = System.IO.Path.Combine(Application.StartupPath, KNIGHTS_MEMBER_FILE);

            // 既にファイルがある場合
            if (System.IO.File.Exists(filePath) == true)
            {
                // 既存データを読み込む
                var xd = XDocument.Load(filePath);
                // 社員一覧を取得
                var rt = xd.Element(MEMBER_XML_ROOT);
                var members = rt.Elements().Select(e => e);
                // 社員データを作成
                foreach (var member in members)
                {
                    var data = new KnightsMemberData();

                    // モバゲーID
                    data.ID = member.Attribute(MEMBER_XML_ID).Value;
                    // プロデューサー名
                    data.Name = member.Attribute(MEMBER_XML_NAME).Value;
                    // プロデューサー種類
                    data.ProducerType = int.Parse(member.Attribute(MEMBER_XML_TYPE).Value);
                    // 退職者でない場合
                    if (data.ProducerType != KnightsMemberData.TYPE_RETIREE)
                    {
                        // 追加
                        knightsMemberList.Add(data);
                    }
                }
                // 社員数を取得
                knightsMemberCount = knightsMemberList.Count(d => d.ProducerType == KnightsMemberData.TYPE_EMPLOYEE);
            }
            else
            {
                // 社員一覧が必要
                throw new ApplicationException("社員一覧が存在しません。");
            }
            // 待機
            System.Threading.Thread.Sleep(ACCESS_WAIT);
            // 出力
            DisplayOutput("イベント順位を取得中...");
            // イベント明細IDを取得
            var fesEventDetailId = GetCinderellaFesEventID();
            // 全社員
            foreach (var member in knightsMemberList)
            {
                // 現役社員のみ
                if (member.ProducerType == KnightsMemberData.TYPE_EMPLOYEE)
                {
                    var eventName = string.Empty;

                    // イベント順位
                    member.EventRank = GetCinderellaEventRank(member.ID, fesEventDetailId, out eventName);
                    // イベント明細ID
                    member.EventDetailID = fesEventDetailId;
                    // イベント名
                    member.EventName = eventName;
                }
            }
            // イベント情報チェック
            foreach (var member in knightsMemberList)
            {
                // 現役社員のみ
                if (member.ProducerType == KnightsMemberData.TYPE_EMPLOYEE)
                {
                    // イベント明細ID 最大値
                    if (member.EventDetailID != fesEventDetailId)
                    {
                        // イベント順位
                        member.EventRank = 0;
                        // イベント名
                        member.EventName = string.Empty;
                    }
                }
            }
            // 出力
            DisplayOutput("サーバー更新中: イベント順位");
            // サーバーへ反映
            if (UpdateEventRank() == true)
            {
                // ログ出力
                Program.WriteLog("イベント順位を更新しました。");
            }
            else
            {
                // ログ出力
                Program.WriteLog("イベント順位の更新に失敗しました。");
            }
            // 閉じる
            this.InvokeOnUiThreadIfRequired(() => this.Close());
        }

        /// <summary>
        /// サーバーの Membersテーブルを更新
        /// </summary>
        /// <returns></returns>
        private bool UpdateEventRank()
        {
            try
            {
                // 全社員
                foreach (var member in knightsMemberList)
                {
                    // 現役社員のみ
                    if (member.ProducerType == KnightsMemberData.TYPE_EMPLOYEE)
                    {
                        if (member.UpdateEventRank() == false)
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Program.WriteExceptionLog(e);

                return false;
            }

            return true;
        }
        #endregion
        #endregion

        #region CefSharp
        /// <summary>
        /// CefSharpの初期化
        /// </summary>
        public void InitializeChromium()
        {            
            var applicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var browserSubprocessPath = System.IO.Path.Combine(applicationBase, "CefSharp.BrowserSubprocess.exe");

            if (System.IO.File.Exists(browserSubprocessPath) == true)
            {
                var settings = new CefSettings();

                // 実行ファイルのパス
                settings.BrowserSubprocessPath = browserSubprocessPath;
                // User-Agentを iPadにする
                settings.UserAgent = "Mozilla/5.0 (iPad; CPU OS 12_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/72.0.3626.101 Mobile/15E148 Safari/605.1";
                // CefSharpの初期化
                Cef.Initialize(settings, performDependencyCheck: false, browserProcessHandler: null);
                // ブラウザーを生成（モバマスのトップページ）
                chromeBrowser = new ChromiumWebBrowser("http://sp.pf.mbga.jp/12008305/");
                // キャッシュの設定
                var requestContextSettings = new RequestContextSettings() { CachePath = applicationBase };

                chromeBrowser.RequestContext = new RequestContext(requestContextSettings);
                chromeBrowser.TabIndex = 0;
                chromeBrowser.TabStop = true;

                chromeBrowser.IsBrowserInitializedChanged += OnIsBrowserInitializedChanged;
                chromeBrowser.LoadingStateChanged += OnLoadingStateChanged;
                chromeBrowser.ConsoleMessage += OnBrowserConsoleMessage;
                chromeBrowser.StatusMessage += OnBrowserStatusMessage;
                chromeBrowser.TitleChanged += OnBrowserTitleChanged;
                chromeBrowser.AddressChanged += OnBrowserAddressChanged;
                // フォームへ追加
                this.Controls.Add(chromeBrowser);
                // 前面へ移動
                chromeBrowser.BringToFront();
                // フォーム全体
                chromeBrowser.Dock = DockStyle.Fill;
            }
        }

        /// <summary>
        /// ブラウザー初期化 イベント
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">イベント引数</param>
        private void OnIsBrowserInitializedChanged(object sender, EventArgs e)
        {
            DisplayOutput($"IsBrowserInitialized: always true (v75.1.141)");

            var b = ((ChromiumWebBrowser)sender);

            this.InvokeOnUiThreadIfRequired(() => b.Focus());
        }

        /// <summary>
        /// コンソールメッセージ
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="args">イベント引数</param>
        private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs args)
        {
            DisplayOutput($"Line: {args.Line}, Source: {args.Source}, Message: {args.Message}");
        }

        /// <summary>
        /// ステータスメッセージ
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="args">イベント引数</param>
        private void OnBrowserStatusMessage(object sender, StatusMessageEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => labelStatus.Text = args.Value);
        }

        /// <summary>
        /// メッセージ出力
        /// </summary>
        /// <param name="output">出力テキスト</param>
        public void DisplayOutput(string output)
        {
            this.InvokeOnUiThreadIfRequired(() => labelOutput.Text = output);
        }

        /// <summary>
        /// 読み込みステータス変更
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="args">イベント引数</param>
        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            var outputMessage = (args.IsLoading == true) ? "読み込み中..." : "読み込み完了";

            DisplayOutput(outputMessage);
            // 読み込み終了
            if (args.IsLoading == false && scrapingProgress == false)
            {
                var url = textUrl.Text;

                // 解析開始
                scrapingProgress = true;
                // モバゲーのログイン画面以外
                if (url.StartsWith("https://connect.mobage.jp/login") == false)
                {
                    // タイマー
                    timerDocumentCompleted.Enabled = true;
                }
            }
        }

        /// <summary>
        /// ブラウザー タイトル変更
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="args">イベント引数</param>
        private void OnBrowserTitleChanged(object sender, TitleChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => Text = args.Title);
        }

        /// <summary>
        /// ブラウザー アドレス変更
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="args">イベント引数</param>
        private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => textUrl.Text = args.Address);
        }

        /// <summary>
        /// ページ遷移
        /// </summary>
        /// <param name="url">URL</param>
        private void LoadUrl(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                chromeBrowser.Load(url);
            }
        }
        #endregion

        #region フォーム
        /// <summary>
        /// フォームの設定を復元
        /// </summary>
        /// <param name="rect">矩形</param>
        /// <param name="state">状態</param>
        protected void RestoreFormSettings(Rectangle rect, FormWindowState state)
        {
            // 初期化されているか？
            if (rect.IsEmpty == false)
            {
                // サイズを復元
                this.Bounds = new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height);
            }
            else
            {
                // 画面中央
                this.CenterToScreen();
            }
            // 状態を復元
            this.WindowState = state;
            // Loadイベント中でもフォームが表示されてしまうので強制的に再描画
            this.Refresh();
        }
        #endregion

        #region 解析
        /// <summary>
        /// 乱数パラメータを取得
        /// </summary>
        /// <returns>乱数</returns>
        private string GetRndParameter()
        {
            var r = random.Next(100000000, 999999999);

            return r.ToString();
        }

        /// <summary>
        /// PRAを読み込む
        /// </summary>
        /// <param name="index">社員のインデックス</param>
        private void LoadRecords(int index)
        {
            var memberData = knightsMemberList[index];

            // 待機
            System.Threading.Thread.Sleep(ACCESS_WAIT);
            // 成績を読み込む                                
            var memberUrl = new Uri(memberData.RankingAwardURL);

            LoadUrl(memberUrl.ToString());
        }

        /// <summary>
        /// 社員一覧を作成
        /// </summary>
        /// <param name="page">ページ位置</param>
        /// <param name="source">HTML</param>
        /// <returns>成否</returns>
        private bool CreateKnightsMember(int page, string source)
        {
            // 最初のページ
            if (knightsMemberPage == 1)
            {
                // 社員数の場所を検索
                var startIndex = source.IndexOf(MEMBER_COUNT_KEY);

                if (startIndex >= 0)
                {
                    startIndex += MEMBER_COUNT_KEY.Length;
                    // 社員数を抽出
                    var length = source.IndexOf('<', startIndex) - startIndex;
                    var memberCount = source.Substring(startIndex, length);
                    // 数値変換
                    knightsMemberCount = int.Parse(memberCount);
                }
            }
            // System.Text.Encoding.CodePagesが必要
            var parser = new AngleSharp.Html.Parser.HtmlParser();
            var htmlDocument = parser.ParseDocument(source);
            var urlElements = htmlDocument.QuerySelectorAll("a");

            foreach (var element in urlElements)
            {
                var href = element.GetAttribute("href");
                var url = string.Empty;

                try
                {
                    url = new Uri(href).ToString();

                    if (url.StartsWith(MEMBER_INFO_KEY) == true)
                    {
                        // 名前
                        var memberName = element.InnerHtml;
                        // モバゲーID
                        var startIndex = MEMBER_INFO_KEY.Length;
                        var length = url.IndexOf('%', startIndex) - startIndex;
                        var memberID = url.Substring(startIndex, length);

                        System.Diagnostics.Debug.WriteLine($"ID: {memberID}, Name: {memberName}, URL: {url}");

                        var member = new KnightsMemberData();

                        member.ID = memberID;
                        member.Name = memberName;

                        knightsMemberList.Add(member);
                    }
                }
                catch (UriFormatException)
                {
                    url = string.Empty;
                }
            }

            return true;
        }

        /// <summary>
        /// 自分以外の PRA
        /// </summary>
        /// <param name="nextIndex">次のインデックス</param>
        /// <param name="index">社員のインデックス</param>
        /// <param name="source">HTML</param>
        /// <returns>成否</returns>
        private bool CreateRecordsOther(out int nextIndex, int index, string source)
        {
            nextIndex = index;

            if (index >= knightsMemberList.Count)
            {
                return false;
            }
            // 現在の社員
            var currentMember = knightsMemberList[index];
            // System.Text.Encoding.CodePagesが必要
            var parser = new AngleSharp.Html.Parser.HtmlParser();
            var htmlDocument = parser.ParseDocument(source);
            var urlElements = htmlDocument.QuerySelectorAll("li");
            var separator = new string[] { "<span class=\"blue\">", "</span>", "<span class=\"yellow\">", "位(", "人)" };

            foreach (var element in urlElements)
            {
                var innerHtml = element.InnerHtml;

                if (innerHtml.StartsWith("<span class=\"blue\">") == true)
                {
                    var dataArray = innerHtml.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                    currentMember.AddRecords(dataArray);
                }
            }
            // 次の成績へ
            currentMember.RankingAwardIndex = currentMember.RankingAwardIndex + 1;
            // 次の URLを確認
            if (currentMember.RankingAwardURL.Length == 0)
            {
                // 次の社員へ
                nextIndex++;
            }

            return true;
        }

        /// <summary>
        /// 自分の PRA
        /// </summary>
        /// <param name="nextIndex">次のインデックス</param>
        /// <param name="index">社員のインデックス</param>
        /// <param name="source">HTML</param>
        /// <returns>成否</returns>
        private bool CreateRecordsMyself(out int nextIndex, int index, string source)
        {
            nextIndex = index;

            if (index >= knightsMemberList.Count)
            {
                return false;
            }
            // 現在の社員
            var currentMember = knightsMemberList[index];
            // System.Text.Encoding.CodePagesが必要
            var parser = new AngleSharp.Html.Parser.HtmlParser();
            var htmlDocument = parser.ParseDocument(source);
            var urlElements = htmlDocument.QuerySelectorAll("li");
            var separator = new string[] { "<span class=\"blue\">", "</span>", "<span class=\"yellow\">", "位(", "人)" };

            foreach (var element in urlElements)
            {
                var innerHtml = element.InnerHtml;

                if (innerHtml.StartsWith("<span class=\"blue\">") == true)
                {
                    var dataArray = innerHtml.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                    currentMember.AddRecords(dataArray);
                }
            }
            // 次の成績へ
            currentMember.RankingAwardIndex = currentMember.RankingAwardIndex + 1;
            // 次の URLを確認
            if (currentMember.RankingAwardURL.Length == 0)
            {
                // 次の社員へ
                nextIndex++;
            }

            return true;
        }

        /// <summary>
        /// 今週の PRA
        /// </summary>
        /// <param name="page">ページ</param>
        /// <param name="source">HTML</param>
        /// <returns></returns>
        private bool CreateThisWeek(int page, string source)
        {
            // 最初のページ
            if (knightsMemberPage == 1)
            {
                var filePath = System.IO.Path.Combine(Application.StartupPath, KNIGHTS_MEMBER_FILE);

                // 既にファイルがある場合
                if (System.IO.File.Exists(filePath) == true)
                {
                    // 既存データを読み込む
                    var xd = XDocument.Load(filePath);
                    // 社員一覧を取得
                    var rt = xd.Element(MEMBER_XML_ROOT);
                    var members = from e in rt.Elements() select e;
                    // 社員データを作成
                    foreach (var member in members)
                    {
                        var data = new KnightsMemberData();

                        // モバゲーID
                        data.ID = member.Attribute(MEMBER_XML_ID).Value;
                        // プロデューサー名
                        data.Name = member.Attribute(MEMBER_XML_NAME).Value;
                        // プロデューサー種類
                        data.ProducerType = int.Parse(member.Attribute(MEMBER_XML_TYPE).Value);
                        // 退職者でない場合
                        if (data.ProducerType != KnightsMemberData.TYPE_RETIREE)
                        {
                            // 追加
                            knightsMemberList.Add(data);
                        }
                    }
                    // 社員数を取得
                    knightsMemberCount = knightsMemberList.Count(d => d.ProducerType == KnightsMemberData.TYPE_EMPLOYEE);
                }
                else
                {
                    // 社員一覧が必要
                    return false;
                }
            }
            // System.Text.Encoding.CodePagesが必要
            var parser = new AngleSharp.Html.Parser.HtmlParser();
            var htmlDocument = parser.ParseDocument(source);
            var tableElements = htmlDocument.QuerySelectorAll("td");
            var separator = new string[] { "<span class=\"blue\">", "</span>" };

            foreach (var element in tableElements)
            {
                var innerHtml = element.InnerHtml;
                var tdDocument = parser.ParseDocument(innerHtml);
                var urlElements = tdDocument.QuerySelectorAll("a");

                foreach (var innerElement in urlElements)
                {
                    var href = innerElement.GetAttribute("href");
                    var url = string.Empty;

                    try
                    {
                        url = new Uri(href).ToString();

                        if (url.StartsWith(MEMBER_INFO_KEY) == true)
                        {
                            // 名前
                            var memberName = innerElement.InnerHtml;
                            // モバゲーID
                            var startIndex = MEMBER_INFO_KEY.Length;
                            var length = url.IndexOf('%', startIndex) - startIndex;
                            var memberID = url.Substring(startIndex, length);

                            System.Diagnostics.Debug.WriteLine($"ID: {memberID}, Name: {memberName}, URL: {url}");
                            // 既存データを検索
                            var member = knightsMemberList.FirstOrDefault(d => d.ID.Equals(memberID) == true);

                            if (member == null)
                            {
                                // 新規作成
                                member = new KnightsMemberData();

                                member.ID = memberID;
                                member.Name = memberName;

                                knightsMemberList.Add(member);
                            }
                            // 分解
                            var dataArray = innerHtml.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                            member.AddThisWeek(dataArray);
                            // 既存データあり
                            member.Exists = true;
                        }
                    }
                    catch (UriFormatException)
                    {
                        url = string.Empty;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// ログ出力
        /// <param name="source">ログ テキスト</param>
        /// </summary>
        private string WriteFetchSource(string url, string source)
        {
            var writeFolder = System.IO.Path.Combine(Application.StartupPath, "FetchLog", "sp.pf.mbga.jp");

            // 存在確認
            if (System.IO.Directory.Exists(writeFolder) == false)
            {
                // フォルダの作成
                System.IO.Directory.CreateDirectory(writeFolder);
            }
            // パス作成
            var logFilePath = System.IO.Path.Combine(writeFolder, $"{DateTime.Today:yyyyMMdd}_source.txt");

            using (var sw = new System.IO.StreamWriter(logFilePath, true))
            {
                // 日時を出力
                sw.WriteLine($"----- {DateTime.Now.ToString("HH:mm:ss")}, URL:{url}");
                // ログ出力
                sw.WriteLine(source);
            }

            return logFilePath;
        }
        #endregion

        #region Cinderella API
        /// <summary>
        /// 直近フェスのイベントIDを取得
        /// </summary>
        /// <returns>イベントID</returns>
        private int GetCinderellaFesEventID()
        {
            var result = 0;

            // Cinderella API
            var request = new RestSharp.RestRequest(RestSharp.Method.GET);
            // パラメーター
            request.AddParameter("eventTypeId", "6");   // 6:フェス
            request.AddParameter("pretty", "false");
            // API呼び出し
            var url = "https://imcg.pink-check.school/api/v1/events";
            var content = CinderellaApiGetData(url, request);

            try
            {
                // イベント情報
                var eventModel = Newtonsoft.Json.JsonConvert.DeserializeObject<CinderellaEventModel>(content);

                foreach (var cinderellaEvent in eventModel.CinderellaEvents)
                {
                    // イベント基本情報
                    var eventInfoModel = Newtonsoft.Json.JsonConvert.DeserializeObject<CinderellaEventInfoModel>(cinderellaEvent.eventContent);
                    // イベント明細情報
                    var eventDetailModel = Newtonsoft.Json.JsonConvert.DeserializeObject<CinderellaEventDetailModel>(eventInfoModel.details);

                    foreach (var eventDetail in eventDetailModel.CinderellaDetails)
                    {
                        // イベント明細IDを取得
                        var eventDetailId = eventDetail.eventDetailId;

                        // 最大値なら
                        if (eventDetailId > result)
                        {
                            // 更新
                            result = eventDetailId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ログ出力
                Program.WriteLog(ex.ToString());
                // ソース出力
                WriteFetchJSON(url, content);
            }

            return result;
        }

        /// <summary>
        /// イベントの順位を取得
        /// </summary>
        /// <returns>イベント順位</returns>
        private int GetCinderellaEventRank(string ID, int fesEventDetailId, out string eventName)
        {
            var result = 0;

            // 初期化
            eventName = string.Empty;
            // Cinderella API
            var request = new RestSharp.RestRequest(RestSharp.Method.GET);
            // パラメーター
            request.AddParameter("pretty", "false");
            // API呼び出し
            var url = $"https://imcg.pink-check.school/api/v1/producers/{ID}";
            var content = CinderellaApiGetData(url, request);

            try
            {
                // プロデューサー明細情報
                var eventModel = Newtonsoft.Json.JsonConvert.DeserializeObject<CinderellaProducersModel>(content);
                var cinderellaProducers = eventModel?.CinderellaProducers.OrderByDescending(d => d.eventDetailId);

                if (cinderellaProducers != null)
                {
                    // イベント明細IDの降順
                    foreach (var cinderellaProducer in cinderellaProducers)
                    {
                        // 対象イベントのみ
                        if (cinderellaProducer.eventDetailId == fesEventDetailId)
                        {
                            // イベント順位
                            result = cinderellaProducer.eventRank ?? 0;
                            // イベント名
                            eventName = cinderellaProducer.eventName ?? string.Empty;

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ログ出力
                Program.WriteExceptionLog(ex);
                // ソース出力
                WriteFetchJSON(url, content);
            }

            return result;
        }

        /// <summary>
        /// API呼び出し
        /// </summary>
        /// <param name="request">RestSharpオブジェクト</param>
        /// <returns>取得内容</returns>
        private string CinderellaApiGetData(string uriString, RestSharp.RestRequest request)
        {
            var result = string.Empty;
            var exception = default(Exception);
            var retryCount = 0;

            do
            {
                var client = new RestSharp.RestClient();

                // URL
                client.BaseUrl = new Uri(uriString);
                // 実行
                var response = client.Execute(request);
                // 例外を取得
                exception = response.ErrorException;

                if (exception == null)
                {
                    // レスポンス コード
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // 取得結果
                        result = response.Content;
                        // 待機
                        System.Threading.Thread.Sleep(CINDERELLA_API_WAIT);
                    }
                    else
                    {
                        throw new ApplicationException($"Cinderella APIがステータス コード = {response.StatusCode} を返しました。");
                    }
                }
                else
                {
                    // ログ出力
                    Program.WriteExceptionLog(exception);
                    // リトライ回数
                    if (retryCount > 10)
                    {
                        throw new ApplicationException("API呼び出しのリトライ回数が上限を超えました。");
                    }
                    else
                    {
                        // リトライ回数
                        retryCount++;
                        // 待機
                        System.Threading.Thread.Sleep(CINDERELLA_API_WAIT * 40);
                    }
                }
            }
            while (exception != null);

            return result;
        }

        /// <summary>
        /// ログ出力
        /// <param name="source">ログ テキスト</param>
        /// </summary>
        private string WriteFetchJSON(string url, string source)
        {
            var writeFolder = System.IO.Path.Combine(Application.StartupPath, "FetchLog", "imcg.pink-check.school");

            // 存在確認
            if (System.IO.Directory.Exists(writeFolder) == false)
            {
                // フォルダの作成
                System.IO.Directory.CreateDirectory(writeFolder);
            }
            // パス作成
            var logFilePath = System.IO.Path.Combine(writeFolder, $"{DateTime.Today:yyyyMMdd}_JSON.txt");

            using (var sw = new System.IO.StreamWriter(logFilePath, true))
            {
                // 日時を出力
                sw.WriteLine($"----- {DateTime.Now.ToString("HH:mm:ss")}, URL:{url}");
                // ログ出力
                sw.WriteLine(source);
            }

            return logFilePath;
        }
        #endregion
    }
}
