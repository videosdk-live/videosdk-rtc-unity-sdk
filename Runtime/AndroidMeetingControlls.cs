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


    }

#endif

}
