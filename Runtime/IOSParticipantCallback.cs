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

        private event Action<string,string> OnStreamEnabledCallback;
        private event Action<string,string> OnStreamDisabledCallback;
        private event Action<string,byte[]> OnVideoFrameReceivedCallback;
        private event Action<string, string> OnResumeStreamCallback;
        private event Action<string, string> OnPauseStreamCallback;

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
        public void SubscribeToPauseStream(Action<string, string> callback)
        {
            OnPauseStreamCallback += callback;
        }

        public void UnsubscribeFromPauseStream(Action<string, string> callback)
        {
            OnPauseStreamCallback -= callback;
        }

        public void SubscribeToResumeStream(Action<string, string> callback)
        {
            OnResumeStreamCallback += callback;
        }

        public void UnsubscribeFromResumeStream(Action<string, string> callback)
        {
            OnResumeStreamCallback -= callback;
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnStreamEnabledDelegate(string Id, string data);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnStreamDisabledDelegate(string Id, string data);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnVideoFrameReceivedDelegate(string Id, IntPtr data, int length);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnPauseStreamDelegate(string Id, string kind);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnResumeStreamDelegate(string Id, string kind);


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

        //[AOT.MonoPInvokeCallback(typeof(OnPauseStreamDelegate))]
        //private static void OnPauseStream(string id, string kind)
        //{
        //    Instance.OnPauseStreamCallback?.Invoke(id, kind);
        //}

        //[AOT.MonoPInvokeCallback(typeof(OnResumeStreamDelegate))]
        //private static void OnResumeStream(string id, string kind)
        //{
        //    Instance.OnResumeStreamCallback?.Invoke(id, kind);
        //}
    }
#endif
}



