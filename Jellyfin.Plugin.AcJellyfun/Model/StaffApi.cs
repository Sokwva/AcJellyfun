using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.AcJellyfun.Model
{
    public class StaffApi
    {
        [JsonPropertyName("result")]
        public int Result { get; set; }

        [JsonPropertyName("staffInfos")]
        public List<StaffItem> StaffInfo { get; set; }

        [JsonPropertyName("upInfo")]
        public List<StaffItem> UpInfo { get; set; }
    }

    public class StaffItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("staffRoleName")]
        public string StaffRoleName { get; set; }

        [JsonPropertyName("headUrl")]
        public string HeadURL { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; }
    }
}