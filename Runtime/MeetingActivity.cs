using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
// Callback class to receive messages from Android
namespace live.videosdk
{
    internal sealed class MeetingActivity : MeetingCallback, IMeetingActivity
    {
        private AndroidJavaClass _pluginClass;
        private AndroidJavaObject _currentActivity;
        public event Action<string, string, string, bool> OnMeetingJoinedCallback;
        public event Action<string, string, bool> OnMeetingLeftCallback;
        public event Action<string, string, bool> OnParticipantJoinedCallback;
        public event Action<string, string, bool> OnParticipantLeftCallback;
        public event Action<string> OnErrorCallback;
        public event Action<string> OnMeetingStateChangedCallback;

        public MeetingActivity(AndroidJavaClass pluginClass, AndroidJavaObject currentActivity/*, IEnumerable<ParticipantManager> participantsList*/)
        {
            _pluginClass = pluginClass;
            _currentActivity = currentActivity;
            RegisterNativeMettingCallBack();
        }

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
        private void RegisterNativeMettingCallBack()
        {
            _pluginClass.CallStatic("registerMeetingCallback", this);
        }

        public void JoinMeeting(string token, string meetingId, string name, bool micEnable, bool camEnable, string participantId)
        {

            if (Application.platform == RuntimePlatform.Android)
            {
                _pluginClass.CallStatic("joinMeeting", _currentActivity, token, meetingId, name, micEnable, camEnable, participantId);
                return;
            }
            Debug.LogError("VideoSDK not supported on this platform.");
        }

        public void LeaveMeeting()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                _pluginClass.CallStatic("leave");
                return;
            }
            Debug.LogError("VideoSDK not supported on this platform.");
        }

        public override void OnMeetingJoined(string meetingId, string Id, string name)
        {
            OnMeetingJoinedCallback?.Invoke(meetingId, Id, name, true);
        }

        public override void OnMeetingLeft(string Id, string name)
        {
            OnMeetingLeftCallback?.Invoke(Id, name, true);
        }

        public override void OnParticipantJoined(string Id, string name)
        {
            OnParticipantJoinedCallback?.Invoke(Id, name,false);
        }

        public override void OnParticipantLeft(string Id, string name)
        {
            OnParticipantLeftCallback?.Invoke(Id, name, false);

        }

        public override void OnMeetingStateChanged(string state)
        {
            OnMeetingStateChangedCallback?.Invoke(state);
        }

        public override void OnError(string jsonString)
        {
            OnErrorCallback?.Invoke(jsonString);
        }

      
    }


}
