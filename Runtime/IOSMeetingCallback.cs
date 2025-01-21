using System;
using System.Runtime.InteropServices;
namespace live.videosdk
{
#if UNITY_IOS
    internal sealed class IOSMeetingCallback:IMeetingCallback
    {
        private static IOSMeetingCallback _instance;
        public static IOSMeetingCallback Instance
        {
            get
            {
                if(_instance==null)
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
          OnError
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

        public void SubscribeToAudioDeviceChanged(Action<string, string[]> callback)
        {
            OnAudioDeviceChangedCallback += callback;
        }

        public void UnsubscribeFromAudioDeviceChanged(Action<string, string[]> callback)
        {
            OnAudioDeviceChangedCallback -= callback;
        }

        public void SubscribeToFetchAudioDevice(Action<string[]> callback)
        {
            OnFetchAudioDeviceCallback += callback;
        }

        public void UnsubscribeFromFetchAudioDevice(Action<string[]> callback)
        {
            OnFetchAudioDeviceCallback -= callback;
        }

        private static event Action<string, string, string, bool, bool , string , string , string , string > OnMeetingJoinedCallback;
        private static event Action<string, string, bool> OnMeetingLeftCallback;
        private static event Action<string, string, bool> OnParticipantJoinedCallback;
        private static event Action<string, string, bool> OnParticipantLeftCallback;
        private static event Action<string> OnErrorCallback;
        private static event Action<string> OnMeetingStateChangedCallback;
        private static event Action<string, string[]> OnAudioDeviceChangedCallback;
        private static event Action<string[]> OnFetchAudioDeviceCallback;

        // Delegate definitions
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnMeetingJoinedDelegate(string meetingId, string Id, string name,bool enabledLogs,string logEndPoint, string jwtKey, string peerId, string sessionId);

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


        // Bind the delegates to native functions
        [DllImport("__Internal")]
        private static extern void RegisterMeetingCallbacks(
            OnMeetingJoinedDelegate onMeetingJoined,
            OnMeetingLeftDelegate onMeetingLeft,
            OnParticipantJoinedDelegate onParticipantJoined,
            OnParticipantLeftDelegate onParticipantLeft,
            OnMeetingStateChangedDelegate onMeetingStateChanged,
            OnErrorDelegate onError
        );



        [AOT.MonoPInvokeCallback(typeof(OnMeetingJoinedDelegate))]
        private static void OnMeetingJoined(string meetingId, string Id, string name, bool enabledLogs,string logEndPoint, string jwtKey, string peerId, string sessionId)
        {
            OnMeetingJoinedCallback?.Invoke(meetingId, Id, name, true,enabledLogs,logEndPoint,jwtKey,peerId,sessionId);
        }

        [AOT.MonoPInvokeCallback(typeof(OnMeetingLeftDelegate))]
        private static void OnMeetingLeft(string Id, string name)
        {
            OnMeetingLeftCallback?.Invoke(Id, name, true);
        }

        [AOT.MonoPInvokeCallback(typeof(OnParticipantJoinedDelegate))]
        private static void OnParticipantJoined(string Id, string name)
        {
            OnParticipantJoinedCallback?.Invoke(Id, name, false);
        }

        [AOT.MonoPInvokeCallback(typeof(OnParticipantLeftDelegate))]
        private static void OnParticipantLeft(string Id, string name)
        {
            OnParticipantLeftCallback?.Invoke(Id, name, false);
        }

        [AOT.MonoPInvokeCallback(typeof(OnMeetingStateChangedDelegate))]
        private static void OnMeetingStateChanged(string state)
        {
            OnMeetingStateChangedCallback?.Invoke(state);
        }

        [AOT.MonoPInvokeCallback(typeof(OnErrorDelegate))]
        private static void OnError(string jsonString)
        {
            OnErrorCallback?.Invoke(jsonString);
        }

        
    }
#endif
}
