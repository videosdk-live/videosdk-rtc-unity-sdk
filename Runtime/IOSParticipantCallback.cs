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
            RegisterUserCallbacks(OnStreamEnabled, OnStreamDisabled, OnVideoFrameReceived,OnStreamPaused ,OnStreamResumed);
        }

        private event Action<string,string> OnStreamEnabledCallback;
        private event Action<string,string> OnStreamDisabledCallback;
        private event Action<string,byte[]> OnVideoFrameReceivedCallback;
        private event Action<string,string> OnStreamPausedCallback;
        private event Action<string,string> OnStreamResumedCallback;

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
        public void SubscribeToFrameReceived(Action<string, byte[]> callback)
        {
            OnVideoFrameReceivedCallback += callback;
        }

        public void UnsubscribeFromFrameReceived(Action<string, byte[]> callback)
        {
            OnVideoFrameReceivedCallback -= callback;
        }

        public void SubscribeToStreamPaused(Action<string,string> callback)
        {
            OnStreamPausedCallback += callback;
        }

        public void UnsubscribeFromStreamPaused(Action<string,string> callback)
        {
            OnStreamPausedCallback -= callback;
        }

        public void SubscribeToStreamResumed(Action<string,string> callback)
        {
            OnStreamResumedCallback += callback;
        }

        public void UnsubscribeFromStreamResumed(Action<string,string> callback)
        {
            OnStreamResumedCallback -= callback;
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnStreamEnabledDelegate(string Id, string data);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnStreamDisabledDelegate(string Id, string data);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnVideoFrameReceivedDelegate(string Id, IntPtr data, int length);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnStreamPausedDelegate(string Id, string kind);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnStreamResumedDelegate(string Id, string kind);

        // Bind the delegates to native functions
        [DllImport("__Internal")]
        private static extern void RegisterUserCallbacks(
            OnStreamEnabledDelegate onStreamEnabled,
            OnStreamDisabledDelegate onStreamDisabled,
            OnVideoFrameReceivedDelegate onVideoFrameReceived,
            OnStreamPausedDelegate onStreamPaused,
            OnStreamResumedDelegate onStreamResumed
        );

        [AOT.MonoPInvokeCallback(typeof(OnStreamEnabledDelegate))]
        private static void OnStreamEnabled(string id,string jsonString)
        {
            Instance.OnStreamEnabledCallback?.Invoke(id,jsonString);
        }
        [AOT.MonoPInvokeCallback(typeof(OnStreamEnabledDelegate))]
        private static void OnStreamDisabled(string id, string jsonString)
        {
            Instance.OnStreamDisabledCallback?.Invoke(id, jsonString);
        }
        [AOT.MonoPInvokeCallback(typeof(OnVideoFrameReceivedDelegate))]
        private static void OnVideoFrameReceived(string id, IntPtr data, int length)
        {
            byte[] frameBytes = new byte[length];
            Marshal.Copy(data, frameBytes, 0, length);
            Instance.OnVideoFrameReceivedCallback?.Invoke(id, frameBytes);
        }
        [AOT.MonoPInvokeCallback(typeof(OnStreamResumedDelegate))]
        private static void OnStreamResumed(string id, string kind)
        {
            Instance.OnStreamResumedCallback?.Invoke(id,kind);
        }
        [AOT.MonoPInvokeCallback(typeof(OnStreamPausedDelegate))]
        private static void OnStreamPaused(string id, string kind)
        {
            Instance.OnStreamPausedCallback?.Invoke(id,kind);
        }

    }
#endif
}



