using UnityEngine;

namespace live.videosdk
{
#if UNITY_ANDROID
    internal sealed class AndroidMeetingControlls : IMeetingControlls
    {
        private AndroidJavaClass _pluginClass;
        public AndroidMeetingControlls()
        {
            _pluginClass = new AndroidJavaClass(Meeting.packageName);
        }
        public void ToggleWebCam(bool status, string Id)
        {
           
            _pluginClass.CallStatic("toggleWebCam", status);

        }
        public void ToggleMic(bool status, string Id)
        {
            _pluginClass.CallStatic("toggleMic", status);
        }

        public void PauseStream(string participantId,string kind)
        {
            _pluginClass.CallStatic("pauseStream",participantId,kind);
        }

        public void ResumeStream(string participantId, string kind)
        {
            _pluginClass.CallStatic("resumeStream", participantId, kind);
        }
    }

#endif

}
