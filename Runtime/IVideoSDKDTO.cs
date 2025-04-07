namespace live.videosdk
{
    internal interface IVideoSDKDTO
    {
        void Initialize(string sessionId, string jwt, string roomId,string peerId, bool enabledLogs, string dtoUri, string packageVersion);
        void SendDTO(string logtype, string logtext);
    }


}
