namespace live.videosdk
{
    internal interface IMeetingControlls
    {
        void ToggleMic(bool status, string Id);
        void ToggleWebCam(bool status, string Id);
        void PauseStream(StreamKind kind, string Id);
        void ResumeStream(StreamKind kind, string Id);
       
    }
}

