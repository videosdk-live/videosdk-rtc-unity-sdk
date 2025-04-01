using System.Runtime.InteropServices;
using UnityEngine;

namespace live.videosdk
{
#if UNITY_IOS
    internal sealed class IOSMeetingControlls : IMeetingControlls
    {
        private IVideoSDKDTO _videoSdkDto;

        public IOSMeetingControlls(IVideoSDKDTO videoSdkDto)
        {
             _videoSdkDto = videoSdkDto;
        }

        public void ToggleWebCam(bool status,string Id)
        {
            toggleWebCam(status,Id);
            _videoSdkDto.SendDTO("INFO", $"ToggleWebCam:- status:{status} ParticipantId:{Id}");
        }

        public void ToggleMic(bool status,string Id)
        {
            toggleMic(status,Id);
             _videoSdkDto.SendDTO("INFO", $"ToggleMic:- status:{status} ParticipantId:{Id}");
        }

        public void PauseStream(StreamKind kind, string Id)
        {
            string type = kind.ToString();
            pauseStream(Id,type);
            _videoSdkDto.SendDTO("INFO", $"pauseStream:- kind:{type} ParticipantId:{Id}");
        }
        public void ResumeStream(StreamKind kind, string Id)
        {
            string type = kind.ToString();
            pauseStream(Id,type);
            _videoSdkDto.SendDTO("INFO", $"resumeStream:- kind:{type} ParticipantId:{Id}");
        }


        [DllImport("__Internal")]
        private static extern void toggleWebCam(bool status,string Id);
        [DllImport("__Internal")]
        private static extern void toggleMic(bool status,string Id);
        [DllImport("__Internal")]
        private static extern void pauseStream(string kind,string Id);
        [DllImport("__Internal")]
        private static extern void resumeStream(string kind, string Id);

    }
#endif
}
