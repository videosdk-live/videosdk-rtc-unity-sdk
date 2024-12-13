namespace live.videosdk
{
    internal interface IMeetingControlls
    {
        void LeaveMeeting();
        void ToggleMic(bool status);
        void ToggleWebCam(bool status);
    }
}

