using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace live.videosdk
{
    internal static class MeetingActivityFactory
    {
        public static IMeetingActivity Create(IVideoSDKDTO videoSdkDto)
        {
#if UNITY_ANDROID
            return new AndroidMeetingActivity(AndroidMeetingCallback.Instance, videoSdkDto);
#elif UNITY_IOS
            return new IOSMeetingActivity(IOSMeetingCallback.Instance,videoSdkDto);
#else
            throw new PlatformNotSupportedException("Unsupported platform");
#endif

        }
    }

}
