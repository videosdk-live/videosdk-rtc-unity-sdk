using System;
// Callback class to receive messages from Android
namespace live.videosdk
{
    internal interface IMeetingCallback
    {
        void SubscribeToAudioDeviceChanged(Action<string, string[]> callback);
        void SubscribeToError(Action<string> callback);
        void SubscribeToExternalCallHangup(Action callback);
        void SubscribeToExternalCallRinging(Action callback);
        void SubscribeToExternalCallStarted(Action callback);
        void SubscribeToFetchAudioDevice(Action<string[]> callback);
        void SubscribeToMeetingJoined(Action<string, string, string, bool, bool, string, string, string, string> callback);
        void SubscribeToMeetingLeft(Action<string, string, bool> callback);
        void SubscribeToMeetingStateChanged(Action<string> callback);
        void SubscribeToParticipantJoined(Action<string, string, bool> callback);
        void SubscribeToParticipantLeft(Action<string, string, bool> callback);
        void SubscribeToSpeakerChanged(Action<string> callback);
        void UnsubscribeFromAudioDeviceChanged(Action<string, string[]> callback);
        void UnsubscribeFromError(Action<string> callback);
        void UnsubscribeFromExternalCallHangup(Action callback);
        void UnsubscribeFromExternalCallRinging(Action callback);
        void UnsubscribeFromExternalCallStarted(Action callback);
        void UnsubscribeFromFetchAudioDevice(Action<string[]> callback);
        void UnsubscribeFromMeetingJoined(Action<string, string, string, bool, bool, string, string, string, string> callback);
        void UnsubscribeFromMeetingLeft(Action<string, string, bool> callback);
        void UnsubscribeFromMeetingStateChanged(Action<string> callback);
        void UnsubscribeFromParticipantJoined(Action<string, string, bool> callback);
        void UnsubscribeFromParticipantLeft(Action<string, string, bool> callback);
        void UnsubscribeFromSpeakerChanged(Action<string> callback);
    }

//#endif
}
