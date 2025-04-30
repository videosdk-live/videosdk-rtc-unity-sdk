using System;
using UnityEngine;

namespace live.videosdk
{
#if UNITY_ANDROID
    internal abstract class AndroidParticipantCallback : AndroidJavaProxy
    {
        public AndroidParticipantCallback() : base("live.videosdk.unity.android.callbacks.ParticipantCallback") { }

        public abstract void OnStreamEnabled(string jsonString);

        public abstract void OnStreamDisabled(string jsonString);

        //public abstract void OnPauseStream(string kind);

        //public abstract void OnResumeStream(string kind);

        public abstract void OnVideoFrameReceived(string videoStream);

    }

#endif

}



