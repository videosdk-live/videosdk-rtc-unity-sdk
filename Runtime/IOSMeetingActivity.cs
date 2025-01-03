﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
// Callback class to receive messages from Android
namespace live.videosdk
{

    #if UNITY_IOS
    internal sealed class IOSMeetingActivity :IMeetingActivity
    {
        private IMeetingCallback _meetCallback;
        public IOSMeetingActivity(IMeetingCallback meetCallback)
        {
            _meetCallback = meetCallback;
        }

        #region meet-events

        // Public methods to subscribe and unsubscribe to events
        public void SubscribeToMeetingJoined(Action<string, string, string, bool> callback)
        {
            _meetCallback.SubscribeToMeetingJoined(callback);
        }

        public void UnsubscribeFromMeetingJoined(Action<string, string, string, bool> callback)
        {
            _meetCallback.UnsubscribeFromMeetingJoined(callback);
        }

        public void SubscribeToMeetingLeft(Action<string, string, bool> callback)
        {
            _meetCallback.SubscribeToMeetingLeft(callback);
        }

        public void UnsubscribeFromMeetingLeft(Action<string, string, bool> callback)
        {
            _meetCallback.UnsubscribeFromMeetingLeft(callback);
        }

        public void SubscribeToParticipantJoined(Action<string, string, bool> callback)
        {
            _meetCallback.SubscribeToParticipantJoined(callback);
        }

        public void UnsubscribeFromParticipantJoined(Action<string, string, bool> callback)
        {
            _meetCallback.UnsubscribeFromParticipantJoined(callback);
        }

        public void SubscribeToParticipantLeft(Action<string, string, bool> callback)
        {
            _meetCallback.SubscribeToParticipantLeft(callback);
        }

        public void UnsubscribeFromParticipantLeft(Action<string, string, bool> callback)
        {
            _meetCallback.UnsubscribeFromParticipantLeft(callback);
        }

        public void SubscribeToMeetingStateChanged(Action<string> callback)
        {
            _meetCallback.SubscribeToMeetingStateChanged(callback);
        }

        public void UnsubscribeFromMeetingStateChanged(Action<string> callback)
        {
            _meetCallback.UnsubscribeFromMeetingStateChanged(callback);
        }

        public void SubscribeToError(Action<string> callback)
        {
            _meetCallback.SubscribeToError(callback);
        }

        public void UnsubscribeFromError(Action<string> callback)
        {
            _meetCallback.UnsubscribeFromError(callback);
        }

        #endregion

        public void CreateMeetingId(string jsonResponse, string token, Action<string> onSuccess)
        {
            try
            {
                //Debug.LogError("Meet Response : " + jsonResponse);
                JObject result = JObject.Parse(jsonResponse);
                var meetingId = result["roomId"].ToString();
                onSuccess?.Invoke(meetingId);
            }
            catch (JsonReaderException ex)
            {
                Debug.LogError($"Json respose is Invalid {ex.Message}");
            }

        }

        public void JoinMeeting(string token, string meetingId, string name, bool micEnable, bool camEnable, string participantId)
        {
            joinMeeting(token, meetingId, name, micEnable, camEnable, participantId);
        }

        public void LeaveMeeting()
        {
            leave();
        }

        [DllImport("__Internal")]
        private static extern void leave();

        [DllImport("__Internal")]
        private static extern void joinMeeting(string token, string meetingId, string name, bool micEnable, bool camEnable, string participantId);

      
    }

#endif
}
