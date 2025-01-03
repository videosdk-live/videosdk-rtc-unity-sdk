namespace live.videosdk
{
    internal interface IMeetingControlls
    {
        void ToggleMic(bool status, string Id);
        void ToggleWebCam(bool status, string Id);
        void PauseStream(string paticipantId,string kind);
        void ResumeStream(string paticipantId, string kind);
    }
}

