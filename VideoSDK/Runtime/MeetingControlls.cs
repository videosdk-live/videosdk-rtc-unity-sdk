using UnityEngine;

namespace live.videosdk
{
    internal sealed class MeetingControlls : IMeetingControlls
    {
        private AndroidJavaClass _pluginClass;
        public MeetingControlls(AndroidJavaClass pluginClass)
        {
            _pluginClass = pluginClass;
        }
        public void ToggleWebCam(bool status)
        {
            _pluginClass.CallStatic("toggleWebCam", status);

        }

        public void ToggleMic(bool status)
        {
            _pluginClass.CallStatic("toggleMic", status);
        }

        public void LeaveMeeting()
        {
            _pluginClass.CallStatic("leave");

        }

    }

}
