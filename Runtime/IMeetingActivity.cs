using System;
using UnityEngine;

namespace live.videosdk
{
    internal interface IMeetingActivity
    {
        void CreateMeetingId(string jsonResponse, string token, Action<string> onSuccess);
        void JoinMeeting(string token, string jsonResponse, string name, bool micEnable, bool camEnable, string participantId, string packageVersion, CustomStreamData encorderConfig);
        void LeaveMeeting();
        string GetAudioDevices();
        string GetVideoDevices();

        string GetSelectedAudioDevice();
        string GetSelectedVideoDevice();

        void ChangeAudioDevice(string deviceLabel);
        void ChangeVideoDevice(string deviceLabel);
        void SetVideoEncoderConfig(string videoConfig);
        void SubscribeToError(Action<string> callback);
        void SubscribeToExternalCallHangup(Action callback);
        void SubscribeToExternalCallRinging(Action callback);
        void SubscribeToExternalCallStarted(Action callback);


        //void SubscribeToAvailableAudioDevices(Action<string, string> callback);
        void SubscribeToAudioDeviceChanged(Action<string, string> callback);

        //void SubscribeToAvailableVideoDevices(Action<string, string> callback);
        void SubscribeToVideoDeviceChanged(Action<string, string> callback);


        void SubscribeToMeetingJoined(Action<string, string, string, bool, bool, string, string, string, string> callback);
        void SubscribeToMeetingLeft(Action<string, string, bool> callback);
        void SubscribeToMeetingStateChanged(Action<string> callback);
        void SubscribeToParticipantJoined(Action<string, string, bool> callback);
        void SubscribeToParticipantLeft(Action<string, string, bool> callback);
        void SubscribeToSpeakerChanged(Action<string> callback);
        void SubscribeToResumedAllStreams(Action<string> callback);
        void SubscribeToPausedAllStreams(Action<string> callback);

        //void UnsubscribeFromAvailableAudioDevices(Action<string, string> callback);
        void UnsubscribeFromAudioDeviceChanged(Action<string, string> callback);

        //void UnsubscribeFromAvailableVideoDevices(Action<string, string> callback);
        void UnsubscribeFromVideoDeviceChanged(Action<string, string> callback);

        void UnsubscribeFromError(Action<string> callback);
        void UnsubscribeFromExternalCallHangup(Action callback);
        void UnsubscribeFromExternalCallRinging(Action callback);
        void UnsubscribeFromExternalCallStarted(Action callback);
        void UnsubscribeFromMeetingJoined(Action<string, string, string, bool, bool, string, string, string, string> callback);
        void UnsubscribeFromMeetingLeft(Action<string, string, bool> callback);
        void UnsubscribeFromMeetingStateChanged(Action<string> callback);
        void UnsubscribeFromParticipantJoined(Action<string, string, bool> callback);
        void UnsubscribeFromParticipantLeft(Action<string, string, bool> callback);
        void UnsubscribeFromSpeakerChanged(Action<string> callback);
        void UnsubscribeFromPausedAllStreams(Action<string> callback);
        void UnsubscribeFromResumedAllStreams(Action<string> callback);
        void PauseAllStreams(string kind);
        void ResumeAllStreams(string kind);
    }
}



