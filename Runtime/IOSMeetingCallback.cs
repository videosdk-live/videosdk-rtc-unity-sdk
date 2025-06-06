using System;
using System.Runtime.InteropServices;
namespace live.videosdk
{
#if UNITY_IOS
    internal sealed class IOSMeetingCallback : IMeetingCallback
    {
        private static IOSMeetingCallback _instance;
        public static IOSMeetingCallback Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IOSMeetingCallback();
                }
                return _instance;
            }
        }

        private IOSMeetingCallback()
        {
            //For Singleton Pattern
        }

        static IOSMeetingCallback()
        {
            RegisterMeetingCallbacks(OnMeetingJoined,
            OnMeetingLeft,
            OnParticipantJoined,
            OnParticipantLeft,
            OnMeetingStateChanged,
            OnError,
            OnSpeakerChanged,
            OnExternalCallStarted,
            OnExternalCallRinging,
            OnExternalCallHangup,
            OnPausedAllStreams,
            OnResumedAllStreams,
            OnAudioDeviceChanged,
            OnMicRequested,
            OnWebcamRequested
            );
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

        public void SubscribeToAudioDeviceChanged(Action<string, string> callback)
        {
            OnAudioDeviceChangedCallback += callback;
        }

        public void UnsubscribeFromAudioDeviceChanged(Action<string, string> callback)
        {
            OnAudioDeviceChangedCallback -= callback;
        }

        public void SubscribeToSpeakerChanged(Action<string> callback)
        {
            OnSpeakerChangedCallback += callback;
        }

        public void UnsubscribeFromSpeakerChanged(Action<string> callback)
        {
            OnSpeakerChangedCallback -= callback;
        }

        public void SubscribeToExternalCallHangup(Action callback)
        {
            OnExternalCallHangupCallback += callback;
        }
        public void UnsubscribeFromExternalCallHangup(Action callback)
        {
            OnExternalCallHangupCallback -= callback;
        }
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

        public void SubscribeToWebcamRequested(Action<string, Action, Action> callback) => OnWebcamRequestedCallback += callback;
        public void UnsubscribeFromWebcamRequested(Action<string, Action, Action> callback) => OnWebcamRequestedCallback -= callback;

        public void SubscribeToMicRequested(Action<string, Action, Action> callback) => OnMicRequestedCallback += callback;
        public void UnsubscribeFromMicRequested(Action<string, Action, Action> callback) => OnMicRequestedCallback -= callback;

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

        // Delegate definitions
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnMeetingJoinedDelegate(string meetingId, string Id, string name, bool enabledLogs, string logEndPoint, string jwtKey, string peerId, string sessionId);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnMeetingLeftDelegate(string Id, string name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnParticipantJoinedDelegate(string Id, string name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnParticipantLeftDelegate(string Id, string name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnMeetingStateChangedDelegate(string state);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnErrorDelegate(string jsonString);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnSpeakerChangedDelegate(string jsonString);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnExternalCallHangupDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnExternalCallStartedDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnExternalCallRingingDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnPausedAllStreamsDelegate(string kind);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnResumedAllStreamsDelegate(string kind);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnAudioDeviceChangedDelegate(string availableDevice, string selectedDevice);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnMicRequestedDelegate(string participantId);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnWebcamRequestedDelegate(string participantId);

        // Bind the delegates to native functions
        [DllImport("__Internal")]
        private static extern void RegisterMeetingCallbacks(
            OnMeetingJoinedDelegate onMeetingJoined,
            OnMeetingLeftDelegate onMeetingLeft,
            OnParticipantJoinedDelegate onParticipantJoined,
            OnParticipantLeftDelegate onParticipantLeft,
            OnMeetingStateChangedDelegate onMeetingStateChanged,
            OnErrorDelegate onError,
            OnSpeakerChangedDelegate onSpeakerChanged,
            OnExternalCallStartedDelegate onCallStarted,
            OnExternalCallRingingDelegate onCallRinging,
            OnExternalCallHangupDelegate onCallHangup,
            OnPausedAllStreamsDelegate OnPausedAllStreams,
            OnResumedAllStreamsDelegate OnResumedAllStreams,
            OnAudioDeviceChangedDelegate OnAudioDeviceChanged,
            OnMicRequestedDelegate OnMicRequested,
            OnWebcamRequestedDelegate OnWebcamRequested
        );

        [AOT.MonoPInvokeCallback(typeof(OnMeetingJoinedDelegate))]
        private static void OnMeetingJoined(string meetingId, string Id, string name, bool enabledLogs, string logEndPoint, string jwtKey, string peerId, string sessionId)
        {
            Instance.OnMeetingJoinedCallback?.Invoke(meetingId, Id, name, true, enabledLogs, logEndPoint, jwtKey, peerId, sessionId);
        }

        [AOT.MonoPInvokeCallback(typeof(OnMeetingLeftDelegate))]
        private static void OnMeetingLeft(string Id, string name)
        {
            Instance.OnMeetingLeftCallback?.Invoke(Id, name, true);
        }

        [AOT.MonoPInvokeCallback(typeof(OnParticipantJoinedDelegate))]
        private static void OnParticipantJoined(string Id, string name)
        {
            Instance.OnParticipantJoinedCallback?.Invoke(Id, name, false);
        }

        [AOT.MonoPInvokeCallback(typeof(OnParticipantLeftDelegate))]
        private static void OnParticipantLeft(string Id, string name)
        {
            Instance.OnParticipantLeftCallback?.Invoke(Id, name, false);
        }

        [AOT.MonoPInvokeCallback(typeof(OnMeetingStateChangedDelegate))]
        private static void OnMeetingStateChanged(string state)
        {
            Instance.OnMeetingStateChangedCallback?.Invoke(state);
        }

        [AOT.MonoPInvokeCallback(typeof(OnErrorDelegate))]
        private static void OnError(string jsonString)
        {
            Instance.OnErrorCallback?.Invoke(jsonString);
        }

        [AOT.MonoPInvokeCallback(typeof(OnSpeakerChangedDelegate))]
        private static void OnSpeakerChanged(string Id)
        {
            Instance.OnSpeakerChangedCallback?.Invoke(Id);
        }

        [AOT.MonoPInvokeCallback(typeof(OnExternalCallRingingDelegate))]
        private static void OnExternalCallRinging()
        {
            Instance.OnExternalCallRingingCallback?.Invoke();
        }
        [AOT.MonoPInvokeCallback(typeof(OnExternalCallStartedDelegate))]
        private static void OnExternalCallStarted()
        {
            Instance.OnExternalCallStartedCallback?.Invoke();
        }
        [AOT.MonoPInvokeCallback(typeof(OnExternalCallHangupDelegate))]
        private static void OnExternalCallHangup()
        {
            Instance.OnExternalCallHangupCallback?.Invoke();
        }

        [AOT.MonoPInvokeCallback(typeof(OnPausedAllStreamsDelegate))]
        private static void OnPausedAllStreams(string kind)
        {
            Instance.OnPausedAllStreamsCallback?.Invoke(kind);
        }

        [AOT.MonoPInvokeCallback(typeof(OnResumedAllStreamsDelegate))]
        private static void OnResumedAllStreams(string kind)
        {
            Instance.OnResumedAllStreamsCallback?.Invoke(kind);
        }

        [AOT.MonoPInvokeCallback(typeof(OnAudioDeviceChangedDelegate))]
        private static void OnAudioDeviceChanged(string availableDevice, string selectedDevice)
        {
            Instance.OnAudioDeviceChangedCallback?.Invoke(availableDevice, selectedDevice);
        }

        [AOT.MonoPInvokeCallback(typeof(OnMicRequestedDelegate))]
        private static void OnMicRequested(string participantId)
        {
            Instance.OnMicRequestedCallback?.Invoke(participantId,null , null);
        }

        [AOT.MonoPInvokeCallback(typeof(OnWebcamRequestedDelegate))]
        private static void OnWebcamRequested(string participantId)
        {
            Instance.OnWebcamRequestedCallback?.Invoke(participantId, null, null);
        }
    }
#endif
}
