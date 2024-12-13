using System;
using UnityEngine;

namespace live.videosdk
{
    internal abstract class ParticipantCallback : AndroidJavaProxy
    {
        public ParticipantCallback() : base("live.videosdk.unity.android.callbacks.ParticipantCallback") { }

        public abstract void OnStreamEnabled(string jsonString);

        public abstract void OnStreamDisabled(string jsonString);

        public abstract void OnVideoFrameReceived(string videoStream);

        // Utility to run actions on the Unity main thread safely
        protected void RunOnUnityMainThread(Action action)
        {
            if (action != null)
            {
                MainThreadDispatcher.Instance.Enqueue(action);
            }
        }

    }
}



