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
        private AndroidMeetingCallback() : base("live.videosdk.unity.android.callbacks.MeetingCallback")
        {
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
        public void SubscribeToMeetingJoined(Action<string, string, string, bool, bool, string, string, string, string> callback)
        {
            OnMeetingJoinedCallback += callback;
        }

        public void UnsubscribeFromMeetingJoined(Action<string, string, string, bool, bool, string, string, string, string> callback)
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


        public void SubscribeToAudioDeviceChanged(Action<string, string> callback) => OnAudioDeviceChangedCallback += callback;
        public void UnsubscribeFromAudioDeviceChanged(Action<string, string> callback) => OnAudioDeviceChangedCallback -= callback;


        public void SubscribeToSpeakerChanged(Action<string> callback) => OnSpeakerChangedCallback += callback;
        public void UnsubscribeFromSpeakerChanged(Action<string> callback) => OnSpeakerChangedCallback -= callback;

        public void SubscribeToExternalCallHangup(Action callback) => OnExternalCallHangupCallback += callback;
        public void UnsubscribeFromExternalCallHangup(Action callback) => OnExternalCallHangupCallback -= callback;


        public void SubscribeToWebcamRequested(Action<string, Action, Action> callback) => OnWebcamRequestedCallback += callback;
        public void UnsubscribeFromWebcamRequested(Action<string, Action, Action> callback) => OnWebcamRequestedCallback -= callback;

        public void SubscribeToMicRequested(Action<string, Action, Action> callback) => OnMicRequestedCallback += callback;
        public void UnsubscribeFromMicRequested(Action<string, Action, Action> callback) => OnMicRequestedCallback -= callback;

        public void SubscribeToExternalCallStarted(Action callback)
        {
            OnExternalCallStartedCallback += callback;
        }
        public void UnsubscribeFromExternalCallStarted(Action callback)
        {
            OnExternalCallStartedCallback -= callback;
        }

        public void SubscribeToExternalCallRinging(Action callback)
        {
            OnExternalCallRingingCallback += callback;
        }
        public void UnsubscribeFromExternalCallRinging(Action callback)
        {
            OnExternalCallRingingCallback -= callback;
        }

        public void SubscribeToPausedAllStreams(Action<string> callback)
        {
            OnPausedAllStreamsCallback += callback;
        }

        public void UnsubscribeFromPausedAllStreams(Action<string> callback)
        {
            OnPausedAllStreamsCallback -= callback;
        }

        public void SubscribeToResumedAllStreams(Action<string> callback)
        {
            OnResumedAllStreamsCallback += callback;
        }

        public void UnsubscribeFromResumedAllStreams(Action<string> callback)
        {
            OnResumedAllStreamsCallback -= callback;
        }

        private event Action<string, string, string, bool, bool, string, string, string, string> OnMeetingJoinedCallback;
        private event Action<string, string, bool> OnMeetingLeftCallback;
        private event Action<string, string, bool> OnParticipantJoinedCallback;
        private event Action<string, string, bool> OnParticipantLeftCallback;
        private event Action<string> OnErrorCallback;
        private event Action<string> OnMeetingStateChangedCallback;

        private event Action<string, string> OnAudioDeviceChangedCallback;

        private event Action<string> OnSpeakerChangedCallback;
        private event Action OnExternalCallHangupCallback;
        private event Action OnExternalCallStartedCallback;
        private event Action OnExternalCallRingingCallback;
        private event Action<string> OnPausedAllStreamsCallback;
        private event Action<string> OnResumedAllStreamsCallback;

        private event Action<string, Action, Action> OnWebcamRequestedCallback;
        private event Action<string, Action, Action> OnMicRequestedCallback;



        private void OnMeetingJoined(string meetingId, string Id, string name, bool enabledLogs, string logEndPoint, string jwtKey, string peerId, string sessionId)
        {
            OnMeetingJoinedCallback?.Invoke(meetingId, Id, name, true, enabledLogs, logEndPoint, jwtKey, peerId, sessionId);
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

        private void OnAudioDeviceChanged(string availableDevice, string selectedDevice)
        {
            //Debug.Log($"OnAudioDeviceChanged callback");
            OnAudioDeviceChangedCallback?.Invoke(availableDevice, selectedDevice);
        }

        private void OnSelectedAudioDevice(string s)
        {
            //Debug.Log($"OnSelectedAudioDevice {s}");
        }

        private void OnSpeakerChanged(string participantId)
        {
            OnSpeakerChangedCallback?.Invoke(participantId);
        }

        private void OnExternalCallRinging()
        {
            OnExternalCallRingingCallback?.Invoke();
        }
        private void OnExternalCallStarted()
        {
            OnExternalCallStartedCallback?.Invoke();
        }
        private void OnExternalCallHangup()
        {
            OnExternalCallHangupCallback?.Invoke();
        }

        private void OnPausedAllStreams(string kind)
        {
            OnPausedAllStreamsCallback?.Invoke(kind);
        }

        private void OnResumedAllStreams(string kind)
        {
            OnResumedAllStreamsCallback?.Invoke(kind);
        }

        private void OnWebcamRequested(string participantId, AndroidJavaObject accept, AndroidJavaObject reject)
        {
            RunOnUnityMainThread(() =>
            {
                // Convert the AndroidJavaObject (Runnable) to an Action
                Action acceptAction = () => accept?.Call("run");
                Action rejectAction = () => reject?.Call("run");
                OnWebcamRequestedCallback?.Invoke(participantId, acceptAction, rejectAction);
            });
        }

        private void OnMicRequested(string participantId, AndroidJavaObject accept, AndroidJavaObject reject)
        {
            RunOnUnityMainThread(() =>
            {
                // Convert the AndroidJavaObject (Runnable) to an Action
                Action acceptAction = () => accept?.Call("run");
                Action rejectAction = () => reject?.Call("run");
                OnMicRequestedCallback?.Invoke(participantId, acceptAction, rejectAction);
            });
        }


        public static void RunOnUnityMainThread(Action action)
        {
            if (action != null)
            {
                MainThreadDispatcher.Instance.Enqueue(action);
            }
        }

    }
#endif
}
