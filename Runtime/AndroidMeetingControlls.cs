using UnityEngine;

namespace live.videosdk
{
#if UNITY_ANDROID
    internal sealed class AndroidMeetingControlls : IMeetingControlls
    {
        private AndroidJavaClass _pluginClass;
        private IVideoSDKDTO _videoSdkDto;
        public AndroidMeetingControlls(IVideoSDKDTO videoSdkDto)
        {
            _pluginClass = new AndroidJavaClass(Meeting.packageName);
            _videoSdkDto = videoSdkDto;
        }
        public void ToggleWebCam(bool status, string Id)
        {
            _pluginClass.CallStatic("toggleWebCam", status);
            _videoSdkDto.SendDTO("INFO", $"ToggleWebCam:- status:{status} ParticipantId:{Id}");
        }
        public void ToggleMic(bool status, string Id)
        {
            _pluginClass.CallStatic("toggleMic", status);
            _videoSdkDto.SendDTO("INFO", $"ToggleMic:- status:{status} ParticipantId:{Id}");
        }

        public void PauseStream(StreamKind kind, string Id)
        {
            _pluginClass.CallStatic("pauseStream", kind.ToString());
            _videoSdkDto.SendDTO("INFO", $"pauseStream:- kind:{kind} ParticipantId:{Id}");
        }
        public void ResumeStream(StreamKind kind, string Id)
        {
            _pluginClass.CallStatic("pauseStream", kind);
            _videoSdkDto.SendDTO("INFO", $"pauseStream:- kind:{kind} ParticipantId:{Id}");
        }
    }

#endif

}
