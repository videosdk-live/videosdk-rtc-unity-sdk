using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
// Callback class to receive messages from Android
namespace live.videosdk
{

#if UNITY_IOS
    internal sealed class IOSMeetingActivity : IMeetingActivity
    {
        private IMeetingCallback _meetCallback;
        private IVideoSDKDTO _videoSdkDto;
        public IOSMeetingActivity(IMeetingCallback meetCallback, IVideoSDKDTO videoSdkDto)
        {
            _meetCallback = meetCallback;
            _videoSdkDto = videoSdkDto;
        }

    #region meet-events

        // Public methods to subscribe and unsubscribe to events
        public void SubscribeToMeetingJoined(Action<string, string, string, bool, bool, string, string, string, string> callback)
        {
            _meetCallback.SubscribeToMeetingJoined(callback);
        }

        public void UnsubscribeFromMeetingJoined(Action<string, string, string, bool, bool, string, string, string, string> callback)
        {
            _meetCallback.UnsubscribeFromMeetingJoined(callback);
        }

        public void SubscribeToMeetingLeft(Action<string, string, bool> callback)
        {
            _meetCallback.SubscribeToMeetingLeft(callback);
        }

        public void UnsubscribeFromMeetingLeft(Action<string, string, bool> callback)
        {
            _meetCallback.UnsubscribeFromMeetingLeft(callback);
        }

        public void SubscribeToParticipantJoined(Action<string, string, bool> callback)
        {
            _meetCallback.SubscribeToParticipantJoined(callback);
        }

        public void UnsubscribeFromParticipantJoined(Action<string, string, bool> callback)
        {
            _meetCallback.UnsubscribeFromParticipantJoined(callback);
        }

        public void SubscribeToParticipantLeft(Action<string, string, bool> callback)
        {
            _meetCallback.SubscribeToParticipantLeft(callback);
        }

        public void UnsubscribeFromParticipantLeft(Action<string, string, bool> callback)
        {
            _meetCallback.UnsubscribeFromParticipantLeft(callback);
        }

        public void SubscribeToMeetingStateChanged(Action<string> callback)
        {
            _meetCallback.SubscribeToMeetingStateChanged(callback);
        }

        public void UnsubscribeFromMeetingStateChanged(Action<string> callback)
        {
            _meetCallback.UnsubscribeFromMeetingStateChanged(callback);
        }

        public void SubscribeToError(Action<string> callback)
        {
            _meetCallback.SubscribeToError(callback);
        }

        public void UnsubscribeFromError(Action<string> callback)
        {
            _meetCallback.UnsubscribeFromError(callback);
        }

        public void SubscribeToAudioDeviceChanged(Action<string, string> callback)
        {
            _meetCallback.SubscribeToAudioDeviceChanged(callback);
        }

        public void UnsubscribeFromAudioDeviceChanged(Action<string, string> callback)
        {
            _meetCallback.UnsubscribeFromAudioDeviceChanged(callback);
        }

        public void SubscribeToSpeakerChanged(Action<string> callback)
        {
            _meetCallback.SubscribeToSpeakerChanged(callback);
        }

        public void UnsubscribeFromSpeakerChanged(Action<string> callback)
        {
            _meetCallback.UnsubscribeFromSpeakerChanged(callback);
        }

        public void SubscribeToExternalCallRinging(Action callback)
        {
            _meetCallback.SubscribeToExternalCallRinging(callback);
        }

        public void UnsubscribeFromExternalCallRinging(Action callback)
        {
            _meetCallback.UnsubscribeFromExternalCallRinging(callback);
        }

        public void SubscribeToExternalCallStarted(Action callback)
        {
            _meetCallback.SubscribeToExternalCallStarted(callback);
        }

        public void UnsubscribeFromExternalCallStarted(Action callback)
        {
            _meetCallback.UnsubscribeFromExternalCallStarted(callback);
        }

        public void SubscribeToExternalCallHangup(Action callback)
        {
            _meetCallback.SubscribeToExternalCallHangup(callback);
        }

        public void UnsubscribeFromExternalCallHangup(Action callback)
        {
            _meetCallback.UnsubscribeFromExternalCallHangup(callback);
        }

        public void SubscribeToPausedAllStreams(Action<string> callback)
        {
            _meetCallback.SubscribeToPausedAllStreams(callback);
        }

        public void UnsubscribeFromPausedAllStreams(Action<string> callback)
        {
            _meetCallback.UnsubscribeFromPausedAllStreams(callback);
        }

        public void SubscribeToResumedAllStreams(Action<string> callback)
        {
            _meetCallback.SubscribeToResumedAllStreams(callback);
        }

        public void UnsubscribeFromResumedAllStreams(Action<string> callback)
        {
            _meetCallback.UnsubscribeFromResumedAllStreams(callback);
        }

        public void SubscribeToWebcamRequested(Action<string, Action, Action> callback) => _meetCallback.SubscribeToWebcamRequested(callback);
        public void UnsubscribeFromWebcamRequested(Action<string, Action, Action> callback) => _meetCallback.UnsubscribeFromWebcamRequested(callback);

        public void SubscribeToMicRequested(Action<string, Action, Action> callback) => _meetCallback.SubscribeToMicRequested(callback);
        public void UnsubscribeFromMicRequested(Action<string, Action, Action> callback) => _meetCallback.UnsubscribeFromMicRequested(callback);


    #endregion

        public void CreateMeetingId(string jsonResponse, string token, Action<string> onSuccess)
        {
            try
            {
                //Debug.LogError("Meet Response : " + jsonResponse);
                JObject result = JObject.Parse(jsonResponse);
                var meetingId = result["roomId"].ToString();
                onSuccess?.Invoke(meetingId);
            }
            catch (JsonReaderException ex)
            {
                Debug.LogError($"Json respose is Invalid {ex.Message}");
            }

        }

        public void JoinMeeting(string token, string jsonResponse, string name, bool micEnable, bool camEnable, string participantId, string packageVersion, CustomVideoStream encorderConfig)
        {
            try
            {
                JObject result = JObject.Parse(jsonResponse);

                var meetingId = result["meetingId"].ToString();
                string platform = "Unity-" + Application.platform.ToString();

                if (encorderConfig == null && camEnable)
                {
                    encorderConfig = new CustomVideoStream(VideoEncoderConfig.h90p_w160p);
                }

                string encoderConfigJsonStr = JsonConvert.SerializeObject(encorderConfig);

                joinMeeting(token, meetingId, name, micEnable, camEnable, participantId, packageVersion, platform, encoderConfigJsonStr);
                _videoSdkDto.SendDTO("INFO", $"JoinMeeting:- MeetingId:{meetingId}");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.StackTrace);
            }

        }

        public void LeaveMeeting()
        {
            leave();
            _videoSdkDto.SendDTO("INFO", $"LeaveMeeting");
        }

        public void SetVideoEncoderConfig(string videoConfig)
        {
            setVideoEncoderConfig(videoConfig);
            _videoSdkDto.SendDTO("INFO", $"SetVideoEncoderConfig config: {videoConfig}");
        }

        public void PauseAllStreams(string kind)
        {
            pauseAllStreams(kind);
            _videoSdkDto.SendDTO("INFO", $"PauseAllStreams:- Kind:{kind}");
        }

        public void ResumeAllStreams(string kind)
        {
            resumeAllStreams(kind);
            _videoSdkDto.SendDTO("INFO", $"ResumeAllStreams:- Kind:{kind}");
        }

        public string GetAudioDevices()
        {
            return getAudioDevices();
        }

        public string GetVideoDevices()
        {
            return getVideoDevices();
        }

        public string GetSelectedAudioDevice()
        {
            return getSelectedAudioDevice();
        }

        public string GetSelectedVideoDevice()
        {
            return getSelectedVideoDevice();
        }

        public void ChangeAudioDevice(string deviceLabel)
        {
            changeAudioDevice(deviceLabel);
        }

        public void ChangeVideoDevice(string deviceLabel)
        {
            changeVideoDevice(deviceLabel);
        }

        [DllImport("__Internal")]
        private static extern void setVideoEncoderConfig(string config);

        [DllImport("__Internal")]
        private static extern void leave();

        [DllImport("__Internal")]
        private static extern void joinMeeting(string token, string meetingId, string name, bool micEnable, bool camEnable, string participantId, string packageVersion, string platform, string encorderConfig);

        [DllImport("__Internal")]
        private static extern void pauseAllStreams(string kind);

        [DllImport("__Internal")]
        private static extern void resumeAllStreams(string kind);

        [DllImport("__Internal")]
        private static extern string getAudioDevices();

        [DllImport("__Internal")]
        private static extern string getVideoDevices();

        [DllImport("__Internal")]
        private static extern string getSelectedAudioDevice();

        [DllImport("__Internal")]
        private static extern string getSelectedVideoDevice();

        [DllImport("__Internal")]
        private static extern void changeAudioDevice(string deviceLabel);
        [DllImport("__Internal")]
        private static extern void changeVideoDevice(string deviceLabel);


    }

#endif
}
