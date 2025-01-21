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

        public void PauseStream(string participantId, string kind)
        {
            pauseStream(participantId, kind);
            _videoSdkDto.SendDTO("INFO", $"PauseStream:- ParticipantId:{participantId} Kind:{kind}");
        }

        public void ResumeStream(string participantId, string kind)
        {
            resumeStream(participantId, kind);
            _videoSdkDto.SendDTO("INFO", $"ResumeStream:- ParticipantId:{participantId} Kind:{kind}");
        }

        [DllImport("__Internal")]
        private static extern void toggleWebCam(bool status,string Id);
        [DllImport("__Internal")]
        private static extern void toggleMic(bool status,string Id);
        [DllImport("__Internal")]
        private static extern void pauseStream(string Id,string kind);
        [DllImport("__Internal")]
        private static extern void resumeStream(string Id,string kind);

    }
#endif
}
