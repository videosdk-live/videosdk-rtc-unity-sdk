using System;

namespace live.videosdk
{
    internal interface IMeetingActivity
    {
        void CreateMeetingId(string jsonResponse, string token, Action<string> onSuccess);
        void JoinMeeting(string token, string meetingId, string name, bool micEnable, bool camEnable, string participantId);
        void LeaveMeeting();
        void SubscribeToError(Action<string> callback);
        void SubscribeToMeetingJoined(Action<string, string, string, bool> callback);
        void SubscribeToMeetingLeft(Action<string, string, bool> callback);
        void SubscribeToMeetingStateChanged(Action<string> callback);
        void SubscribeToParticipantJoined(Action<string, string, bool> callback);
        void SubscribeToParticipantLeft(Action<string, string, bool> callback);
        void UnsubscribeFromError(Action<string> callback);
        void UnsubscribeFromMeetingJoined(Action<string, string, string, bool> callback);
        void UnsubscribeFromMeetingLeft(Action<string, string, bool> callback);
        void UnsubscribeFromMeetingStateChanged(Action<string> callback);
        void UnsubscribeFromParticipantJoined(Action<string, string, bool> callback);
        void UnsubscribeFromParticipantLeft(Action<string, string, bool> callback);
    }
}



