using System;

namespace live.videosdk
{
    internal static class UserFactory
    {
        public static IUser Create(IParticipant participantData,IMeetingControlls meetingControlls,IVideoSDKDTO videoSDK)
        {
#if UNITY_ANDROID
            return new AndroidUser(participantData, meetingControlls,videoSDK);

#elif UNITY_IOS
            return new IOSUser(participantData, meetingControlls,videoSDK);
#else
            throw new PlatformNotSupportedException("Unsupported platform");
#endif

        }
    }

}
