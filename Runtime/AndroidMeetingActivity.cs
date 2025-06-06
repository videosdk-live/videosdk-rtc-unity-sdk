using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
// Callback class to receive messages from Android
namespace live.videosdk
{
#if UNITY_ANDROID
    internal sealed class AndroidMeetingActivity : IMeetingActivity
    {
        private AndroidJavaClass _pluginClass;
        private AndroidJavaObject _currentActivity;
        private IMeetingCallback _meetCallback;
        private IVideoSDKDTO _videoSdkDto;
        private AndroidJavaObject _applicationContext;
        public AndroidMeetingActivity(IMeetingCallback meetCallback, IVideoSDKDTO videoSdkDto)
        {
            _meetCallback = meetCallback;
            _videoSdkDto = videoSdkDto;
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                _currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                _applicationContext = _currentActivity.Call<AndroidJavaObject>("getApplicationContext");
                _pluginClass = new AndroidJavaClass(Meeting.packageName);
            }

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

        public void SubscribeToMeetingLeft(Action<string, string, bool> callback) => _meetCallback.SubscribeToMeetingLeft(callback);
        public void UnsubscribeFromMeetingLeft(Action<string, string, bool> callback) => _meetCallback.UnsubscribeFromMeetingLeft(callback);

        public void SubscribeToParticipantJoined(Action<string, string, bool> callback) => _meetCallback.SubscribeToParticipantJoined(callback);
        public void UnsubscribeFromParticipantJoined(Action<string, string, bool> callback) => _meetCallback.UnsubscribeFromParticipantJoined(callback);

        public void SubscribeToParticipantLeft(Action<string, string, bool> callback) => _meetCallback.SubscribeToParticipantLeft(callback);
        public void UnsubscribeFromParticipantLeft(Action<string, string, bool> callback) => _meetCallback.UnsubscribeFromParticipantLeft(callback);

        public void SubscribeToMeetingStateChanged(Action<string> callback) => _meetCallback.SubscribeToMeetingStateChanged(callback);
        public void UnsubscribeFromMeetingStateChanged(Action<string> callback) => _meetCallback.UnsubscribeFromMeetingStateChanged(callback);

        public void SubscribeToError(Action<string> callback) => _meetCallback.SubscribeToError(callback);
        public void UnsubscribeFromError(Action<string> callback) => _meetCallback.UnsubscribeFromError(callback);

        public void SubscribeToAudioDeviceChanged(Action<string, string> callback) => _meetCallback.SubscribeToAudioDeviceChanged(callback);
        public void UnsubscribeFromAudioDeviceChanged(Action<string, string> callback) => _meetCallback.UnsubscribeFromAudioDeviceChanged(callback);


        public void SubscribeToSpeakerChanged(Action<string> callback) => _meetCallback.SubscribeToSpeakerChanged(callback);
        public void UnsubscribeFromSpeakerChanged(Action<string> callback) => _meetCallback.UnsubscribeFromSpeakerChanged(callback);

        public void SubscribeToExternalCallRinging(Action callback) => _meetCallback.SubscribeToExternalCallRinging(callback);
        public void UnsubscribeFromExternalCallRinging(Action callback) => _meetCallback.UnsubscribeFromExternalCallRinging(callback);

        public void SubscribeToExternalCallStarted(Action callback) => _meetCallback.SubscribeToExternalCallStarted(callback);
        public void UnsubscribeFromExternalCallStarted(Action callback) => _meetCallback.UnsubscribeFromExternalCallStarted(callback);

        public void SubscribeToExternalCallHangup(Action callback) => _meetCallback.SubscribeToExternalCallHangup(callback);
        public void UnsubscribeFromExternalCallHangup(Action callback) => _meetCallback.UnsubscribeFromExternalCallHangup(callback);

        public void SubscribeToResumedAllStreams(Action<string> callback) => _meetCallback.SubscribeToResumedAllStreams(callback);
        public void UnsubscribeFromResumedAllStreams(Action<string> callback) => _meetCallback.UnsubscribeFromResumedAllStreams(callback);

        public void SubscribeToPausedAllStreams(Action<string> callback) => _meetCallback.SubscribeToPausedAllStreams(callback);
        public void UnsubscribeFromPausedAllStreams(Action<string> callback) => _meetCallback.UnsubscribeFromPausedAllStreams(callback);


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
                    encorderConfig = new CustomVideoStream(VideoEncoderConfig.h144p_w176p);
                }

                JoinMeetingConfig joinMeetingConfig = new JoinMeetingConfig(token,
                                                                            meetingId,
                                                                            name,
                                                                            micEnable,
                                                                            camEnable,
                                                                            participantId,
                                                                            packageVersion,
                                                                            platform,
                                                                            encorderConfig);


                //Debug.Log($"join config {JsonConvert.SerializeObject(joinMeetingConfig)}");
                //_pluginClass.CallStatic("joinMeeting", _currentActivity, token, meetingId, name, micEnable, camEnable, participantId, packageVersion, platform);
                _pluginClass.CallStatic("joinMeeting", _currentActivity, JsonConvert.SerializeObject(joinMeetingConfig));
                _videoSdkDto.SendDTO("INFO", $"JoinMeeting:- MeetingId:{meetingId}");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.StackTrace);
            }

        }

        public void LeaveMeeting()
        {
            _pluginClass.CallStatic("leave");
            _videoSdkDto.SendDTO("INFO", $"LeaveMeeting");
        }

        public string GetAudioDevices()
        {
            //_videoSdkDto.SendDTO("INFO", $"GetAudioDevices");
            return _pluginClass.CallStatic<string>("getAudioDevices");
        }

        public string GetVideoDevices()
        {
            return _pluginClass.CallStatic<string>("getVideoDevices");
            //_videoSdkDto.SendDTO("INFO", $"GetVideoDevices");
        }

        public string GetSelectedAudioDevice()
        {
            return _pluginClass.CallStatic<string>("getSelectedAudioDevice");
        }

        public string GetSelectedVideoDevice()
        {
            return _pluginClass.CallStatic<string>("getSelectedVideoDevice");
        }

        public void ChangeAudioDevice(string deviceLabel)
        {
            //Debug.Log($"ChangeAudioDevice SendDTO {deviceLabel}");
            _pluginClass.CallStatic("changeAudioDevice", deviceLabel);
            _videoSdkDto.SendDTO("INFO", $"ChangeAudioDevice");
        }

        public void ChangeVideoDevice(string deviceLabel)
        {
            //Debug.Log($"ChangeVideoDevice SendDTO {deviceLabel}");
            _pluginClass.CallStatic("changeVideoDevice", deviceLabel);
            _videoSdkDto.SendDTO("INFO", $"ChangeVideoDevice");
        }

        public void SetVideoEncoderConfig(string videoConfig)
        {
            _pluginClass.CallStatic("setVideoEncoderConfig", videoConfig, _applicationContext);
            _videoSdkDto.SendDTO("INFO", $"SetVideoEncoderConfig config: {videoConfig}");
        }

        public void PauseAllStreams(string kind)
        {
            _pluginClass.CallStatic("pauseAllStreams", kind);
            _videoSdkDto.SendDTO("INFO", $"PauseAllStreams:- Kind:{kind}");
        }

        public void ResumeAllStreams(string kind)
        {
            _pluginClass.CallStatic("resumeAllStreams", kind);
            _videoSdkDto.SendDTO("INFO", $"ResumeAllStreams:- Kind:{kind}");
        }


    }

#endif
}
