using System;

namespace live.videosdk
{
    internal static class MeetingControllFactory
    {
        public static IMeetingControlls Create()
        {
#if UNITY_ANDROID
            return new AndroidMeetingControlls();
#elif UNITY_IOS
            return new IOSMeetingControlls();
#else
            throw new PlatformNotSupportedException("Unsupported platform");
#endif

        }
    }

}
