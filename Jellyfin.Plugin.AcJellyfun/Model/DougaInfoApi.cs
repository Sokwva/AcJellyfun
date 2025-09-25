using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.AcJellyfun.Model
{
    public class DougaInfoApiUser
    {
        [JsonPropertyName("id")]
        public string ID { get; set; }

        [JsonPropertyName("headUrl")]
        public string HeadUrl { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class DougaInfoApiChannel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("parentName")]
        public string ParentName { get; set; }
    }

    public class DougaInfoApiData
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("commentCount")]
        public int CommentCount { get; set; }

        [JsonPropertyName("bananaCount")]
        public int BananaCount { get; set; }

        [JsonPropertyName("stowCount")]
        public int StowCount { get; set; }

        [JsonPropertyName("channel")]
        public DougaInfoApiChannel Channel { get; set; }

        [JsonPropertyName("likeCount")]
        public int LikeCount { get; set; }

        [JsonPropertyName("danmakuCount")]
        public int DanmakuCount { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("viewCount")]
        public int ViewCount { get; set; }

        [JsonPropertyName("user")]
        public DougaInfoApiUser User { get; set; }

        [JsonPropertyName("createTimeMillis")]
        public long CreateTimeMillis { get; set; }
        [JsonPropertyName("createTime")]
        public string CreateTime { get; set; }
        [JsonPropertyName("coverUrl")]
        public string CoverURL { get; set; }
        [JsonPropertyName("shareUrl")]
        public string ShareURL { get; set; }
    }

    public class DougaInfoApiResp
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public DougaInfoApiData Data { get; set; }
    }
}