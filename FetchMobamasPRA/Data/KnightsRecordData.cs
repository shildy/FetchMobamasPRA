using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FetchMobamasPRA.Data
{
    /// <summary>
    /// プロデューサー (成績) データ
    /// </summary>
    class KnightsRecordData
    {
        #region プロパティ
        public DateTime RecordDate { get; set; } = DateTime.MaxValue;
        public byte ProducerRank { get; set; } = byte.MinValue;
        public int WeeklyRank { get; set; } = int.MaxValue;
        public int FanCount { get; set; } = int.MinValue;
        #endregion

        /// <summary>
        /// プロデューサー ランクの設定
        /// </summary>
        /// <param name="rankText">プロデューサー ランク（文字列）</param>
        public void SetProducerRank(string rankText)
        {
            var rankArray = new string[] { "F", "E", "D", "C", "B", "A", "S", "SS", "S3", "S4", "S5", "S6", "S7", "S8", "S9", "S10" };
            var rank = 1;

            // 初期化
            ProducerRank = 0;
            // 検索
            foreach (var rankValue in rankArray)
            {
                // 比較
                if (rankValue.ToLower().Equals(rankText.ToLower()) == true)
                {
                    ProducerRank = (byte)rank;

                    break;
                }
                // 次のランクへ
                rank++;
            }
        }
    }
}
