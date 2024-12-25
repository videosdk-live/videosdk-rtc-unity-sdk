using System;
using System.Runtime.InteropServices;

namespace live.videosdk
{
    #if UNITY_IOS
    internal sealed class IOSParticipantCallback
    {
        private static IOSParticipantCallback _instance;
        public static IOSParticipantCallback Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IOSParticipantCallback();
                }
                return _instance;
            }
        }

        private IOSParticipantCallback()
        {
            //For Singleton Pattern
        }

        static IOSParticipantCallback()
        {
            RegisterUserCallbacks(OnStreamEnabled, OnStreamDisabled, OnVideoFrameReceived);
        }

        private static event Action<string,string> OnStreamEnabledCallback;
        private static event Action<string,string> OnStreamDisabledCallback;
        private static event Action<string,string> OnVideoFrameReceivedCallback;

        public void SubscribeToStreamEnabled(Action<string,string> callback)
        {
            OnStreamEnabledCallback += callback;
        }

        public void UnsubscribeFromStreamEnabled(Action<string, string> callback)
        {
            OnStreamEnabledCallback -= callback;
        }
        public void SubscribeToStreamDisabled(Action<string, string> callback)
        {
            OnStreamDisabledCallback += callback;
        }

        public void UnsubscribeFromStreamDisabled(Action<string, string> callback)
        {
            OnStreamDisabledCallback -= callback;
        }
        public void SubscribeToFrameReceived(Action<string, string> callback)
        {
            OnVideoFrameReceivedCallback += callback;
        }

        public void UnsubscribeFromFrameReceived(Action<string, string> callback)
        {
            OnVideoFrameReceivedCallback -= callback;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnStreamEnabledDelegate(string Id, string data);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnStreamDisabledDelegate(string Id, string data);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnVideoFrameReceivedDelegate(string Id, string data);

        // Bind the delegates to native functions
        [DllImport("__Internal")]
        private static extern void RegisterUserCallbacks(
            OnStreamEnabledDelegate onStreamEnabled,
            OnStreamDisabledDelegate onStreamDisabled,
            OnVideoFrameReceivedDelegate onVideoFrameReceived
        );

        [AOT.MonoPInvokeCallback(typeof(OnStreamEnabledDelegate))]
        private static void OnStreamEnabled(string id,string jsonString)
        {
            OnStreamEnabledCallback?.Invoke(id,jsonString);
        }
        [AOT.MonoPInvokeCallback(typeof(OnStreamEnabledDelegate))]
        private static void OnStreamDisabled(string id, string jsonString)
        {
            OnStreamDisabledCallback?.Invoke(id, jsonString);
        }
        [AOT.MonoPInvokeCallback(typeof(OnVideoFrameReceivedDelegate))]
        private static void OnVideoFrameReceived(string id, string jsonString)
        {
            OnVideoFrameReceivedCallback?.Invoke(id, jsonString);
        }


    }
#endif
}



