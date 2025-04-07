using System;

namespace live.videosdk
{
    internal static class MeetingControllFactory
    {
        public static IMeetingControlls Create(IVideoSDKDTO videoSdkDto)
        {
#if UNITY_ANDROID
            return new AndroidMeetingControlls(videoSdkDto);
#elif UNITY_IOS
            return new IOSMeetingControlls(videoSdkDto);
#else
            throw new PlatformNotSupportedException("Unsupported platform");
#endif

        }
    }

}
