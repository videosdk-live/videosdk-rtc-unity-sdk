﻿using UnityEngine;

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

        public void PauseStream(string participantId,string kind)
        {
            _pluginClass.CallStatic("pauseStream",participantId,kind);
            _videoSdkDto.SendDTO("INFO", $"PauseStream:- ParticipantId:{participantId} Kind:{kind}");
        }

        public void ResumeStream(string participantId, string kind)
        {
            _pluginClass.CallStatic("resumeStream", participantId, kind);
            _videoSdkDto.SendDTO("INFO", $"ResumeStream:- ParticipantId:{participantId} Kind:{kind}");
        }
    }

#endif

}
