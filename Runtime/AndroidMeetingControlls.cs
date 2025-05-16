using UnityEngine;

namespace live.videosdk
{
#if UNITY_ANDROID
    internal sealed class AndroidMeetingControlls : IMeetingControlls
    {
        private AndroidJavaClass _pluginClass;
        private AndroidJavaObject _applicationContext;
        private AndroidJavaObject _currentActivity;
        private IVideoSDKDTO _videoSdkDto;
        public AndroidMeetingControlls(IVideoSDKDTO videoSdkDto)
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                _currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                _applicationContext = _currentActivity.Call<AndroidJavaObject>("getApplicationContext");
                _pluginClass = new AndroidJavaClass(Meeting.packageName);
            }
           
            _videoSdkDto = videoSdkDto;
        }
        public void ToggleWebCam(bool status, string Id, string customVideoStream = null)
        {
            _pluginClass.CallStatic("toggleWebCam", status, customVideoStream,_applicationContext);
            _videoSdkDto.SendDTO("INFO", $"ToggleWebCam:- status:{status} ParticipantId:{Id}");
        }
        public void ToggleMic(bool status, string Id)
        {
            _pluginClass.CallStatic("toggleMic", status);
            _videoSdkDto.SendDTO("INFO", $"ToggleMic:- status:{status} ParticipantId:{Id}");
        }

        public void PauseStream(StreamKind kind, string Id)
        {
            string type = kind.ToString();
            _pluginClass.CallStatic("pauseStream", Id,type);
            _videoSdkDto.SendDTO("INFO", $"pauseStream:- kind:{type} ParticipantId:{Id}");
        }
        public void ResumeStream(StreamKind kind, string Id)
        {
            string type = kind.ToString();
            _pluginClass.CallStatic("resumeStream",Id ,type);
            _videoSdkDto.SendDTO("INFO", $"resumeStream:- kind:{type} ParticipantId:{Id}");
        }
    }

#endif

}
