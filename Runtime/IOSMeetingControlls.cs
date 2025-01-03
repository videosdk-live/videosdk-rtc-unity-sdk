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

        public void PauseStream(string paticipantId, string kind)
        {
            pauseStream(paticipantId, kind);
        }

        public void ResumeStream(string paticipantId, string kind)
        {
            resumeStream(paticipantId, kind);
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
