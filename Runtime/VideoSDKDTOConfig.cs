namespace live.videosdk
{
    [System.Serializable]
    public class VideoSDKDTOConfig
    {
        public VideoSDKDTOConfig(
             string logType,
             string logText,
             Attributes attributes
        )
        {
            this.logType = logType;
            this.logText = logText;
            this.attributes = attributes;
        }

        public string logType { get; }

        public string logText { get; }

        public Attributes attributes { get; }
    }


}
