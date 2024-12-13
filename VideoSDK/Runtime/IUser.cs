using System;

namespace live.videosdk
{
    public interface IUser
    {
        bool IsLocal { get; }
        string ParticipantId { get; }
        string ParticipantName { get; }
        bool MicEnabled { get; }
        bool CamEnabled { get; }

        event Action<byte[]> OnVideoFrameReceivedCallback;
        event Action<string> OnStreamEnabledCallaback;
        event Action<string> OnStreamDisabledCallaback;
        event Action OnParticipantLeftCallback;

        void ToggleMic(bool status);
        void ToggleWebCam(bool status);
        void OnParticipantLeft();
    }
}



