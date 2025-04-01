using Newtonsoft.Json;
namespace live.videosdk
{
    [System.Serializable]
    public class Error
    {
        [JsonProperty(PropertyName="code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName= "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

    }

}

