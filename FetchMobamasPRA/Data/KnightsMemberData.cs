using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RestSharp;

namespace FetchMobamasPRA.Data
{
    /// <summary>
    /// プロデューサー (社員) データ
    /// </summary>
    class KnightsMemberData
    {
        #region 定数
        // プロデューサー種類
        public const int TYPE_EMPLOYEE = 0;
        public const int TYPE_RETIREE = 1;
        public const int TYPE_SUBSCRIBER = 2;
        // API待機時間
        private const int API_WAIT = 750;
        #endregion

        #region プロパティ
        // モバゲーID
        public string ID { get; set; } = string.Empty;
        // プロデューサー名
        public string Name { get; set; } = string.Empty;
        // プロデューサー種類
        public int ProducerType { get; set; } = TYPE_EMPLOYEE;
        // イベント明細ID
        public int EventDetailID { get; set; } = 0;
        // イベント名
        public string EventName { get; set; } = string.Empty;
        // イベント順位
        public int EventRank { get; set; } = 0;
        // PRAインデックス
        public int RankingAwardIndex { get; set; } = 0;
        // PRAの URL
        public string RankingAwardURL
        {
            get
            {
                var result = string.Empty;

                if (RankingAwardIndex < rankingAwardUrlList.Count)
                {
                    result = rankingAwardUrlList[RankingAwardIndex];
                }

                return result;
            }
        }
        // 既存データ有無
        public bool Exists { get; set; } = false;
        #endregion

        #region メンバー変数
        // PRAの URLリスト
        private List<string> rankingAwardUrlList = new List<string>();
        // PRAの成績リスト
        private List<KnightsRecordData> recordList = new List<KnightsRecordData>();
        #endregion

        /// <summary>
        /// PRAの URLを追加
        /// </summary>
        /// <param name="url">PRAの URL</param>
        public void AddRankingAwardUrl(string url)
        {
            rankingAwardUrlList.Add(url);
        }

        /// <summary>
        /// PRAの成績を追加
        /// </summary>
        /// <param name="records">PRAの成績</param>
        public void AddRecords(string [] records)
        {
            var data = new KnightsRecordData();

            // 日付
            var parseValue = DateTime.MaxValue;

            if (DateTime.TryParseExact(records[0].Trim(), "yyyy-MM-dd", System.Globalization.DateTimeFormatInfo.CurrentInfo, System.Globalization.DateTimeStyles.None, out parseValue) == true)
            {
                // 成功
                data.RecordDate = parseValue;
            }
            // プロデューサーランク
            var producerRank = records[1].Trim().Replace("ランク", string.Empty);

            data.SetProducerRank(producerRank);
            // 順位
            data.WeeklyRank = int.Parse(records[2].Trim());
            // ファン数
            data.FanCount = int.Parse(records[4].Trim());

            recordList.Add(data);
        }

        /// <summary>
        /// 今週の成績を追加
        /// </summary>
        /// <param name="records">今週の成績</param>
        public void AddThisWeek(string[] records)
        {
            var data = new KnightsRecordData();

            // 日付
            data.RecordDate = DateTime.Today;
            // プロデューサーランク
            var producerRank = records[5].Trim().Replace("<br>", string.Empty);

            data.SetProducerRank(producerRank);
            // ファン数
            data.FanCount = int.Parse(records[7].Trim().Replace("<br>", string.Empty));

            recordList.Add(data);
        }

        /// <summary>
        /// 直近5週のPRAを Webサイトへ登録
        /// </summary>
        /// <returns>登録結果</returns>
        public bool UpdatePRA()
        {
            var result = (int?)1;
            var request = new RestRequest(Method.POST);

            // 日付の降順リスト
            var sortedList = recordList.OrderByDescending(d => d.RecordDate);

            var fan05 = GetFanCount(sortedList, 4);
            var fan04 = GetFanCount(sortedList, 3);
            var fan03 = GetFanCount(sortedList, 2);
            var fan02 = GetFanCount(sortedList, 1);
            var fan01 = GetFanCount(sortedList, 0);
            // パラメーター設定
            request.AddCookie("csrfToken", "mobamasPRA");
            request.AddParameter("username", ID);
            request.AddParameter("producer_name", Name);
            request.AddParameter("producer_rank", GetLatestRank(sortedList));
            request.AddParameter("fan05", fan05);
            request.AddParameter("fan04", fan04);
            request.AddParameter("fan03", fan03);
            request.AddParameter("fan02", fan02);
            request.AddParameter("fan01", fan01);
            request.AddParameter("latest", GetLatestDate(sortedList));
            request.AddParameter("producer_type", ProducerType);
            request.AddParameter("_csrfToken", "mobamasPRA");
            // データ更新
            var content = UpdateMemberPostData(request);

            try
            {
                // 解析開始
                var xd = XDocument.Parse(content);
                var xml = xd.Element("update_member");
                // 処理結果
                result = GetIntValue(xml, "result");

                if ((result ?? 1) != 0)
                {
                    // ログ出力
                    Program.WriteLog($"モバゲーID:{ID}, コード:{GetStringValue(xml, "success_code")}, 詳細:{GetStringValue(xml, "success_detail")}");
                }
            }
            catch (System.Xml.XmlException)
            {
                // ログ出力
                Program.WriteLog($"モバゲーID: {ID}, 詳細:XML解析で例外が発生しました。");
            }

            return (result == 0);
        }

        /// <summary>
        /// 今週のPRAを Webサイトへ登録
        /// </summary>
        /// <returns>登録結果</returns>
        public bool UpdateThisWeek()
        {
            var result = (int?)1;
            var request = new RestRequest(Method.POST);

            // 日付の降順リスト
            var sortedList = recordList.OrderByDescending(d => d.RecordDate);

            var fan00 = GetFanCount(sortedList, 0);
            // パラメーター設定
            request.AddCookie("csrfToken", "mobamasPRA");
            request.AddParameter("username", ID);
            request.AddParameter("producer_name", Name);
            request.AddParameter("producer_rank", GetLatestRank(sortedList));
            request.AddParameter("fan00", fan00);
            request.AddParameter("producer_type", ProducerType);
            request.AddParameter("_csrfToken", "mobamasPRA");
            // データ更新
            var content = UpdateMemberPostData(request);

            try
            {
                // 解析開始
                var xd = XDocument.Parse(content);
                var xml = xd.Element("update_member");
                // 処理結果
                result = GetIntValue(xml, "result");

                if ((result ?? 1) != 0)
                {
                    // ログ出力
                    Program.WriteLog($"モバゲーID:{ID}, コード:{GetStringValue(xml, "success_code")}, 詳細:{GetStringValue(xml, "success_detail")}");
                }
            }
            catch (System.Xml.XmlException)
            {
                // ログ出力
                Program.WriteLog($"モバゲーID: {ID}, 詳細:XML解析で例外が発生しました。");
            }

            return (result == 0);
        }

        /// <summary>
        /// イベントの結果を Webサイトへ登録
        /// </summary>
        /// <returns>登録結果</returns>
        public bool UpdateEventRank()
        {
            var result = (int?)1;
            var request = new RestRequest(Method.POST);

            // パラメーター設定
            request.AddCookie("csrfToken", "mobamasPRA");
            request.AddParameter("username", ID);
            request.AddParameter("event_name", EventName);
            request.AddParameter("event_rank", EventRank);
            request.AddParameter("_csrfToken", "mobamasPRA");
            // データ更新
            var content = UpdateMemberPostData(request);

            try
            {
                // 解析開始
                var xd = XDocument.Parse(content);
                var xml = xd.Element("update_member");
                // 処理結果
                result = GetIntValue(xml, "result");

                if ((result ?? 1) != 0)
                {
                    // ログ出力
                    Program.WriteLog($"モバゲーID:{ID}, コード:{GetStringValue(xml, "success_code")}, 詳細:{GetStringValue(xml, "success_detail")}");
                }
            }
            catch (System.Xml.XmlException)
            {
                // ログ出力
                Program.WriteLog($"モバゲーID: {ID}, 詳細:XML解析で例外が発生しました。");
            }

            return (result == 0);
        }

        /// <summary>
        /// ファン数の取得
        /// </summary>
        /// <param name="sortedList">週ごとのリスト</param>
        /// <param name="index">週数</param>
        /// <returns>ファン数</returns>
        private int GetFanCount(IOrderedEnumerable<KnightsRecordData> sortedList, int index)
        {
            var result = 0;

            if (index < sortedList.Count())
            {
                var data = sortedList.ElementAt(index);

                result = data.FanCount;
            }

            return result;
        }

        /// <summary>
        /// 最新日付の取得
        /// </summary>
        /// <param name="sortedList">週ごとのリスト</param>
        /// <returns>最新日付</returns>
        private DateTime? GetLatestDate(IOrderedEnumerable<KnightsRecordData> sortedList)
        {
            var data = sortedList.FirstOrDefault();

            return data?.RecordDate.Date;
        }

        /// <summary>
        /// 最新順位の取得
        /// </summary>
        /// <param name="sortedList">週ごとのリスト</param>
        /// <returns>最新順位</returns>
        private byte GetLatestRank(IOrderedEnumerable<KnightsRecordData> sortedList)
        {
            var data = sortedList.FirstOrDefault();

            return data?.ProducerRank ?? 0;
        }

        /// <summary>
        /// API呼び出し
        /// </summary>
        /// <param name="request">RestSharpオブジェクト</param>
        /// <returns>取得内容</returns>
        private string UpdateMemberPostData(RestRequest request)
        {
            var result = string.Empty;
            var exception = default(Exception);
            var retryCount = 0;

            do
            {
                var client = new RestClient();

                // APIキー
                request.AddParameter("api_key", "A34njff9ntUjB6Gpx33aEWCzYHNCamRt");
                // URL (CakePHPで作成したサイト)
                client.BaseUrl = new Uri("http://localhost/members/update_member");
                // ログ出力
                Program.WriteLog($"モバゲーID:{ID}, {client.BaseUrl.ToString()}");
                // 実行
                var response = client.Execute(request);
                // 例外を取得
                exception = response.ErrorException;

                if (exception == null)
                {
                    // 取得結果
                    result = response.Content;
                    // ログ出力
                    WriteContent("UpdateMember", result, "update_member");
                    // 待機
                    System.Threading.Thread.Sleep(API_WAIT);
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
                        System.Threading.Thread.Sleep(API_WAIT * 40);
                    }
                }
            }
            while (exception != null);

            return result;
        }

        #region RESTユーティリティ
        /// <summary>
        /// API取得内容のログ出力
        /// </summary>
        /// <param name="method">処理の種類</param>
        /// <param name="content">取得内容</param>
        private void WriteContent(string method, string content, string rootElement)
        {
            var resultCode = string.Empty;

            try
            {
                // XML解析
                var xd = XDocument.Parse(content);
                var xml = xd.Element(rootElement);
                // 結果コード
                resultCode = xml.Element("result").Value ?? "null";
            }
            catch (System.Xml.XmlException)
            {
                resultCode = "exception";
            }
            // 正常終了以外
            if (resultCode.Equals("0") == false)
            {
                // 出力先フォルダ
                var writeFolder = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "FetchLog", method, DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"));
                // 存在確認
                if (System.IO.Directory.Exists(writeFolder) == false)
                {
                    // フォルダの作成
                    System.IO.Directory.CreateDirectory(writeFolder);
                }
                // ファイル名
                var fileName = string.Format("{0}-{1}.xml", DateTime.Now.ToString("yyyyMMdd_HHmmssfff"), resultCode);
                // パス作成
                var contentFilePath = System.IO.Path.Combine(writeFolder, fileName);
                // ファイルを開く
                using (var sw = new System.IO.StreamWriter(contentFilePath))
                {
                    // ログ出力
                    sw.Write(content);
                }
            }
        }

        /// <summary>
        /// 整数の取得
        /// </summary>
        /// <param name="element">要素</param>
        /// <param name="name">要素名</param>
        /// <returns>取得値</returns>
        private int? GetIntValue(XElement element, string name)
        {
            var result = default(int?);

            // 値を取得
            var value = element?.Element(name)?.Value ?? string.Empty;
            // トリミング
            value = value.Trim();

            if (value.Length > 0)
            {
                // 変換
                var parseValue = int.MaxValue;

                // 変換
                if (int.TryParse(value, System.Globalization.NumberStyles.AllowThousands, null, out parseValue) == true)
                {
                    // 成功
                    result = parseValue;
                }
            }

            return result;
        }

        /// <summary>
        /// 整数の取得
        /// </summary>
        /// <param name="element">要素</param>
        /// <param name="name">要素名</param>
        /// <returns>取得値</returns>
        private long? GetLongValue(XElement element, string name)
        {
            var result = default(long?);

            // 値を取得
            var value = element?.Element(name)?.Value ?? string.Empty;
            // トリミング
            value = value.Trim();

            if (value.Length > 0)
            {
                // 変換
                var parseValue = long.MaxValue;

                // 変換
                if (long.TryParse(value, System.Globalization.NumberStyles.AllowThousands, null, out parseValue) == true)
                {
                    // 成功
                    result = parseValue;
                }
            }

            return result;
        }

        /// <summary>
        /// 文字列の取得
        /// </summary>
        /// <param name="element">要素</param>
        /// <param name="name">要素名</param>
        /// <returns>取得文字列</returns>
        private string GetStringValue(XElement element, string name)
        {
            // 値を取得
            var value = element?.Element(name)?.Value ?? string.Empty;

            return value.Trim();
        }

        /// <summary>
        /// 日付の取得
        /// </summary>
        /// <param name="element">要素</param>
        /// <param name="name">要素名</param>
        /// <returns>取得値</returns>
        private DateTime? GetDateTimeValue(XElement element, string name)
        {
            var result = default(DateTime?);

            // 値を取得
            var value = element?.Element(name)?.Value ?? string.Empty;
            // トリミング
            value = value.Trim();

            if (value.Length > 0)
            {
                // 変換
                var parseValue = DateTime.MaxValue;

                // 変換
                if (DateTime.TryParseExact(value, "yyyyMdHmmss", System.Globalization.DateTimeFormatInfo.CurrentInfo, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out parseValue) == true)
                {
                    // 成功
                    result = parseValue;
                }
            }

            return result;
        }
        #endregion
    }
}
