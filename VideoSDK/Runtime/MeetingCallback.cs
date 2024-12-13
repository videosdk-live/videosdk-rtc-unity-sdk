using UnityEngine;
namespace live.videosdk
{
    internal abstract class MeetingCallback : AndroidJavaProxy
    {

        public MeetingCallback() : base("live.videosdk.unity.android.callbacks.MeetingCallback") { }

        public abstract void OnMeetingJoined(string meetingId, string Id, string name);
        public abstract void OnMeetingLeft(string Id, string name);

        public abstract void OnParticipantJoined(string Id, string name);

        public abstract void OnParticipantLeft(string Id, string name);

        public abstract void OnMeetingStateChanged(string state);

        public abstract void OnError(string jsonString);
    }
}
