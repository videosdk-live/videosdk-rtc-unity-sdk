using System.Runtime.InteropServices;
using UnityEngine;

namespace live.videosdk
{
#if UNITY_IOS
    internal sealed class IOSMeetingControlls : IMeetingControlls
    {
        public void ToggleWebCam(bool status,string Id)
        {
            toggleWebCam(status,Id);

        }

        public void ToggleMic(bool status,string Id)
        {
            toggleMic(status,Id);
        }

        [DllImport("__Internal")]
        private static extern void toggleWebCam(bool status,string Id);
        [DllImport("__Internal")]
        private static extern void toggleMic(bool status,string Id);

    }
#endif
}
