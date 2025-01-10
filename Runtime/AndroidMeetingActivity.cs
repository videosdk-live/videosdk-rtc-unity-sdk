using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
// Callback class to receive messages from Android
namespace live.videosdk
{
#if UNITY_ANDROID
    internal sealed class AndroidMeetingActivity : IMeetingActivity
    {
        private AndroidJavaClass _pluginClass;
        private AndroidJavaObject _currentActivity;
        private IMeetingCallback _meetCallback;

        public AndroidMeetingActivity(IMeetingCallback meetCallback)
        {
            _meetCallback = meetCallback;
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                _currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                _pluginClass = new AndroidJavaClass(Meeting.packageName);
            }
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

        public void SubscribeToAudioDeviceChanged(Action<string,string[]> callback)
        {
            _meetCallback.SubscribeToAudioDeviceChanged(callback);
        }

        public void UnsubscribeFromAudioDeviceChanged(Action<string,string[]> callback)
        {
            _meetCallback.UnsubscribeFromAudioDeviceChanged(callback);
        }

        public void SubscribeToFetchAudioDevice(Action<string[]> callback)
        {
            _meetCallback.SubscribeToFetchAudioDevice(callback);
        }

        public void UnsubscribeFromFetchAudioDevice(Action<string[]> callback)
        {
            _meetCallback.UnsubscribeFromFetchAudioDevice(callback);
        }

        #endregion

        public void CreateMeetingId(string jsonResponse, string token, Action<string> onSuccess)
        {
            try
            {
                //Debug.LogError("Meet Response : " + jsonResponse);
                JObject result = JObject.Parse(jsonResponse);

                var meetingId= result["roomId"].ToString();
                onSuccess?.Invoke(meetingId);
            }
            catch (JsonReaderException ex)
            {
                Debug.LogError($"Json respose is Invalid {ex.Message}");
            }

        }
       
        public void JoinMeeting(string token, string jsonResponse, string name, bool micEnable, bool camEnable, string participantId)
        {
            try
            {
                JObject result = JObject.Parse(jsonResponse);

                var meetingId = result["meetingId"].ToString();
               
                _pluginClass.CallStatic("joinMeeting", _currentActivity, token, meetingId, name, micEnable, camEnable, participantId);
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.StackTrace);
            }
            
        }

        public void LeaveMeeting()
        {
            _pluginClass.CallStatic("leave");
        }

    }

#endif
}
