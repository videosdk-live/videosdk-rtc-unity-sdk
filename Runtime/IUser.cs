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
        event Action<StreamKind> OnStreamEnabledCallaback;
        event Action<StreamKind> OnStreamDisabledCallaback;
        event Action OnParticipantLeftCallback;
        event Action<StreamKind> OnStreamPausedCallaback;
        event Action<StreamKind> OnStreamResumedCallaback;

        void ToggleMic(bool status);
        void ToggleWebCam(bool status, string customVideoStream);
        void PauseStream(StreamKind kind);
        void ResumeStream(StreamKind kind);
        void OnParticipantLeft();

    }
}



