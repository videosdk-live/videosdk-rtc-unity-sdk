namespace live.videosdk
{
    internal interface IMeetingControlls
    {
        void ToggleMic(bool isLocal, bool status, string Id);
        void ToggleWebCam(bool isLocal, bool status, string Id, string customVideoSrtream);
        void Leave(string Id);
        void PauseStream(StreamKind kind, string Id);
        void ResumeStream(StreamKind kind, string Id);
    }
}

