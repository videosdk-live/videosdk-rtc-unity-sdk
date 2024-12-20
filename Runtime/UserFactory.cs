using System;

namespace live.videosdk
{
    internal static class UserFactory
    {
        public static IUser Create(Participant participantData,IMeetingControlls meetingControlls)
        {
#if UNITY_ANDROID
            return new AndroidUser(participantData, meetingControlls);

#elif UNITY_IOS
            return new IOSUser(participantData, meetingControlls);
#else
            throw new PlatformNotSupportedException("Unsupported platform");
#endif

        }
    }

}
