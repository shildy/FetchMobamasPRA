using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Cinderella API プロデューサー情報用
/// </summary>
/// <remarks>参考にしたページ: https://blog.beachside.dev/entry/2016/08/30/190000 </remarks>
namespace FetchMobamasPRA.CinderellaAPI
{
    /// <summary>
    /// Cinderella API プロデューサー明細情報 モデル
    /// </summary>
    [JsonConverter(typeof(CinderellaProducersModelConverter))]
    class CinderellaProducersModel
    {
        public List<CinderellaProducer> CinderellaProducers { get; set; }
    }

    /// <summary>
    /// プロデューサー明細情報
    /// </summary>
    public class CinderellaProducer
    {
        // イベント明細ID（キー）
        public int eventDetailId { get; set; }
        // イベント名
        public string eventName { get; set; }
        // イベント終了日時
        public DateTime eventEndDateTime { get; set; }
        // イベント最終順位
        public int? eventRank { get; set; }
        // イベント最終pt
        public long? eventPoint { get; set; }
        // モバゲーID
        public int? mobageId { get; set; }
        // 所属プロダクションID
        public int? productionId { get; set; }
        // 所属プロダクション名
        public string productionName { get; set; }
        // ユニット名
        public string unitName { get; set; }
        // 肩書ID
        public int? katagakiId { get; set; }
        // プロデューサー名
        public string producerName { get; set; }
        // プロデューサーランク（S10ランク: 16 ～ Fランク: 1）
        public int? producerRank { get; set; }
        // ファン数
        public long? fans { get; set; }
        // リーダーアイドルハッシュ
        public string leaderHash { get; set; }
        // レベル
        public int? level { get; set; }
        // 属性(Cute/Cool/Passion)
        public string classification { get; set; }
        // LIVEバトル回数
        public int? battle { get; set; }
        // LIVEバトル勝利数
        public int? victory { get; set; }
        // アルバム写真数
        public int? album { get; set; }
        // 親愛度MAX人数
        public int? shinaiMax { get; set; }
        // ホシイモノ1カードハッシュ
        public string favorite1Hash { get; set; }
        // ホシイモノ1カード名
        public string favorite1Name { get; set; }
        // ホシイモノ2カードハッシュ
        public string favorite2Hash { get; set; }
        // ホシイモノ2カード名
        public string favorite2Name { get; set; }
        // ホシイモノ3カードハッシュ
        public string favorite3Hash { get; set; }
        // ホシイモノ3カード名
        public string favorite3Name { get; set; }
    }

    /// <summary>
    /// プロデューサー明細情報 コンバーター
    /// </summary>
    public class CinderellaProducersModelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(List<CinderellaProducer>);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var cinderellaProducers = serializer.Deserialize<JObject>(reader).Properties().Select(p =>
            {
                var cinderellaProducer = p.Value.ToObject<CinderellaProducer>();

                cinderellaProducer.eventDetailId = int.Parse(p.Name);

                return cinderellaProducer;
            }).ToList();

            return new CinderellaProducersModel() { CinderellaProducers = cinderellaProducers };
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
