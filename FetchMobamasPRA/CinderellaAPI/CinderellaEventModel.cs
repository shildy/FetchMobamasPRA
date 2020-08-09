using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Cinderella API イベント情報用
/// </summary>
/// <remarks>参考にしたページ: https://blog.beachside.dev/entry/2016/08/30/190000 </remarks>
namespace FetchMobamasPRA.CinderellaAPI
{
    /// <summary>
    /// Cinderella API イベント情報 モデル
    /// </summary>
    [JsonConverter(typeof(CinderellaEventModelConverter))]
    public class CinderellaEventModel
    {
        public List<CinderellaEvent> CinderellaEvents { get; set; }
    }

    /// <summary>
    /// イベント情報
    /// </summary>
    public class CinderellaEvent
    {
        public string mobageEventId { get; set; }   // 不定
        public string eventTypeName { get; set; }
        public string name { get; set; }
        public string eventContent { get; set; }
    }

    /// <summary>
    /// イベント情報 コンバーター
    /// </summary>
    public class CinderellaEventModelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(List<CinderellaEvent>);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var cinderellaEvents = serializer.Deserialize<JObject>(reader).Properties().Select(p =>
            {
                var cinderellaEvent = p.Value.ToObject<CinderellaEvent>();

                cinderellaEvent.mobageEventId = p.Name;
                cinderellaEvent.eventContent = p.Value.ToString();

                return cinderellaEvent;
            } ).ToList();

            return new CinderellaEventModel() { CinderellaEvents = cinderellaEvents };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// Cinderella API イベント基本情報 モデル
    /// </summary>
    [JsonConverter(typeof(CinderellaEventInfoModelConverter))]
    public class CinderellaEventInfoModel
    {
        public int eventTypeId { get; set; }
        public string mobageEventId { get; set; }
        public int eventInfoId { get; set; }
        public string details { get; set; }         // Content
    }

    /// <summary>
    /// イベント基本情報 コンバーター
    /// </summary>
    public class CinderellaEventInfoModelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(CinderellaEventInfoModel);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = new CinderellaEventInfoModel();
            var properties = serializer.Deserialize<JObject>(reader).Properties();

            foreach (var property in properties)
            {
                switch (property.Name)
                {
                    case "eventTypeId":
                        result.eventTypeId = int.Parse(property.Value.ToString());

                        break;
                    case "mobageEventId":
                        result.mobageEventId = property.Value.ToString();

                        break;
                    case "eventInfoId":
                        result.eventInfoId = int.Parse(property.Value.ToString());

                        break;
                    case "details":
                        result.details = property.Value.ToString();

                        break;
                }
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Cinderella API イベント明細情報 モデル
    /// </summary>
    [JsonConverter(typeof(CinderellaEventDetailModelConverter))]
    public class CinderellaEventDetailModel
    {
        public List<CinderellaEventDetail> CinderellaDetails { get; set; }
    }

    /// <summary>
    /// イベント明細情報
    /// </summary>
    public class CinderellaEventDetail
    {
        public int eventDetailId { get; set; }
        public DateTime finalRankingDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        public DateTime beginDateTime { get; set; }
        public int? orgRewardMembers { get; set; }
        public int? orgRewardBorderRank { get; set; }
        public int? orgRewardRankingTypeId { get; set; }
        public int? take1CardRank { get; set; }
        public int? take2CardRank { get; set; }
        public string eventDetailTypeId { get; set; }
        public int sequence { get; set; }
        public string remark { get; set; }
        public string cardName { get; set; }
        public string cardHash { get; set; }
        public string explanation { get; set; }
    }

    /// <summary>
    /// イベント明細情報 コンバーター
    /// </summary>
    public class CinderellaEventDetailModelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(List<CinderellaEventDetail>);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var cinderellaDetails = serializer.Deserialize<JObject>(reader).Properties().Select(p =>
            {
                var cinderellaDetail = p.Value.ToObject<CinderellaEventDetail>();

                cinderellaDetail.eventDetailId = int.Parse(p.Name);

                return cinderellaDetail;
            }).ToList();

            return new CinderellaEventDetailModel() { CinderellaDetails = cinderellaDetails };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
