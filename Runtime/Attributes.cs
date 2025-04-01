namespace live.videosdk
{
    [System.Serializable]
    public class Attributes
    {
        public Attributes(
             string roomId,
             string peerId,
             string sessionId,
             string SDK,
             string modelName,
             string osVersion,
             string SDKVersion
        )
        {
            this.roomId = roomId;
            this.peerId = peerId;
            this.SDK = SDK;
            this.sessionId = sessionId;
            this.modelName = modelName;
            this.osVersion = osVersion;
            this.SDKVersion = SDKVersion;
        }

        public string roomId { get; }

        public string peerId { get; }

        public string sessionId { get; }

        public string SDK { get; }

        public string modelName { get; }

        public string osVersion { get; }

        public string SDKVersion { get; }
    }


}
