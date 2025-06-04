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

        public void ToggleWebCam(bool isLocal, bool status, string Id, string customVideoStream = null)
        {
            Debug.Log($"ToggleWebCam {isLocal}  {status}  {Id}");
            if (isLocal)
            {
                toggleWebCam(status, Id, customVideoStream);
                _videoSdkDto.SendDTO("INFO", $"ToggleWebCam:- status:{status} ParticipantId:{Id}");
            }
            else
            {
                toggleRemoteWebcam(Id, status);
                _videoSdkDto.SendDTO("INFO", $"ToggleRemoteWebcam:- status:{status} ParticipantId:{Id}");
            }
        }

        public void ToggleMic(bool isLocal, bool status, string Id)
        {
            if (isLocal)
            {
                toggleMic(status, Id);
                _videoSdkDto.SendDTO("INFO", $"ToggleMic:- status:{status} ParticipantId:{Id}");
            }
            else
            {
                toggleRemoteMic(Id, status);
                _videoSdkDto.SendDTO("INFO", $"ToggleRemoteMic:- status:{status} ParticipantId:{Id}");
            }
        }
        public void Remove(string Id)
        {
            removeRemoteParticipant(Id);
            _videoSdkDto.SendDTO("INFO", $"RemoveRemoteParticipant:- ParticipantId:{Id}");
        }


        public void PauseStream(StreamKind kind, string Id)
        {
            string type = kind.ToString();
            pauseStream(Id, type);
            _videoSdkDto.SendDTO("INFO", $"pauseStream:- kind:{type} ParticipantId:{Id}");
        }
        public void ResumeStream(StreamKind kind, string Id)
        {
            string type = kind.ToString();
            pauseStream(Id, type);
            _videoSdkDto.SendDTO("INFO", $"resumeStream:- kind:{type} ParticipantId:{Id}");
        }

        [DllImport("__Internal")]
        private static extern void toggleWebCam(bool status, string Id, string customVideoStream = null);
        [DllImport("__Internal")]
        private static extern void toggleMic(bool status, string Id, string customAudioStream = null);
        [DllImport("__Internal")]
        private static extern void pauseStream(string kind, string Id);
        [DllImport("__Internal")]
        private static extern void resumeStream(string kind, string Id);
        [DllImport("__Internal")]
        private static extern void toggleRemoteMic(string Id, bool micStatus);
        [DllImport("__Internal")]
        private static extern void toggleRemoteWebcam(string Id, bool micStatus);
        [DllImport("__Internal")]
        private static extern void removeRemoteParticipant(string Id);

    }
#endif
}
