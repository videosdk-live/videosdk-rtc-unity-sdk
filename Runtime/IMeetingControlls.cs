namespace live.videosdk
{
    internal interface IMeetingControlls
    {
        void ToggleMic(bool status, string Id);
        void ToggleWebCam(bool status, string Id);
    }
}

