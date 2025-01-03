using System;
using UnityEngine;
namespace live.videosdk
{
#if UNITY_ANDROID
    internal sealed class AndroidMeetingCallback : AndroidJavaProxy, IMeetingCallback
    {
        private static AndroidMeetingCallback _instance;
        public static AndroidMeetingCallback Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AndroidMeetingCallback();
                }
                return _instance;
            }
        }
        private AndroidMeetingCallback() : base("live.videosdk.unity.android.callbacks.MeetingCallback") {
            RegisterNativeMettingCallBack();
        }

        private void RegisterNativeMettingCallBack()
        {
            using (var pluginClass = new AndroidJavaClass(Meeting.packageName))
            {
                pluginClass.CallStatic("registerMeetingCallback", this);
            }
            
        }

        // Public methods to subscribe and unsubscribe to events
        public void SubscribeToMeetingJoined(Action<string, string, string, bool> callback)
        {
            OnMeetingJoinedCallback += callback;
        }

        public void UnsubscribeFromMeetingJoined(Action<string, string, string, bool> callback)
        {
            OnMeetingJoinedCallback -= callback;
        }

        public void SubscribeToMeetingLeft(Action<string, string, bool> callback)
        {
            OnMeetingLeftCallback += callback;
        }

        public void UnsubscribeFromMeetingLeft(Action<string, string, bool> callback)
        {
            OnMeetingLeftCallback -= callback;
        }

        public void SubscribeToParticipantJoined(Action<string, string, bool> callback)
        {
            OnParticipantJoinedCallback += callback;
        }

        public void UnsubscribeFromParticipantJoined(Action<string, string, bool> callback)
        {
            OnParticipantJoinedCallback -= callback;
        }

        public void SubscribeToParticipantLeft(Action<string, string, bool> callback)
        {
            OnParticipantLeftCallback += callback;
        }

        public void UnsubscribeFromParticipantLeft(Action<string, string, bool> callback)
        {
            OnParticipantLeftCallback -= callback;
        }

        public void SubscribeToMeetingStateChanged(Action<string> callback)
        {
            OnMeetingStateChangedCallback += callback;
        }

        public void UnsubscribeFromMeetingStateChanged(Action<string> callback)
        {
            OnMeetingStateChangedCallback -= callback;
        }

        public void SubscribeToError(Action<string> callback)
        {
            OnErrorCallback += callback;
        }

        public void UnsubscribeFromError(Action<string> callback)
        {
            OnErrorCallback -= callback;
        }

        public void SubscribeToCallCallHangup(Action callback)
        {
            OnCallHangupCallback += callback;
        }

        public void UnsubscribeFromCallCallHangup(Action callback)
        {
            OnCallHangupCallback -= callback;
        }
        public void SubscribeToCallRinging(Action callback)
        {
            OnCallRingingCallback += callback;
        }

        public void UnsubscribeFromCallRinging(Action callback)
        {
            OnCallRingingCallback -= callback;
        }
        public void SubscribeToCallStarted(Action callback)
        {
            OnCallStartedCallback += callback;
        }

        public void UnsubscribeFromCallStarted(Action callback)
        {
            OnCallStartedCallback -= callback;
        }


        private static event Action<string, string, string, bool> OnMeetingJoinedCallback;
        private static event Action<string, string, bool> OnMeetingLeftCallback;
        private static event Action<string, string, bool> OnParticipantJoinedCallback;
        private static event Action<string, string, bool> OnParticipantLeftCallback;
        private static event Action<string> OnErrorCallback;
        private static event Action<string> OnMeetingStateChangedCallback;

        private void OnMeetingJoined(string meetingId, string Id, string name)
        {
            OnMeetingJoinedCallback?.Invoke(meetingId, Id, name, true);
        }
        private void OnMeetingLeft(string Id, string name)
        {
            OnMeetingLeftCallback?.Invoke(Id, name, true);
        }

        private void OnParticipantJoined(string Id, string name)
        {
            OnParticipantJoinedCallback?.Invoke(Id, name, false);
        }

        private void OnParticipantLeft(string Id, string name)
        {
            OnParticipantLeftCallback?.Invoke(Id, name, false);
        }

        private void OnMeetingStateChanged(string state)
        {
            OnMeetingStateChangedCallback?.Invoke(state);
        }

        private void OnError(string jsonString)
        {
            OnErrorCallback?.Invoke(jsonString);
        }

        private void OnCallStarted()
        {
            OnCallStartedCallback?.Invoke();
        }
        private void OnCallRinging()
        {
            OnCallRingingCallback?.Invoke();
        }
        private void OnCallCallHangup()
        {
            OnCallHangupCallback?.Invoke();
        }

    }
#endif
}
