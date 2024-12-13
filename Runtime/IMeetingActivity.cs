using System;

namespace live.videosdk
{
    internal interface IMeetingActivity
    {
        event Action<string, string, string, bool> OnMeetingJoinedCallback;
        event Action<string, string, bool> OnMeetingLeftCallback;
        event Action<string, string, bool> OnParticipantJoinedCallback;
        event Action<string, string, bool> OnParticipantLeftCallback;
        event Action<string> OnErrorCallback;
        event Action<string> OnMeetingStateChangedCallback;
        void CreateMeetingId(string jsonResponse, string token, Action<string> onSuccess);
        void JoinMeeting(string token, string meetingId, string name, bool micEnable, bool camEnable, string participantId);
        void LeaveMeeting();
    }
}



